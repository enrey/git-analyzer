using System;

namespace Analyzer.Git.Application.Dto.GitLab
{
    public class DateAndIdItem
    {
        public DateTime Dt { get; set; }

        public string Url { get; set; }

        public int Iid { get; set; }

        public string Repo { get; set; }

        public string Title { get; set; }
    }
}
