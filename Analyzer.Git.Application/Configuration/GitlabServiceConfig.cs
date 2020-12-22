namespace Analyzer.Git.Application.Configuration
{
    /// <summary>
    /// Конфигурация для работы API GitLabService "http://localhost:5003/api/GitLab"
    /// </summary>
    public class GitlabServiceConfig
    {
        /// <summary>
        /// Базовый URL
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Ендпоинт для получения активных репозиториев
        /// </summary>
        public string ActiveRepositories { get; set; }
    }
}
