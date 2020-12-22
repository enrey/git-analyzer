using Analyzer.Git.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Analyzer.Git.Application.Services
{
    /// <summary>
    /// Сервис для работы с API GitLabService "http://localhost:5003/api/GitLab"
    /// </summary>
    public interface IGitlabServiceClient
    {
        /// <summary>
        /// Получить списко активных репозиториев начиная с указанной даты
        /// </summary>
        Task<IEnumerable<RepositoryInfoConfig>> GetAllReposFromApi(DateTime lastActivityAfter);
    }
}
