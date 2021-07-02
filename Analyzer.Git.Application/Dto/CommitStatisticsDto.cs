using System;

namespace Analyzer.Git.Application.Dto
{
    /// <summary>
    /// DTO статистики по сотруднику
    /// </summary>
    public class CommitStatisticsDto
    {
        /// <summary>
        /// Имя сотрудника
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Эл.почта сотрудника
        /// </summary>
        public string Email { get; set; }

        public string Sha { get; set; }

        public string Message { get; set; }

        public DateTime CommitDate { get; set; }

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
    }
}
