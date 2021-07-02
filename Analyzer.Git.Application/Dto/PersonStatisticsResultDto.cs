using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto
{
    public class PersonStatisticsResultDto
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
        public string Date { get; set; }    

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
        public int CommitsCount { get; set; }

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

        public List<PersonStatisticsCommitResultDto> CommitsArray { get; set; }
    }
}
