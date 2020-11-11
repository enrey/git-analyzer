using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GitAnalyzer.Application.Dto.Statistics;
using GitAnalyzer.Application.Services.GitLab;
using GitAnalyzer.Web.Contracts.GitLab;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace GitAnalyzer.Web.GitLab.Api.Controllers
{
    /// <summary>
    /// Контроллер для работы с данными из GitLab
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GitLabController : ControllerBase
    {
        private const int TIMEOUT_SECONDS = 60 * 60;

        private readonly IGitLabService _gitLabService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Контроллер для работы с данными из GitLab
        /// </summary>
        public GitLabController(IGitLabService gitLabService, IMapper mapper, IMemoryCache cache)
        {
            _gitLabService = gitLabService;
            _cache = cache;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение статистики по мерджреквестам из GitLab'а
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("{startDate}/{endDate}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<UserMergeRequestsStatisicsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMergeRequests(DateTime startDate, DateTime endDate)
        {
            return await _cache.GetOrCreateAsync("statistic" + startDate.ToShortDateString() + "/" + endDate.ToShortDateString(),
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    var dtos = await _gitLabService.GetMergeRequestsStatistics(startDate, endDate);

                    var result = _mapper.Map<IEnumerable<UserMergeRequestsStatisicsContract>>(dtos);

                    return Ok(result);
                });
        }

        /// <summary>
        /// Получение пользователей GitLab'а
        /// </summary>
        [HttpGet("gitlabUsers")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<UserMergeRequestsStatisicsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGitlabUsers()
        {
            return await _cache.GetOrCreateAsync("statistic_users",
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    var dtos = await _gitLabService.GetUsers();

                    return Ok(dtos);
                });
        }


        /// <summary>
        /// Возвращает последние коммиты репозиториев GitLab'а
        /// </summary>
        [HttpGet("gitlab-repositories-commits")]
        [ProducesResponseType(typeof(IEnumerable<RepositoryLastCommitDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGitlabAllRepositoriesLastCommit()
        {
            var commits = await _gitLabService.GetRepositoriesLastCommit();

            return Ok(commits);
        }
    }
}