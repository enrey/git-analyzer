using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Dto.GitLab;
using GitAnalyzer.Application.Dto.Statistics;
using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitAnalyzer.Application.Services.GitLab
{
    /// <summary>
    /// Сервис для взаимодействия с GitLab
    /// </summary>
    public class GitLabService : IGitLabService
    {
        private readonly GitLabConfig _gitLabConfig;
        private readonly RepositoriesConfig _repositoriesConfig;

        /// <summary>
        /// Сервис для взаимодействия с GitLab
        /// </summary>
        public GitLabService(
            IOptionsMonitor<GitLabConfig> gitLabOptionsMonitor,
            IOptionsMonitor<RepositoriesConfig> repositoriesOptionsMonitor)
        {
            _gitLabConfig = gitLabOptionsMonitor.CurrentValue;
            _repositoriesConfig = repositoriesOptionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Получить статистику пользователей по мерджреквестам
        /// </summary>
        public async Task<IEnumerable<UserMergeRequestsStatisicsDto>> GetMergeRequestsStatistics(DateTime startDate, DateTime endDate)
        {
            var configRepos = _repositoriesConfig.ReposInfo
                .Select(ri => new { ri.Name, Url = ri.Url.ToLower() })
                .ToList();

            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var users = (await client.Users.GetAsync())
                .Select(u => new { u.Id, u.Email, u.Username })
                .ToList();

            var projects = (await client.Projects.GetAsync());

            var repos = projects
                .Where(pr => configRepos.Select(cr => cr.Url).Contains(pr.HttpUrlToRepo.ToLower()))
                .Join(configRepos, loaded => loaded.HttpUrlToRepo, cfgRepo => cfgRepo.Url, (loaded, cfgRepo) => new
                {
                    loaded.Id,
                    cfgRepo.Name
                })
                .ToList();

            var mergeRequests = (await client.MergeRequests.GetAsync(options =>
            {
                options.State = QueryMergeRequestState.All;
                options.CreatedAfter = startDate.AddMonths(-1);
            }))
            .Select(mr => new
            {
                mr.ProjectId,
                AuthorId = mr.Author.Id,
                mr.CreatedAt,
                MergedById = mr.MergedBy?.Id,
                mr.MergedAt
            })
            .ToList();

            var creationData = mergeRequests
                .Where(mr => startDate <= mr.CreatedAt)
                .GroupBy(mr => new { mr.AuthorId, mr.ProjectId })
                .Select(gr => new
                {
                    UserId = gr.Key.AuthorId,
                    gr.Key.ProjectId,
                    Created = gr.Count()
                })
                .ToList();

            var mergeData = mergeRequests
                .Where(mr => mr.MergedById.HasValue && mr.MergedAt != null && mr.MergedAt.Value <= endDate)
                .GroupBy(mr => new { mr.MergedById, mr.ProjectId })
                .Select(gr => new
                {
                    UserId = gr.Key.MergedById.Value,
                    gr.Key.ProjectId,
                    Merged = gr.Count()
                })
                .ToList();

            var userProjectPairs = creationData
                .Select(cd => new
                {
                    cd.UserId,
                    cd.ProjectId
                })
                .Concat(
                    mergeData.Select(md => new
                    {
                        md.UserId,
                        md.ProjectId
                    })
                )
                .Distinct()
                .ToList();

            var statistics = userProjectPairs
                .Select(p => new
                {
                    p.ProjectId,
                    p.UserId,
                    Created = creationData.FirstOrDefault(cd => cd.ProjectId == p.ProjectId && cd.UserId == p.UserId)?.Created ?? 0,
                    Merged = mergeData.FirstOrDefault(cd => cd.ProjectId == p.ProjectId && cd.UserId == p.UserId)?.Merged ?? 0
                })
                .Join(users, stat => stat.UserId, user => user.Id, (stat, user) => new
                {
                    ProjectId = int.Parse(stat.ProjectId),
                    stat.UserId,
                    user.Email,
                    user.Username,
                    stat.Created,
                    stat.Merged
                })
                .Join(repos, stat => stat.ProjectId, repo => repo.Id, (stat, repo) => new
                {
                    RepositoryName = repo.Name,
                    Email = stat.Email,
                    Username = stat.Username,
                    Opened = stat.Created,
                    Merged = stat.Merged
                })
                .GroupBy(o => o.Email)
                .Select(o => new UserMergeRequestsStatisicsDto
                {
                    Email = o.Key,
                    Username = o.Select(i => i.Username).First(), // TODO: несколько реп может быть
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
            var configReposUrls = _repositoriesConfig.ReposInfo
                .Select(ri => ri.Url.ToLower())
                .ToList();

            var client = new GitLabClient(_gitLabConfig.ApiUrl, _gitLabConfig.PrivateToken);

            var projects = (await client.Projects.GetAsync())
                .Where(pr => configReposUrls.Contains(pr.HttpUrlToRepo.ToLower()))
                .Select(pr => new { pr.Id, pr.Name, pr.DefaultBranch });

            var result = new ConcurrentBag<RepositoryLastCommitDto>();

            await Task.WhenAll(projects.Select(async pr =>
            {
                var lastCommit = pr.DefaultBranch.Contains("/") ?
                    (await client.Commits.GetAsync(pr.Id))[0] :
                    await client.Commits.GetAsync(pr.Id, pr.DefaultBranch);

                result.Add(new RepositoryLastCommitDto
                {
                    RepositoryName = pr.Name,
                    RepositoryHash = lastCommit.Id,
                    RepositoryDate = lastCommit.CommittedDate
                });
            }));

            return result.OrderBy(r => r.RepositoryName).ToList();
        }
    }
}
