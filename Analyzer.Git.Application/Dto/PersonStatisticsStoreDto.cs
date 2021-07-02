namespace Analyzer.Git.Application.Dto
{
    public class PersonStatisticsStoreDto : CommitStatisticsDto
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
    }
}
