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
        public string BaseUrl { get; set; }

        /// <summary>
        /// Токен авторизации
        /// </summary>
        public string PrivateToken { get; set; }
    }
}
