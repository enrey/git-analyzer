using GitAnalyzer.Application.Dto.GitLab;
using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
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

        /// <summary>
        /// Сервис для взаимодействия с GitLab
        /// </summary>
        public GitLabService()
        {
        }

        /// <summary>
        /// Получить статистику пользователей по мерджреквестам
        /// </summary>
        public async Task<IEnumerable<UserMergeRequestsStatisicsDto>> GetMergeRequestsStatistics(DateTime startDate, DateTime endDate)
        {
            var baseUrl = "http://git.it2g.ru/api/v4";
            var privateToken = "";

            var client = new GitLabClient(baseUrl, privateToken);

            var users = (await client.Users.GetAsync())
                .Select(u => new { u.Id, u.Email })
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
                    stat.ProjectId, 
                    stat.UserId,
                    user.Email,
                    stat.Created,
                    stat.Merged
                })
                .ToList();


            return null;
        }
    }
}
