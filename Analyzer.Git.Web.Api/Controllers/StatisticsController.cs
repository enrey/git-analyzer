using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Git.Application.Services.Statistics;
using Analyzer.Git.Web.Api.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Analyzer.Git.Web.Api.Controllers
{
    /// <summary>
    /// Контроллер для раьоты со статистикой GIT репозиториев
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private const int TIMEOUT_SECONDS = 60 * 60;

        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Контроллер для раьоты со статистикой GIT репозиториев
        /// </summary>
        public StatisticsController(
            IGitStatisticsService gitStatisticsService,
            IMapper mapper,
            IMemoryCache cache)
        {
            _gitStatisticsService = gitStatisticsService;
            _mapper = mapper;
            _cache = cache;
        }

        /// <summary>
        /// Получение статистики из GIT репозиториев 
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("{startDate}/{endDate}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<RepositoryStatisticsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return await _cache.GetOrCreateAsync("statistic" + startDate.DateTime.ToShortDateString() + "/" + endDate.DateTime.ToShortDateString(),
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    var statistics = await _gitStatisticsService.GetAllRepositoriesStatisticsAsync(startDate, endDate);
                    var result = _mapper.Map<IEnumerable<RepositoryStatisticsContract>>(statistics);

                    return Ok(result);
                });
        }

        /// <summary>
        /// Получение последнего коммита GIT репозиториев
        /// </summary>
        /// <returns></returns>
        [HttpGet("repositories-commits")]
        [ProducesResponseType(typeof(IEnumerable<RepositoryLastCommitContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLastCommits()
        {
            var commits = await _gitStatisticsService.GetAllRepositoriesLastCommitAsync();
            var result = _mapper.Map<IEnumerable<RepositoryLastCommitContract>>(commits);

            return Ok(result);
        }


        /// <summary>
        /// Обновление GIT репозиториев
        /// </summary>
        /// <returns></returns>
        [HttpGet("update-repositories")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateRepositories()
        {
            await _gitStatisticsService.UpdateAllRepositories();

            return NoContent();
        }

        /// <summary>
        /// Обновление GIT репозитория
        /// </summary>
        /// <returns></returns>
        [HttpGet("update-repositories/{repositoryUrl}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateRepositories(string repositoryUrl)
        {
            await _gitStatisticsService.UpdateRepository(repositoryUrl);

            return NoContent();
        }

        /// <summary>
        /// Оценки затраченного на работу времени
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("work-estimates/{startDate}/{endDate}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<RepositoryWorkEstimateContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRepositoriesWorkEstimate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return await _cache.GetOrCreateAsync("estimates" + startDate.DateTime.ToShortDateString() + "/" + endDate.DateTime.ToShortDateString(),
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    var estimates = await _gitStatisticsService.GetWorkSessionsEstimate(startDate, endDate);
                    var result = _mapper.Map<IEnumerable<RepositoryWorkEstimateContract>>(estimates);

                    return Ok(result);
                });
        }
    }
}