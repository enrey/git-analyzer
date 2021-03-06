﻿using Analyzer.Git.Application.Dto.GitLab;
using System;
using System.Collections.Generic;

namespace Analyzer.GitLab.Web.Api.Dto
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
        /// Username пользователя
        /// </summary>
        public string Username { get; set; }

        public List<DateAndIdItem> OpenedDates { get; set; }

        public List<DateAndIdItem> MergedDates { get; set; }

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
