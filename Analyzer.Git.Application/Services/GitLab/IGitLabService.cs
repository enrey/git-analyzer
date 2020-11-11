using GitAnalyzer.Application.Dto.GitLab;
using GitAnalyzer.Application.Dto.Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitAnalyzer.Application.Services.GitLab
{
    /// <summary>
    /// Сервис для взаимодействия с GitLab
    /// </summary>
    public interface IGitLabService
    {
        /// <summary>
        /// Получить статистику пользователей по мерджреквестам
        /// </summary>
        Task<IEnumerable<UserMergeRequestsStatisicsDto>> GetMergeRequestsStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        Task<IEnumerable<UsersDto>> GetUsers();

        /// <summary>
        /// Получить последние коммиты репозиториев GitLab'а
        /// </summary>
        Task<IEnumerable<RepositoryLastCommitDto>> GetRepositoriesLastCommit();

    }
}
