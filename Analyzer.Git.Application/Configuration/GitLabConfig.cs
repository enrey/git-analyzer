namespace GitAnalyzer.Application.Configuration
{
    /// <summary>
    /// Конфигурация для работы с GitLab
    /// </summary>
    public class GitLabConfig
    {
        /// <summary>
        /// Базовый URL
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// GIT UI URL
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// Токен авторизации
        /// </summary>
        public string PrivateToken { get; set; }
    }
}
