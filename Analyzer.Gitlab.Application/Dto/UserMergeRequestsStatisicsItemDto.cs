namespace Analyzer.Git.Application.Dto.GitLab
{
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
