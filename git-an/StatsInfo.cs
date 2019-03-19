namespace git_an
{
    public class AuthorStats
    {
        public AuthorStats(int modified, int deleted, int commited)
        {
            Modified = modified;
            Deleted = deleted;
            Commits = commited;
        }

        public int Modified { get; set; }

        public int Deleted { get; set; }

        public int Commits { get; set; }

        public int Total { get { return Modified + Deleted; } }
    }
}
