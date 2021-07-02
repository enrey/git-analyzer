using System;

namespace Analyzer.Git.Application.Dto
{
    public class PersonStatisticsCommitResultDto
    {
        public DateTime CommitDate { get; set; }

        public string Sha { get; set; }

        public string Message { get; set; }

        public int Deleted { get; set; }

        /// <summary>Количество добавлений </summary>
        public int Added { get; set; }

        public int Total { get; set; }

    }
}
