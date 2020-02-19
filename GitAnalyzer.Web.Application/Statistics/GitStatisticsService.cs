using GitAnalyzer.Web.Application.Configuration;
using GitAnalyzer.Web.Application.Dto.Statistics;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitAnalyzer.Web.Application.Statistics
{
    /// <summary>
    /// Сервис для получения статистики из GIT репозиториев
    /// </summary>
    public class GitStatisticsService : IGitStatisticsService
    {
        private readonly ILogger _logger;
        private readonly StatisticsConfig _statisticsConfig;
        private readonly RepositoriesConfig _repositoriesConfig;
        private readonly object _locker = new object();

        /// <summary>
        /// Сервис для получения статистики из GIT репозиториев
        /// </summary>
        public GitStatisticsService(
            ILogger<GitStatisticsService> logger,
            IOptionsMonitor<StatisticsConfig> statisticsConfigMonitor,
            IOptionsMonitor<RepositoriesConfig> repositoriesConfig
            )
        {
            _logger = logger;
            _statisticsConfig = statisticsConfigMonitor.CurrentValue;
            _repositoriesConfig = repositoriesConfig.CurrentValue;
        }

        /// <summary>
        /// Получить статистику по всем репозиториям за период
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RepositoryStatisticsDto>> GetAllRepositoriesStatisticsAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var dates = GetDates(startDate, endDate);

            var validRepos = _repositoriesConfig.ReposInfo
                .Select(ri => new 
                { 
                    ri.Name,
                    RepoPath = @$"{_repositoriesConfig.ReposFolder}\{ri.LocalPath}",
                    Credentials = new UsernamePasswordCredentials
                    {
                        Username = ri.Username,
                        Password = ri.Password
                    }
                })
                .Where(ri => Repository.IsValid(ri.RepoPath));

            if (!validRepos.Any())
                throw new Exception("Отсутствуют клонированные репозитории");


            await Task.Run(() =>
            {
                Parallel.ForEach(validRepos, vr =>
                {
                    PullRepository(vr.RepoPath, vr.Credentials);
                });
            });
            

            var result = new List<RepositoryStatisticsDto>();

            await Task.Run(() =>
            {
                Parallel.ForEach(validRepos,
                    ri =>
                    {
                        var repoStatistics = new RepositoryStatisticsDto
                        {
                            RepositoryName = ri.Name,
                            Periods = GetStatisticsByDates(ri.RepoPath, dates)
                        };

                        lock (_locker)
                        {
                            result.Add(repoStatistics);
                        }
                    });
            });

            return result.OrderBy(r => r.RepositoryName).ToList();
        }

        /// <summary>
        /// Обновление всех репозиториев
        /// </summary>
        public async Task UpdateAllRepositories()
        {
            var reposInfo = _repositoriesConfig.ReposInfo
                .Select(info => new
                {
                    RepoUrl = info.Url,
                    RepoPath = @$"{_repositoriesConfig.ReposFolder}\{info.LocalPath}",
                    Credentials = new UsernamePasswordCredentials
                    {
                        Username = info.Username,
                        Password = info.Password
                    }
                }).ToList();
            
            await Task.Run(() =>
            {
                Parallel.ForEach(reposInfo, info =>
                {
                    if (!Repository.IsValid(info.RepoPath))
                        CloneRepository(info.RepoUrl, info.RepoPath, info.Credentials);
                    else
                        PullRepository(info.RepoPath, info.Credentials);
                });
            });
        }


        /// <summary>
        /// Клонировать репозиторий
        /// </summary>
        private void CloneRepository(string repoUrl, string repoPath, UsernamePasswordCredentials credentials)
        {
            try
            {
                if (!Directory.Exists(repoPath))
                    Directory.CreateDirectory(repoPath);

                var cloneOptions = new CloneOptions
                {
                    IsBare = false,
                    CredentialsProvider = (_url, _user, _cred) => credentials
                };

                _logger.LogInformation($"Cloning started: \"{repoUrl}\"");

                Repository.Clone(repoUrl, repoPath, cloneOptions);

                _logger.LogInformation($"Cloning ended: \"{repoUrl}\"");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Clonning error! Repository: \"{repoUrl}\". Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить изменения репозитория
        /// </summary>
        private void FetchRepository(string repoPath, UsernamePasswordCredentials credentials)
        {
            try
            {
                using var repo = new Repository(repoPath);

                var fetchOptions = new FetchOptions
                {
                    Prune = true,
                    CredentialsProvider = (_url, _user, _cred) => credentials
                };

                _logger.LogInformation($"Fetching started: \"{repoPath}\"");

                string logMessage = "";
                foreach (var remote in repo.Network.Remotes)
                {
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, logMessage);
                }

                _logger.LogInformation($"Fetching ended: \"{repoPath}\". Message: \"{logMessage}\"");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fetching error! Repository: \"{repoPath}\". Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Pull изменений репозитория
        /// </summary>
        private void PullRepository(string repoPath, UsernamePasswordCredentials credentials)
        {
            try
            {
                using var repo = new Repository(repoPath);

                var pullOptions = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        Prune = true,
                        CredentialsProvider = (_url, _user, _cred) => credentials
                    }
                };

                var signature = new Signature(
                    new Identity(_repositoriesConfig.MergeUserName, _repositoriesConfig.MergeUserEmail), 
                    DateTimeOffset.Now);

                _logger.LogInformation($"Pulling started: \"{repoPath}\"");

                Commands.Pull(repo, signature, pullOptions);

                _logger.LogInformation($"Pulling ended: \"{repoPath}\"");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pulling error! Repository: \"{repoPath}\". Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Разбивка периода посуточно
        /// </summary>
        private IEnumerable<DateTime> GetDates(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var result = new List<DateTime>();
            var date = startDate.Date;

            while (date <= endDate.Date)
            {
                result.Add(date);
                date = date.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Получить посуточную статистику по переданным дням
        /// </summary>
        private IEnumerable<PeriodStatisticsDto> GetStatisticsByDates(string repositoryPath, IEnumerable<DateTime> dates)
        {
            _logger.LogInformation($"Getting statistics started: \"{repositoryPath}\"");

            using var repo = new Repository(repositoryPath);

            // iterate all branches (git log --all)
            var filter = new CommitFilter { IncludeReachableFrom = repo.Branches };

            var filteredCommits = repo.Commits.QueryBy(filter).Where(c =>
                c.Parents.Count() == 1 &&
                dates.Contains(c.Committer.When.Date)
            ).ToList();

            var result = dates.Select(date => GetDayStatistics(repo, filteredCommits, date)).ToList();

            _logger.LogInformation($"Getting statistics ended: \"{repositoryPath}\"");

            return result;
        }

        /// <summary>
        /// Получить статистику за день
        /// </summary>
        private PeriodStatisticsDto GetDayStatistics(Repository repository, ICollection<Commit> filteredCommits, DateTime date)
        {
            var dayCommits = filteredCommits.Where(c => c.Committer.When.Date == date).ToList();

            var statistics = dayCommits.SelectMany(commit =>
                 commit.Parents.SelectMany(parent =>
                    repository.Diff.Compare<Patch>(parent.Tree, commit.Tree)
                        .Where(change => !change.IsBinaryComparison)
                        .Select(change => new
                        {
                            commit.Id,
                            commit.Author.Name,
                            commit.Author.Email,
                            Added = change.LinesAdded,
                            Deleted = change.LinesDeleted,
                            Total = change.LinesAdded + change.LinesDeleted
                        })
                     )
                )
                .GroupBy(r => new { r.Id, r.Name, r.Email })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Email,
                    Added = g.Sum(g => g.Added),
                    Deleted = g.Sum(g => g.Deleted),
                    Total = g.Sum(g => g.Total),
                })
                .GroupBy(r => new { r.Name, r.Email })
                .Select(g => new PersonStatisticsDto
                {
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    Commits = g.Count(),
                    Added = g.Sum(g => g.Added),
                    Deleted = g.Sum(g => g.Deleted),
                    Total = g.Sum(g => g.Total),
                })
                .OrderBy(r => r.Name)
            .ToList();

            return new PeriodStatisticsDto
            {
                Date = date,
                Statistics = statistics
            };
        }
    }
}
