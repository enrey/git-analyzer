using GitLabApiClient.Models.MergeRequests.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitAnalyzer.Application.Services.GitLab
{
    /// <summary>
    /// Сервис для взаимодействия с GitLab
    /// </summary>
    public interface IGitLabService
    {
        Task<IEnumerable<MergeRequest>> GetMergeRequests();
    }
}
