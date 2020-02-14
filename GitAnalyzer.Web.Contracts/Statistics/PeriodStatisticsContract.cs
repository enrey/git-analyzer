using System;
using System.Collections.Generic;

namespace GitAnalyzer.Web.Contracts.Statistics
{
    /// <summary>
    /// Контракт статистики за период
    /// </summary>
    public class PeriodStatisticsContract
    {
        /// <summary>
        /// Дата за которую собрана статистика
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Статистика по сотрудникам
        /// </summary>
        public IEnumerable<PersonStatisticsContract> Statistics { get; set; }
    }
}
