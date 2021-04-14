using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Git.Application.Services;
using Analyzer.Git.Web.Api.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Analyzer.Git.Web.Api.Controllers
{
    /// <summary>
    /// Контроллер для раьоты со статистикой GIT репозиториев
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly GitElasticService _gitElasticService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Контроллер для раьоты со статистикой GIT репозиториев
        /// </summary>
        public StatisticsController(
            IGitStatisticsService gitStatisticsService,
            GitElasticService gitElasticService,
            IMapper mapper)
        {
            _gitStatisticsService = gitStatisticsService;
            _gitElasticService = gitElasticService;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение статистики из GIT репозиториев 
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<RepositoryStatisticsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            //var statistics = await _gitStatisticsService.GetAllRepositoriesStatisticsAsync(startDate, endDate);

            var statistics = _gitElasticService.GetInfo(startDate, endDate);
            
            var result = _mapper.Map<IEnumerable<RepositoryStatisticsContract>>(statistics);

            return Ok(result);
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

        [HttpPost("update-elastic")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Update()
        {
             _gitElasticService.UpdateMonth();            

            return Ok();
        }

        /// <summary>
        /// Оценки затраченного на работу времени
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("work-estimates/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<RepositoryWorkEstimateContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRepositoriesWorkEstimate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var estimates = await _gitStatisticsService.GetWorkSessionsEstimate(startDate, endDate);
            var result = _mapper.Map<IEnumerable<RepositoryWorkEstimateContract>>(estimates);

            return Ok(result);
        }
    }
}