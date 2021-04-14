using System.Collections.Generic;

namespace Analyzer.Git.Web.Api.Dto
{
    /// <summary>
    /// Контракт статистики за период
    /// </summary>
    public class PeriodStatisticsContract
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
        /// Дата за которую собрана статистика
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Статистика по сотрудникам
        /// </summary>
        public IEnumerable<PersonStatisticsContract> Statistics { get; set; }
    }
}
