using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GitAnalyzer.Application.Services.GitLab;
using GitAnalyzer.Web.Contracts.GitLab;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitAnalyzer.Web.GitLab.Api.Controllers
{
    /// <summary>
    /// Контроллер для работы с данными из GitLab
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GitLabController : ControllerBase
    {
        private readonly IGitLabService _gitLabService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Контроллер для работы с данными из GitLab
        /// </summary>
        public GitLabController(IGitLabService gitLabService, IMapper mapper)
        {
            _gitLabService = gitLabService;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение статистики по мерджреквестам из GitLab'а
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<UserMergeRequestsStatisicsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMergeRequests(DateTime startDate, DateTime endDate)
        {
            var dtos = await _gitLabService.GetMergeRequestsStatistics(startDate, endDate);

            var result = _mapper.Map<IEnumerable<UserMergeRequestsStatisicsContract>>(dtos);

            return Ok(result);
        }
    }
}