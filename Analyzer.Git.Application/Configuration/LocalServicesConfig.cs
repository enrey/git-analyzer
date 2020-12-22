namespace Analyzer.Git.Application.Configuration
{
    /// <summary>
    /// Конфигурация для работы с API локальных сервисов
    /// </summary>
    public class LocalServicesConfig
    {
        /// <summary>
        /// Конфигурация для работы API GitLabService "http://localhost:5003/api/GitLab"
        /// </summary>
        public GitlabServiceConfig Gitlab { get; set; }
    }
}
