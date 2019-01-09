using ConsoleTableExt;
using LibGit2Sharp;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace git_an
{
    internal class Main
    {
        //private const string REPO_PATH = @"D:\Work\npa-git\npa";
        private const string REPO_PATH = @"D:\Work\oog";
        private const int DEPTH = 500;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static List<string[]> collapse = new List<string[]>();
        static int divide = 7;

        internal static /*async Task*/ void Work(string[] args)
        {
            //await Task.FromResult(0);


            var dtFromString = "08.01.2018";
            var dtTillString = "01.09.2018";
            var dtFrom = DateTime.ParseExact(dtFromString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var dtTill = DateTime.ParseExact(dtTillString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            collapse.Add(new[]{ "Sergey V. Nemerenko", "Nemerenko Sergey", @"IT2G\nemerenkosv", "Немеренко Сергей" });
        

        var totalDays = (dtTill - dtFrom).Days;

            for (int i = 0; i <= totalDays; i=i+divide)
            {

                var dtStepFrom = dtFrom.AddDays(i);
                var dtStepTill = dtFrom.AddDays(i + divide);



                var result = Count(dtStepFrom, dtStepTill, "Sergey V. Nemerenko");

            //Console.WriteLine("Total commits in repo: " + result.RepoTotalCommitsCount);
            //Console.WriteLine("First commit in repo: " + result.RepoFirst.Author.When.ToLocalTime() + " " + result.RepoFirst.Author.Name);
            //Console.WriteLine("Last commit in repo: " + result.RepoLast.Author.When.ToLocalTime() + " " + result.RepoLast.Author.Name);
            //Console.WriteLine("Searched:");
            Console.WriteLine("First commit: " + result.First.Author.When.ToLocalTime() + " " + result.First.Author.Name);
            Console.WriteLine("Last commit: " + result.Last.Author.When.ToLocalTime() + " " + result.Last.Author.Name);
            Console.WriteLine("Commits found: " + result.TotalCommitsCount);

            DataTable table = new DataTable();

            table.Columns.Add("Автор", typeof(string));
            table.Columns.Add("Коммитов", typeof(int));
            table.Columns.Add("Добавлено", typeof(int));
            table.Columns.Add("Удалено", typeof(int));
            table.Columns.Add("Итого", typeof(int));


            foreach (var author in result.authors.OrderByDescending(o=>o.Value.Commits))
            {
                table.Rows.Add(author.Key, author.Value.Commits, author.Value.Modified, author.Value.Deleted, author.Value.Total);
                //Console.WriteLine($"{author.Key} {author.Value.Modified} {author.Value.Deleted}");
            }
                if (table.Rows.Count != 0)
                {
                    ConsoleTableBuilder.From(table).ExportAndWriteLine();
                }
            }

        }

        class Result
        {
            public Dictionary<string, StatsInfo> authors { get; set; } = new Dictionary<string, StatsInfo>();

            public DateTime minDate { get; set; }

            public DateTime maxDate { get; set; }
            public int RepoTotalCommitsCount { get; internal set; }
            public Commit RepoFirst { get; internal set; }
            public Commit RepoLast { get; internal set; }

            public int TotalCommitsCount { get; internal set; }
            public Commit First { get; internal set; }
            public Commit Last { get; internal set; }

        }

        class StatsInfo
        {
            public StatsInfo(int modified, int deleted, int commited)
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


        private static Result Count(DateTime dtFrom, DateTime dtTill, string onlyDeveloper = null)
        {
            if (!Repository.IsValid(REPO_PATH))
            {
                throw new Exception("Not a valid git repo path");
            }

            var repo = new Repository(REPO_PATH);

            List<(string Author, DateTimeOffset when, int Modified, int Deleted)> stats = new List<(string Author, DateTimeOffset when, int Modified, int Deleted)>();

            var filter = new CommitFilter { SortBy = CommitSortStrategies.Time };

            // Where--no - merges is documented as "Do not print commits with more than one parent. This is exactly the same as --max-parents=1.".
            var all = repo.Commits.Where(o => o.Parents.Count() == 1).ToList();
            //var all = repo.Commits.ToList();

            dtFrom = CheckDateBoundary(all, dtFrom);
            dtTill = CheckDateBoundary(all, dtTill);

            var itemFrom = FindHalf(all, dtFrom);
            var itemTill = FindHalf(all, dtTill);


            //ar commitsQuery = repo.Commits.QueryBy(filter)/*.Take(DEPTH)*/.Where(o => o.Parents.Count() == 1).Where(o=>o.Author.When >= dt);
            var commitsQuery = all.SkipWhile(o=> o.Id != itemTill.Id).TakeWhile(o => o.Id != itemFrom.Id).ToList();

            PatchEntryChanges ch = null;
            Commit c = null;

            foreach (Commit commit in commitsQuery)
            {
                foreach (var parent in commit.Parents)
                {

                    //var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, filter2(commit.Tree));
                    
                    //var diff = ;
                    foreach (var change in repo.Diff.Compare<Patch>(parent.Tree, commit.Tree))
                    {
                        if (change.IsBinaryComparison /*|| Path.GetExtension(change.Path) != ".java"*/)
                            continue;




                        if (ch == null || change.LinesDeleted > ch.LinesDeleted)
                        {
                            ch = change;
                            c = commit;
                        }

                        // Схлопывание
                        string author = commit.Author.Name;

                        foreach (var col in collapse)
                        {
                            if (col.Contains(commit.Author.Name))
                            {
                                author = col[0];
                                break;
                            }
                        }

                        if (onlyDeveloper != null && author != onlyDeveloper)
                            continue;

                        //Logger.Info($"{change.Status} : {change.Path}; ADDED: {change.LinesAdded}, DELETED: {change.LinesDeleted} ");
                            stats.Add((author, commit.Author.When.ToLocalTime(), change.LinesAdded, change.LinesDeleted));
                    }
                }
            }

            var result = new Result
            {
                RepoFirst = all.Last(),
                RepoLast = all.First(),
                RepoTotalCommitsCount = all.Count,
                First = commitsQuery.Last(),
                Last = commitsQuery.First(),
                TotalCommitsCount = commitsQuery.Count,
                authors = stats.GroupBy(o => o.Author).ToDictionary(o => o.Key, o => new StatsInfo(o.Sum(i => i.Modified), o.Sum(i => i.Deleted), o.Count()))
            };
            

            return result;
        }

        public static Tree filter2(Tree tree)
        {
            Console.WriteLine(tree.Skip(2).First().Name);

            return tree;
            //return tree.Where(o=>o.Name)
        }

        private static Commit FindHalf(IList<Commit> sortedDescCommits, DateTime dt/*, int offset = 0*/)
        {
            var cnt = sortedDescCommits.Count;

            var off = cnt / 2;

            //Console.WriteLine(cnt);
            var when = sortedDescCommits.Skip(off).Take(1).Single();
            //Console.WriteLine(when);

            if (off == 1)
                return when;

            if (when.Author.When >= dt)
                return FindHalf(sortedDescCommits.Skip(off).ToList(), dt);
            else
                return FindHalf(sortedDescCommits.Take(off).ToList(), dt);


        }

        private static DateTime CheckDateBoundary(IList<Commit> sortedDescCommits, DateTime dt)
        {
            var maxrepodate = sortedDescCommits.First().Author.When;
            var minrepodate = sortedDescCommits.Last().Author.When;

            if (dt > maxrepodate)
            {
                Logger.Info($"Date {dt} is to big. Using {maxrepodate.ToLocalTime().Date}");
                return maxrepodate.ToLocalTime().Date;

            }

            if (dt < minrepodate)
            {
                Logger.Info($"Date {dt} is to small. Using {minrepodate.ToLocalTime().Date}");
                return minrepodate.ToLocalTime().Date;
            }

            return dt;
        }
    }
}
