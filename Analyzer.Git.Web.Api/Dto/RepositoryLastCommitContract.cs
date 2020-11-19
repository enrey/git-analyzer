using System;
using LibGit2Sharp;

namespace GitAnalyzer.Web.Contracts.Statistics
{
    /// <summary>
    /// DTO последнего коммита в репозитории
    /// </summary>
    public class RepositoryLastCommitContract
    {
        /// <summary>
        /// Имя репозитория
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Url путь до репозитория
        /// </summary>
        public string RepositoryUrl { get; set; }

        /// <summary>
        /// Хеш последнего коммита из репозитория
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Дата последнего коммита из репозитория
        /// </summary>
        public DateTimeOffset? Date { get; set; }
    }
}