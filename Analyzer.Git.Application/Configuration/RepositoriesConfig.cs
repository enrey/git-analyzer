using System.Collections.Generic;

namespace GitAnalyzer.Application.Configuration
{
    public class RepositoriesConfig
    {
        /// <summary>
        /// Токен для глобальной авторизации в гитлабе
        /// </summary>
        public string GitlabAuthToken { get; set; }

        /// <summary>
        /// Папка размещения репозиториев
        /// </summary>
        public string ReposFolder { get; set; }

        /// <summary>
        /// Имя пользователя для мердж-коммита
        /// </summary>
        public string MergeUserName { get; set; }

        /// <summary>
        /// Email пользователя для мердж-коммита
        /// </summary>
        public string MergeUserEmail { get; set; }

        /// <summary>
        /// Период обновления репозиториев (в минутах)
        /// </summary>
        public int ReposUpdatePeriodMinutes { get; set; }

        /// <summary>
        /// Конфигурации репозиториев
        /// </summary>
        public IEnumerable<RepositoryInfoConfig> ReposInfo { get; set; }
    }
}
