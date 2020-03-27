using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Dto.GitLab;
using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
using Microsoft.Extensions.Options;
using System;
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

            var client = new GitLabClient(
                _gitLabConfig.BaseUrl, 
                _gitLabConfig.PrivateToken);
            
            var users = (await client.Users.GetAsync())
                .Select(u => new { u.Id, u.Email })
                .ToList();

            var repos = (await client.Projects.GetAsync())
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
                    stat.Created,
                    stat.Merged
                })
                .Join(repos, stat => stat.ProjectId, repo => repo.Id, (stat, repo) => new UserMergeRequestsStatisicsDto 
                { 
                    RepositoryName = repo.Name,
                    Email = stat.Email,
                    Opened = stat.Created,
                    Merged = stat.Merged
                })
                .ToList();

            return statistics;
        }
    }
}
