using System.Collections.Generic;

namespace Analyzer.Git.Web.Api.Dto
{
    /// <summary>
    /// Контракт статистики по репозиторию
    /// </summary>
    public class RepositoryStatisticsContract
    {
        /// <summary>
        /// Репозиторий
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// UI для фронта
        /// </summary>
        public string WebUI { get; set; }

        /// <summary>
        /// Периоды статистики
        /// </summary>
        public IEnumerable<PeriodStatisticsContract> Periods { get; set; }
    }
}
