using System.Collections.Generic;

namespace GitAnalyzer.Web.Contracts.Statistics
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
        /// Периоды статистики
        /// </summary>
        public IEnumerable<PeriodStatisticsContract> Periods { get; set; }
    }
}
