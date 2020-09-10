using GitAnalyzer.Application.Dto.GitLab;
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

    }
}
