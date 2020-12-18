using System.Collections.Generic;

namespace Analyzer.Git.Web.Api.Dto
{
    /// <summary>
    /// Контракт статистики по сотруднику
    /// </summary>
    public class PersonStatisticsContract
    {
        /// <summary>
        /// Имя сотрудника
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Эл.почта сотрудника
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Количество коммитов
        /// </summary>
        public int Commits { get; set; }

        public IList<string> Shas { get; set; }

        /// <summary>
        /// Количество удалений
        /// </summary>
        public int Deleted { get; set; }

        /// <summary>
        /// Количество добавлений
        /// </summary>
        public int Added { get; set; }

        /// <summary>
        /// Всего
        /// </summary>
        public int Total { get; set; }

        public List<int> CommitsChurnArray { get; set; }
    }
}
