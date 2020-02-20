using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GitAnalyzer.Web.Application.Services.Statistics;
using GitAnalyzer.Web.Contracts.Statistics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitAnalyzer.Web.Api.Controllers
{
    /// <summary>
    /// Контроллер для раьоты со статистикой GIT репозиториев
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Контроллер для раьоты со статистикой GIT репозиториев
        /// </summary>
        public StatisticsController(
            IGitStatisticsService gitStatisticsService,
            IMapper mapper)
        {
            _gitStatisticsService = gitStatisticsService;
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
            var statistics = await _gitStatisticsService.GetAllRepositoriesStatisticsAsync(startDate, endDate);

            var result = _mapper.Map<IEnumerable<RepositoryStatisticsContract>>(statistics);

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
    }
}