namespace GitAnalyzer.Application.Dto.GitLab
{
    /// <summary>
    /// DTO для данных о мердж реквестах пользователя GitLab
    /// </summary>
    public class UserMergeRequestsStatisicsDto
    {
        /// <summary>
        /// Репозиторий
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

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
