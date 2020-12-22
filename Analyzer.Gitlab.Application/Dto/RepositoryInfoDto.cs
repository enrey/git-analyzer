namespace Analyzer.Gitlab.Application.Dto
{
    public class RepositoryInfoDto
    {
        /// <summary>
        /// Имя репозитория
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Url удаленного репозитория 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// UI для фронта
        /// </summary>
        public string WebUI { get; set; }

        /// <summary>
        /// Локальная папка для репозитория
        /// </summary>
        public string LocalPath { get; set; }
    }
}
