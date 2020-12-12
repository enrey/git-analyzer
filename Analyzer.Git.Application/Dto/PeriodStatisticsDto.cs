﻿using System;
using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto
{
    /// <summary>
    /// DTO статистики за период
    /// </summary>
    public class PeriodStatisticsDto
    {
        /// <summary>
        /// Дата за которую собрана статистика
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Статистика по сотрудникам
        /// </summary>
        public IEnumerable<PersonStatisticsDto> Statistics { get; set; }
    }
}
