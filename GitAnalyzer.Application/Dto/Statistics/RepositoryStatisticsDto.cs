using System.Collections.Generic;

namespace GitAnalyzer.Application.Dto.Statistics
{
    /// <summary>
    /// DTO статистики по репозиторию
    /// </summary>
    public class RepositoryStatisticsDto
    {
        /// <summary>
        /// Репозиторий
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Периоды статистики
        /// </summary>
        public IEnumerable<PeriodStatisticsDto> Periods { get; set; }
    }
}
