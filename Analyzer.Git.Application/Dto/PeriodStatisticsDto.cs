using System;
using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto
{
    /// <summary>
    /// DTO статистики за период
    /// </summary>
    public class PeriodStatisticsDto
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
        /// Дата за которую собрана статистика
        /// </summary>
        public string Date { get; set; } // .ToString("yyyy-MM-dd")

        /// <summary>
        /// Статистика по сотрудникам
        /// </summary>
        public IEnumerable<CommitStatisticsDto> Statistics { get; set; }
    }
}
