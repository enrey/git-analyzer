using GitAnalyzer.Web.Application.Dto.Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitAnalyzer.Web.Application.Services.Statistics
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
    }
}
