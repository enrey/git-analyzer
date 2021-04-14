using Analyzer.Git.Application.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Analyzer.Git.Application.Services
{
    /// <summary>
    /// Сервис для получения статистики из GIT репозиториев
    /// </summary>
    public interface IGitStatisticsService
    {
        /// <summary>
        /// Получить статистику по всем репозиториям за период
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<IEnumerable<RepositoryStatisticsDto>> GetAllRepositoriesStatisticsAsync(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Обновление всех репозиториев
        /// </summary>
        Task UpdateAllRepositories();

        /// <summary>
        /// Обновление репозитория
        /// </summary>
        Task UpdateRepository(string repoUrl);

        /// <summary>
        /// Суммирует часы по дням. 
        /// Предполагаем, что работа за день началась за 2 часа до первого коммита и окончилась последним коммитом
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<RepositoryWorkEstimateDto>> GetWorkSessionsEstimate(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Возвращает последние коммиты репозиториев
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<RepositoryLastCommitDto>> GetAllRepositoriesLastCommitAsync();

    }
}
