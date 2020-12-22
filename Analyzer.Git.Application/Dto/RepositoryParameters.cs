namespace Analyzer.Git.Application.Dto
{
    /// <summary>
    /// Параметры репозитория
    /// </summary>
    public class RepositoryParameters
    {
        /// <summary>
        /// Имя репозитория из конфигурации
        /// </summary>
        public string Name { get; set; }

        public string WebUI { get; set; }

        /// <summary>
        /// Путь к репозиторию
        /// </summary>
        public string RepoPath { get; set; }
    }
}
