﻿namespace GitAnalyzer.Web.Application.Dto.Statistics
{
    /// <summary>
    /// DTO для данных оценки затраченного времени
    /// </summary>
    public class PersonWorkEstimateDto
    {
        public string Email { get; set; }

        public double Hours { get; set; }

        public double Days { get; set; }
    }
}
