namespace Analyzer.Gitlab.Application.Dto
{
    public class RepositoryInfoDto
    {
        /// <summary>
        /// Url удаленного репозитория 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// UI для фронта
        /// </summary>
        public string WebUI { get; set; }
    }
}
