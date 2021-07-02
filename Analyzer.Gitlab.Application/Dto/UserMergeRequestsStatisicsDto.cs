using System;
using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto.GitLab
{
    /// <summary>
    /// DTO для данных о мердж реквестах пользователя GitLab
    /// </summary>
    public class UserMergeRequestsStatisicsDto
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username пользователя
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Количество открытых пользователем реквестов
        /// </summary>
        public int OpenedTotal { get; set; }

        /// <summary>
        /// Количество вмердженных пользователем реквестов
        /// </summary>
        public int MergedTotal { get; set; }

        public List<DateAndIdItem> OpenedDates { get; set; }

        public List<DateAndIdItem> MergedDates { get; set; }
    }
}
