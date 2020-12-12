using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto.GitLab
{
    /// <summary>
    /// DTO для данных о мердж реквестах пользователя GitLab
    /// </summary>
    public class CommentsStatisicsDto
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
        /// Количество вмердженных пользователем реквестов
        /// </summary>
        public int TotalComments { get; set; }

        public IList<CommentsStatisicsItemDto> Items { get; set; }
    }
}
