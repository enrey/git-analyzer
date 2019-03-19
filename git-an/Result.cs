using LibGit2Sharp;
using System;
using System.Collections.Generic;

namespace git_an
{
    public class Result
    {
        public Dictionary<string, AuthorStats> Authors { get; set; } = new Dictionary<string, AuthorStats>();

        public DateTime DtFrom { get; set; }

        public DateTime DtTill { get; set; }
    }
}
