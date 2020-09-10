using System.Collections.Generic;

namespace GitAnalyzer.Application.Dto.GitLab
{
    /// <summary>
    /// DTO для данных о пользователях GitLab
    /// </summary>
    public class UsersDto
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username пользователя
        /// </summary>
        public string Username { get; set; }

        public string Url { get; set; }
    }


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

        /// <summary>
        /// Репозиторий
        /// </summary>
        public IList<UserMergeRequestsStatisicsItemDto> Items { get; set; }
    }

    public class UserMergeRequestsStatisicsItemDto
    {
        /// <summary>
        /// Репозиторий
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Количество открытых пользователем реквестов
        /// </summary>
        public int Opened { get; set; }

        /// <summary>
        /// Количество вмердженных пользователем реквестов
        /// </summary>
        public int Merged { get; set; }
    }
}
