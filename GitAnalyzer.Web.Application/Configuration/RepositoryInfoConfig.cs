namespace GitAnalyzer.Web.Application.Configuration
{
    /// <summary>
    /// Конфигурация репозитория
    /// </summary>
    public class RepositoryInfoConfig
    {
        /// <summary>
        /// Имя репозитория
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Url удаленного репозитория 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Локальная папка для репозитория
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }
    }
}
