using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Git.Application.Dto;
using Analyzer.Git.Application.Dto.GitLab;
using Analyzer.Git.Application.Services.GitLab;
using Analyzer.Gitlab.Application.Dto;
using Analyzer.GitLab.Web.Api.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Analyzer.GitLab.Web.Api.Controllers
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


        /// <summary>
        /// Получение статистики по мерджреквестам из GitLab'а
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("comments/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<CommentsStatisicsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMergeRequestsComments(DateTime startDate, DateTime endDate)
        {
            var dtos = await _gitLabService.GetMergeRequestsCommentsStatistics(startDate, endDate);

            return Ok(dtos);
        }

        /// <summary>
        /// Получение пользователей GitLab'а
        /// </summary>
        [HttpGet("gitlabUsers")]
        [ProducesResponseType(typeof(IEnumerable<UserMergeRequestsStatisicsContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGitlabUsers()
        {
            var dtos = await _gitLabService.GetUsers();

            return Ok(dtos);
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

        /// <summary>
        /// Возвращает список активных с начала указанной даты репозиториев 
        /// <param name="sinceDate">Начало периода активности в формате YYYY-MM-DD</param>
        /// </summary>
        [HttpGet("gitlab-active-repositories/{sinceDate}")]
        [ProducesResponseType(typeof(IEnumerable<RepositoryInfoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveRepositories(DateTime sinceDate)
        {
            var repositories = await _gitLabService.GetActiveRepositories(sinceDate);

            return Ok(repositories);
        }
    }
}