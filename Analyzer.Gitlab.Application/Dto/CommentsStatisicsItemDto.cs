using System;

namespace Analyzer.Git.Application.Dto.GitLab
{
    public class CommentsStatisicsItemDto
    {
        public string Username { get; set; }

        public DateTime Dt { get; set; }

        public string Comment { get; set; }
    }
}
