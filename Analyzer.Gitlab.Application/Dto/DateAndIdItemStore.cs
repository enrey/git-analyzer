using System;

namespace Analyzer.Git.Application.Dto.GitLab
{
    public class DateAndIdItemStore
    {
        public EventType EventType { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username пользователя
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Заполняется только для типа Comment
        /// </summary>
        public string Comment { get; set; }

        public string Title { get; set; }

        public DateTime Dt { get; set; }

        public string Url { get; set; }

        public int Iid { get; set; }

        public string Repo { get; set; }
    }
}
