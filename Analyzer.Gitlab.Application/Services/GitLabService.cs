using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Dto;
using Analyzer.Git.Application.Dto.GitLab;
using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.Projects.Requests;
using GitLabApiClient.Models.Projects.Responses;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Analyzer.Git.Application.Services.GitLab
{
    /// <summary>
    /// Сервис для взаимодействия с GitLab
    /// </summary>
    public class GitLabService : IGitLabService
    {
        private const int MAX_DEGREE_OF_PARALLELISM = 5;
        private const string TYPE_COMMENT = "DiffNote";
        private readonly GitLabConfig _gitLabConfig;

        /// <summary>
        /// Сервис для взаимодействия с GitLab
        /// </summary>
        public GitLabService(
            IOptionsMonitor<GitLabConfig> gitLabOptionsMonitor)
        {
            _gitLabConfig = gitLabOptionsMonitor.CurrentValue;
        }

        public async Task<IEnumerable<CommentsStatisicsDto>> GetMergeRequestsCommentsStatistics(DateTime startDate, DateTime endDate)
        {
            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var usersAll = (await client.Users.GetAsync())
                .Where(o => o.LastActivityOn != null)
                .Select(u => new { u.Id, u.Email, u.Username, LastActivityOn = DateTime.Parse(u.LastActivityOn) })
                .ToList();

            var usersActive = usersAll.Where(o => o.LastActivityOn >= startDate).OrderByDescending(o => o.LastActivityOn).ToList();

            var result = usersActive.AsParallel()
                .WithDegreeOfParallelism(MAX_DEGREE_OF_PARALLELISM)
                .Select(o => getItem(o.Id, o.Username, o.Email, startDate, endDate).Result)
                .ToList();

            return result;
        }

        private async Task<CommentsStatisicsDto> getItem(int userId, string username, string useremail, DateTime startDate, DateTime endDate)
        {
            HttpClient cl = new HttpClient();
            // TODO: Убрать копипасту с gitlabclient
            cl.DefaultRequestHeaders.Add("PRIVATE-TOKEN", _gitLabConfig.PrivateToken);

            var response = await cl.GetAsync(_gitLabConfig.ApiUrl + "/users/" + userId + "/events?after=" + startDate + "&per_page=1000");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            dynamic results = JsonConvert.DeserializeObject<dynamic>(responseBody);

            var lst = new List<CommentsStatisicsItemDto>();

            foreach (var a in results)
            {
                if (a.target_type.ToString() == TYPE_COMMENT)
                {
                    var item = new CommentsStatisicsItemDto { Username = a.note.author.username, Dt = DateTime.Parse(a.note.created_at.ToString()), Comment = a.note.body.ToString() };
                    if (item.Dt >= startDate && item.Dt <= endDate)
                    {
                        lst.Add(item);
                    }
                }
            }

            cl.Dispose();

            return new CommentsStatisicsDto
            {
                Items = lst,
                TotalComments = lst.Count,
                Username = username,
                Email = useremail
            };
        }

        /// <summary>
        /// Получить статистику пользователей по мерджреквестам
        /// </summary>
        public async Task<IEnumerable<UserMergeRequestsStatisicsDto>> GetMergeRequestsStatistics(DateTime startDate, DateTime endDate)
        {
            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var users = (await client.Users.GetAsync())
                .Select(u => new { u.Id, u.Email, u.Username })
                .ToList();

            var projects = await client.Projects.GetAsync();

            var repos = projects
                .Select(loaded => new
                {
                    loaded.Id,
                    Name = loaded.HttpUrlToRepo,
                    Url = loaded.WebUrl
                })
                .ToList();

            var mergeRequests = (await client.MergeRequests.GetAsync(options =>
            {
                options.State = QueryMergeRequestState.All;
                options.CreatedAfter = startDate.AddMonths(-1); //TODO: WTF
            }))
            .Select(mr => new
            {
                mr.ProjectId,
                AuthorId = mr.Author.Id,
                mr.CreatedAt,
                MergedById = mr.MergedBy?.Id,
                mr.MergedAt,
                mr.Iid
            })
            .ToList();

            var creationData = mergeRequests
                .Where(mr => mr.CreatedAt >= startDate && mr.CreatedAt <= endDate)
                .GroupBy(mr => new { mr.AuthorId, mr.ProjectId })
                .Select(gr => new
                {
                    UserId = gr.Key.AuthorId,
                    gr.Key.ProjectId,
                    Created = gr.Count(),
                    CreatedDates = gr.Select(x => new DateAndIdItem { Iid = x.Iid, Dt = x.CreatedAt }).ToList()
                })
                .ToList();

            var mergeData = mergeRequests
                .Where(mr => mr.MergedById.HasValue && mr.MergedAt != null && mr.MergedAt.Value >= startDate && mr.MergedAt.Value <= endDate)
                .GroupBy(mr => new { mr.MergedById, mr.ProjectId })
                .Select(gr => new
                {
                    UserId = gr.Key.MergedById.Value,
                    gr.Key.ProjectId,
                    Merged = gr.Count(),
                    MergedDates = gr.Select(x => new DateAndIdItem { Iid = x.Iid, Dt = x.MergedAt.Value }).ToList()
                })
                .ToList();

            var userProjectPairs = creationData
                .Select(cd => new
                {
                    cd.UserId,
                    cd.CreatedDates,
                    MergedDates = new List<DateAndIdItem>(),
                    cd.ProjectId
                })
                .Concat(
                    mergeData.Select(md => new
                    {
                        md.UserId,
                        CreatedDates = new List<DateAndIdItem>(),
                        md.MergedDates,
                        md.ProjectId
                    })
                )
                .GroupBy(o => new { o.UserId, o.ProjectId })//.Distinct()
                .Select(o => new
                {
                    o.Key.UserId,
                    o.Key.ProjectId,
                    MergedDates = o.SelectMany(i => i.MergedDates).ToList(),
                    CreatedDates = o.SelectMany(i => i.CreatedDates).ToList(),
                })
                .ToList();

            const string URL_SUFFIX = "/-/merge_requests/";

            var statistics = userProjectPairs
                .Select(p => new
                {
                    p.ProjectId,
                    p.UserId,
                    Created = creationData.FirstOrDefault(cd => cd.ProjectId == p.ProjectId && cd.UserId == p.UserId)?.Created ?? 0,
                    Merged = mergeData.FirstOrDefault(cd => cd.ProjectId == p.ProjectId && cd.UserId == p.UserId)?.Merged ?? 0,
                    CreatedDates = p.CreatedDates,
                    MergedDates = p.MergedDates
                })
                .Join(users, stat => stat.UserId, user => user.Id, (stat, user) => new
                {
                    ProjectId = int.Parse(stat.ProjectId),
                    stat.UserId,
                    user.Email,
                    user.Username,
                    stat.Created,
                    stat.Merged,
                    stat.CreatedDates,
                    stat.MergedDates
                })
                .Join(repos, stat => stat.ProjectId, repo => repo.Id, (stat, repo) => new
                {
                    RepositoryName = repo.Name,
                    stat.Email,
                    stat.Username,
                    Opened = stat.Created,
                    stat.Merged,
                    OpenedDates = new { repo.Name, repo.Url, stat.CreatedDates },
                    MergedDates = new { repo.Name, repo.Url, stat.MergedDates }
                })
                .GroupBy(o => o.Email)
                .Select(o => new UserMergeRequestsStatisicsDto
                {
                    Email = o.Key,
                    Username = o.Select(i => i.Username).First(), // TODO: несколько реп может быть
                    OpenedDates = o.Select(o => o.OpenedDates).SelectMany(_ => _.CreatedDates.Select(i => new DateAndIdItem { Dt = i.Dt, Url = _.Url + URL_SUFFIX + i.Iid, Iid = i.Iid, Repo = _.Name })).ToList(),
                    MergedDates = o.Select(o => o.MergedDates).SelectMany(_ => _.MergedDates.Select(i => new DateAndIdItem { Dt = i.Dt, Url = _.Url + URL_SUFFIX + i.Iid, Iid = i.Iid, Repo = _.Name })).ToList(),
                    OpenedTotal = o.Sum(i => i.Opened),
                    MergedTotal = o.Sum(i => i.Merged),
                    Items = o.Select(i => new UserMergeRequestsStatisicsItemDto { Merged = i.Merged, Opened = i.Opened, RepositoryName = i.RepositoryName }).ToList()
                })
                .ToList();

            return statistics;
        }

        public async Task<IEnumerable<UsersDto>> GetUsers()
        {
            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var users = (await client.Users.GetAsync())
                .Select(u => new UsersDto { Email = u.Email, Username = u.Username, Url = _gitLabConfig.WebUrl + u.Username })
                .ToList();

            return users;
        }


        /// <summary>
        /// Получить последние коммиты репозиториев GitLab'а
        /// </summary>
        public async Task<IEnumerable<RepositoryLastCommitDto>> GetRepositoriesLastCommit()
        {
            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var projects = (await client.Projects.GetAsync())
                .Select(pr => new { pr.Id, pr.Name, pr.HttpUrlToRepo });

            var result = new ConcurrentBag<RepositoryLastCommitDto>();

            await Task.WhenAll(projects.Select(async pr =>
            {
                var lastCommit = (await client.Commits.GetAsync(pr.Id))[0];

                result.Add(new RepositoryLastCommitDto
                {
                    RepositoryName = pr.Name,
                    RepositoryUrl = pr.HttpUrlToRepo,
                    Hash = lastCommit.Id,
                    Date = lastCommit.CommittedDate
                });
            }));

            return result.OrderBy(r => r.RepositoryName).ToList();
        }

        /// <summary>
        /// Получить активные репозитории GitLab'а
        /// </summary>
        public async Task<IEnumerable<RepositoryParameters>> GetActiveRepositories(DateTime sinceDate)
        {
            Action<ProjectQueryOptions> queryOptionsDelegate;

            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            queryOptionsDelegate = options =>
            {
                options.LastActivityAfter = sinceDate;
                options.Order = ProjectsOrder.LastActivityAt;
            };

            var projects = (await client.Projects.GetAsync(queryOptionsDelegate))
                .Select(rp => new RepositoryParameters() 
                {
                    Name = GenerateRepoNameByUrl(rp.WebUrl),
                    WebUI = rp.WebUrl,
                    RepoPath = GenerateLocalPathNameByUrl(rp.WebUrl)
                });

            return new List<RepositoryParameters>(projects.OrderBy(r => r.WebUI));
        }

        /// <summary>
        /// Сгенерировать имя репозитория
        /// </summary>
        /// <returns></returns>
        private string GenerateRepoNameByUrl(string url)
        {
            var arr = url.Split("/");

            if (arr.Length < 2)
                return url;

            if (arr[^2] == arr[^1])
                return arr.Last();

            return $"{arr[^2]}_{arr[^1]}";
        }

        /// <summary>
        /// Сгенерировать имя репозитория
        /// </summary>
        /// <returns></returns>
        private string GenerateLocalPathNameByUrl(string url)
        {
            var arr = url.Split("/");

            if (arr.Length < 2)
                return url;

            return arr.Last();
        }
    }
}
