using LibGit2Sharp;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace git_an
{
    public class Calculator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string FILTERED_EXT = ".html";

        private InputParams _inputData;

        private Repository _repo;

        private List<Commit> _allCommits;

        public Calculator(InputParams inputData)
        {
            _inputData = inputData;
        }

        public void Init()
        {
            if (!Repository.IsValid(_inputData.RepoPath)) throw new Exception("Not a valid git repo path");

            _repo = new Repository(_inputData.RepoPath);
            _allCommits = HardFilterCommits();
        }

        public Result Calculate(DateTime dtFrom, DateTime dtTill, string onlyDeveloper = null)
        {
            var minDate = CheckDateBoundary(_allCommits, dtFrom);
            var maxDate = CheckDateBoundary(_allCommits, dtTill);
            var commitsQuery = GetCommits(_allCommits, minDate, maxDate);

            var stats = new List<(string Author, DateTimeOffset when, int Modified, int Deleted)>();

            foreach (Commit commit in commitsQuery)
            {
                foreach (var parent in commit.Parents)
                {
                    foreach (var change in _repo.Diff.Compare<Patch>(parent.Tree, commit.Tree))
                    {
                        if (ShouldSkip(change))
                            continue;

                        string author = GetAuthor(commit);

                        if (onlyDeveloper != null && author != onlyDeveloper)
                            continue;

                        stats.Add((author, commit.Author.When.ToLocalTime(), change.LinesAdded, change.LinesDeleted));
                    }
                }
            }

            var result = new Result
            {
                DtFrom = minDate,
                DtTill = maxDate,
                Authors = stats.GroupBy(o => o.Author).ToDictionary(o => o.Key, o => new AuthorStats(o.Sum(i => i.Modified), o.Sum(i => i.Deleted), o.Count()))
            };

            return result;
        }

        private List<Commit> GetCommits(List<Commit> all, DateTime minDate, DateTime maxDate)
        {
            var commitFrom = FindHalf(all, minDate);
            var commitTill = FindHalf(all, maxDate);

            var commitsQuery = all.SkipWhile(o => o.Id != commitTill.Id).TakeWhile(o => o.Id != commitFrom.Id).ToList();
            return commitsQuery;
        }

        private List<Commit> HardFilterCommits()
        {
            // --no-merges is documented as "Do not print commits with more than one parent". This is exactly the same as --max-parents=1.".
            var result = _repo.Commits.Where(o => o.Parents.Count() == 1).ToList();

            Logger.Info($"Repo First Commit: {result.Last()}");
            Logger.Info($"Repo Last Commit: {result.First()}");
            Logger.Info($"Repo Total Commits Count: {result.Count}");

            return result;
        }

        private static bool ShouldSkip(PatchEntryChanges change)
        {
            // Игнорируем бинарники и возможно некоторые типы файлов
            return change.IsBinaryComparison  || System.IO.Path.GetExtension(change.Path).ToLower().Contains(FILTERED_EXT);
        }

        /// <summary>
        /// Схлопывание по автору
        /// </summary>
        private string GetAuthor(Commit commit)
        {
            var author = commit.Author.Name;

            foreach (var col in _inputData.AuthorsToDedupe)
            {
                if (col.Contains(commit.Author.Name))
                {
                    author = col[0];
                    break;
                }
            }

            return author;
        }

        /// <summary>
        /// Ищет коммиты по дате методом половинного деления (для скорости, очень долгая операция)
        /// </summary>
        private static Commit FindHalf(IList<Commit> sortedDescCommits, DateTime dt)
        {
            var total = sortedDescCommits.Count / 2;
            var current = sortedDescCommits.Skip(total).Take(1).Single();

            if (total == 1)
                return current;

            if (current.Author.When >= dt)
                return FindHalf(sortedDescCommits.Skip(total).ToList(), dt);
            else
                return FindHalf(sortedDescCommits.Take(total).ToList(), dt);
        }

        private static DateTime CheckDateBoundary(IList<Commit> sortedDescCommits, DateTime dt)
        {
            var maxRepoDate = sortedDescCommits.First().Author.When;
            var minRepoDate = sortedDescCommits.Last().Author.When;

            if (dt > maxRepoDate)
            {
                Logger.Info($"Date {dt} is to big. Using {maxRepoDate.ToLocalTime().Date}");
                return maxRepoDate.ToLocalTime().Date;

            }

            if (dt < minRepoDate)
            {
                Logger.Info($"Date {dt} is to small. Using {minRepoDate.ToLocalTime().Date}");
                return minRepoDate.ToLocalTime().Date;
            }

            return dt;
        }

    }
}