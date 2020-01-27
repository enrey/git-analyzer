using System.Collections.Generic;

namespace GitAnalyzer.Web.Application.Configuration
{
    public class RepositoriesConfig
    {
        /// <summary>
        /// Папка размещения репозиториев
        /// </summary>
        public string ReposFolder { get; set; }

        /// <summary>
        /// Конфигурации репозиториев
        /// </summary>
        public IEnumerable<RepositoryInfoConfig> ReposInfo { get; set; }
    }
}
