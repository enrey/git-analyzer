using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto
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
        /// URL для UI
        /// </summary>
        public string WebUI { get; set; }

        /// <summary>
        /// Периоды статистики
        /// </summary>
        public IEnumerable<PeriodStatisticsDto> Periods { get; set; }
    }
}
