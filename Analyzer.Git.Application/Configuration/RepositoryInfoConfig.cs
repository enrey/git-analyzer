using System.Linq;

namespace Analyzer.Git.Application.Configuration
{
    /// <summary>
    /// Конфигурация репозитория
    /// </summary>
    public class RepositoryInfoConfig
    {

        /// <summary>
        /// Url удаленного репозитория 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// UI для фронта
        /// </summary>
        public string WebUI { get; set; }

        /// <summary>
        /// Имя репозитория
        /// </summary>
        public string Name
        {
            get => GenerateRepoNameByWebUI();
        }

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

        private string GenerateRepoNameByWebUI()
        {
            var arr = WebUI.Split("/");

            if (arr.Length < 2)
                return WebUI;

            if (arr[^2] == arr[^1])
                return arr.Last();

            return $"{arr[^2]}_{arr[^1]}";
        }
    }
}
