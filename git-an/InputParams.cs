using System;
using System.Collections.Generic;

namespace git_an
{
    public class InputParams
    {
        public string RepoPath { get; set; }

        public DateTime DtFrom { get; set; }

        public DateTime DtTill { get; set; }

        public List<string[]> AuthorsToDedupe { get; set; }
    }
}
