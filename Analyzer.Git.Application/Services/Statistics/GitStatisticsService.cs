using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Dto.Statistics;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitAnalyzer.Application.Services.Statistics
{
    /// <summary>
    /// Сервис для получения статистики из GIT репозиториев
    /// </summary>
    public class GitStatisticsService : IGitStatisticsService
    {
        private const int MAX_PARALLEL = 1;
        private readonly ILogger _logger;
        private readonly StatisticsConfig _statisticsConfig;
        private readonly RepositoriesConfig _repositoriesConfig;
        private readonly WorkEstimateConfig _workEstimateConfig;

        private readonly object _locker = new object();

        /// <summary>
        /// Сервис для получения статистики из GIT репозиториев
        /// </summary>
        public GitStatisticsService(
            ILogger<GitStatisticsService> logger,
            IOptionsMonitor<StatisticsConfig> statisticsConfigMonitor,
            IOptionsMonitor<RepositoriesConfig> repositoriesConfig,
            IOptionsMonitor<WorkEstimateConfig> workEstimateConfig
            )
        {
            _logger = logger;
            _statisticsConfig = statisticsConfigMonitor.CurrentValue;
            _repositoriesConfig = repositoriesConfig.CurrentValue;
            _workEstimateConfig = workEstimateConfig.CurrentValue;
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

            var validRepos = GetAllRepositoriesParameters()
                .Where(ri => Repository.IsValid(ri.RepoPath));

            if (!validRepos.Any())
                throw new Exception("Отсутствуют клонированные репозитории");


            var result = new List<RepositoryStatisticsDto>();

            await Task.Run(() =>
            {
                Parallel.ForEach(validRepos,
                    ri =>
                    {
                        var repoStatistics = new RepositoryStatisticsDto
                        {
                            RepositoryName = ri.Name,
                            WebUI = ri.WebUI,
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
        /// Возвращает последние коммиты репозиториев
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RepositoryLastCommitDto>> GetAllRepositoriesLastCommitAsync()
        {
            var validRepos = GetAllRepositoriesParameters()
                .Where(ri => Repository.IsValid(ri.RepoPath));

            var result = new List<RepositoryLastCommitDto>();

            if (!validRepos.Any())
                throw new Exception("Отсутствуют клонированные репозитории");

            await Task.Run(() =>
            {
                Parallel.ForEach(validRepos,
                    ri =>
                    {
                        var commit = new Repository(ri.RepoPath).Head.Tip;

                        var repoCommits = new RepositoryLastCommitDto
                        {
                            RepositoryName = ri.Name,
                            RepositoryHash = commit?.Sha,
                            RepositoryDate = commit?.Author?.When,
                        };

                        lock (_locker)
                        {
                            result.Add(repoCommits);
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
            _logger.LogInformation($"Updating repositories started");

            // TODO: Если токена нет то использовать логин/пароль Credentials = new UsernamePasswordCredentials { Username = info.Username, Password = info.Password }
            var defaultCreds = new UsernamePasswordCredentials
            {
                Username = "token",
                Password = _repositoriesConfig.GitlabAuthToken
            };

            var reposInfo = _repositoriesConfig.ReposInfo
                .Select(info => new
                {
                    RepoUrl = info.Url,
                    RepoPath = @$"{_repositoriesConfig.ReposFolder}/{info.LocalPath}",
                    Credentials = defaultCreds
                }).ToList();

            await Task.Run(() =>
            {
                Parallel.ForEach(reposInfo, new ParallelOptions { MaxDegreeOfParallelism = MAX_PARALLEL }, info =>
                {
                    if (!Repository.IsValid(info.RepoPath))
                        CloneRepository(info.RepoUrl, info.RepoPath, info.Credentials);
                    else
                        PullRepository(info.RepoPath, info.Credentials);
                });
            });

            _logger.LogInformation($"Updating repositories ended");
        }

        /// <summary>
        /// Суммирует часы по дням. 
        /// Предполагаем, что работа за день началась за 2 часа до первого коммита и окончилась последним коммитом
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<RepositoryWorkEstimateDto>> GetWorkSessionsEstimate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var dates = GetDates(startDate, endDate);

            var validRepos = GetAllRepositoriesParameters()
                .Where(ri => Repository.IsValid(ri.RepoPath))
                .ToList();

            if (!validRepos.Any())
                throw new Exception("Отсутствуют клонированные репозитории");


            var result = new List<RepositoryWorkEstimateDto>();

            await Task.Run(() =>
            {
                Parallel.ForEach(validRepos,
                    vr =>
                    {
                        var repoWorkEstimate = GetRepositoryWorkEstimate(vr, dates);

                        lock (_locker)
                        {
                            result.Add(repoWorkEstimate);
                        }
                    });
            });

            return result.OrderBy(r => r.RepositoryName).ToList();
        }

        /// <summary>
        /// Получить оценку рабочего
        /// </summary>
        /// <param name="repositoryParameters"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        private RepositoryWorkEstimateDto GetRepositoryWorkEstimate(RepositoryParameters repositoryParameters, IEnumerable<DateTime> dates)
        {
            _logger.LogInformation($"Getting work estimates started: \"{repositoryParameters.RepoPath}\"");

            using var repo = new Repository(repositoryParameters.RepoPath);

            var commits = GetCommits(repo, dates, true);

            var estimates = commits.Select(c => new
            {
                Email = c.Author.Email.ToLower(),
                Commit = c
            })
            .GroupBy(key => key.Email)
            .Select(g => new
            {
                Email = g.Key,
                //Commits = g.Select(v => v.Commit).ToList(),
                Hours = EstimateHours(g.Select(v => v.Commit.Author.When))
            })
            .Select(i => new PersonWorkEstimateDto
            {
                Email = i.Email,
                //i.Commits,
                Hours = Math.Round(i.Hours, 2),
                Days = Math.Round(i.Hours / 8, 2)
            })
            .ToList();

            var result = new RepositoryWorkEstimateDto
            {
                RepositoryName = repositoryParameters.Name,
                Estimates = estimates
            };

            _logger.LogInformation($"Getting work estimates ended: \"{repositoryParameters.RepoPath}\"");

            return result;
        }

        /// <summary>
        /// Подсчет количества рабочих часов
        /// </summary>
        /// <param name="commitsDates"></param>
        /// <returns></returns>
        private double EstimateHours(IEnumerable<DateTimeOffset> commitsDates)
        {
            var next = new DateTimeOffset();
            double dayHours = _workEstimateConfig.WorkDayHours;
            double padding = _workEstimateConfig.PaddingHours;

            double result = 0;

            var ordered = commitsDates.OrderByDescending(cd => cd);

            foreach (var t in ordered)
            {
                if (next.Ticks == 0)
                {
                    next = t;
                    continue;
                }

                var diff = (next - t).TotalHours;

                result += diff < dayHours
                 ? diff
                 : padding;

                next = t;
            }

            result += padding;

            return result;
        }

        /// <summary>
        /// Получить коммиты из репозитория за указанные даты с мерджами или без
        /// </summary>
        private IEnumerable<Commit> GetCommits(Repository repository, IEnumerable<DateTime> dates, bool includeMerges)
        {
            // iterate all branches (git log --all)
            var filter = new CommitFilter { IncludeReachableFrom = repository.Branches };

            var result = repository.Commits.QueryBy(filter)
                .Where(c =>
                    (includeMerges || !includeMerges && c.Parents.Count() == 1) &&
                    dates.Contains(c.Author.When.Date)
                ).ToList();

            return result;
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

            var commits = GetCommits(repo, dates, false);

            var result = dates.Select(date => GetDayStatistics(repo, commits, date)).ToList();

            _logger.LogInformation($"Getting statistics ended: \"{repositoryPath}\"");

            return result;
        }

        /// <summary>
        /// Получить статистику за день
        /// </summary>
        private PeriodStatisticsDto GetDayStatistics(Repository repository, IEnumerable<Commit> filteredCommits, DateTime date)
        {
            var dayCommits = filteredCommits.Where(c => c.Author.When.Date == date).ToList();

            var statistics = dayCommits.SelectMany(commit =>
                 commit.Parents.SelectMany(parent =>
                    repository.Diff.Compare<Patch>(parent.Tree, commit.Tree)
                        .Where(change => !change.IsBinaryComparison)
                        .Select(change => new
                        {
                            commit.Id,
                            commit.Author.Name,
                            commit.Author.Email,
                            commit.Sha,
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
                    Shas = g.Select(o => o.Sha),
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
                    Shas = g.SelectMany(o => o.Shas).ToList(),
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

        /// <summary>
        /// Сформировать параметры репозиториев из конфигурации
        /// </summary>
        /// <returns></returns>
        private IEnumerable<RepositoryParameters> GetAllRepositoriesParameters()
        {
            return _repositoriesConfig.ReposInfo
                .Select(ri => new RepositoryParameters
                {
                    Name = ri.Name,
                    WebUI = ri.WebUI,
                    RepoPath = @$"{_repositoriesConfig.ReposFolder}/{ri.LocalPath}"
                })
                .ToList();
        }
    }
}
