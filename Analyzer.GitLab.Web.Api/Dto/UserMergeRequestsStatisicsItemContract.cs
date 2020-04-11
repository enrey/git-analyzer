namespace GitAnalyzer.Web.Contracts.GitLab
{
    /// <summary>
    /// Контракт данных о статистике мерджреквестов пользователя
    /// </summary>
    public class UserMergeRequestsStatisicsItemContract
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
