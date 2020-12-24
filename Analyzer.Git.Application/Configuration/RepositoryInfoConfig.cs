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
    }
}
