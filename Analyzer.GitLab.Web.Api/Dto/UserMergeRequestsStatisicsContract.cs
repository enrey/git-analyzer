using System.Collections.Generic;

namespace GitAnalyzer.Web.Contracts.GitLab
{
    /// <summary>
    /// Контракт данных о статистике мерджреквестов пользователя
    /// </summary>
    public class UserMergeRequestsStatisicsContract
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Количество открытых пользователем реквестов
        /// </summary>
        public int OpenedTotal { get; set; }

        /// <summary>
        /// Количество вмердженных пользователем реквестов
        /// </summary>
        public int MergedTotal { get; set; }

        /// <summary>
        /// Количество открытых пользователем реквестов
        /// </summary>
        public IList<UserMergeRequestsStatisicsItemContract> Items { get; set; }
    }    
}
