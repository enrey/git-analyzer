using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analyzer.Git.Application.Dto;
using Analyzer.Git.Application.Dto.GitLab;
using Analyzer.Gitlab.Application.Dto;
using Analyzer.Gitlab.Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        private readonly GitLabElasticService _gitLabElasticService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Контроллер для работы с данными из GitLab
        /// </summary>
        public GitLabController(IGitLabService gitLabService, GitLabElasticService gitLabElasticService, IMapper mapper)
        {
            _gitLabService = gitLabService;
            _gitLabElasticService = gitLabElasticService;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение статистики по мерджреквестам из GitLab'а
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<UserMergeRequestsStatisicsDto>), StatusCodes.Status200OK)]
        public IActionResult GetMergeRequests(DateTime startDate, DateTime endDate)
        {
            var dtos = _gitLabElasticService.GetMergeRequestsStatistics(startDate, endDate);

            //var result = _mapper.Map<IEnumerable<UserMergeRequestsStatisicsContract>>(dtos);

            return Ok(dtos);
        }


        /// <summary>
        /// Получение статистики по мерджреквест комментам из GitLab'а
        /// </summary>
        /// <param name="startDate">Дата начала периода в формате YYYY-MM-DD</param>
        /// <param name="endDate">Дата окончания периода в формате YYYY-MM-DD</param>
        /// <returns></returns>
        [HttpGet("comments/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<CommentsStatisicsDto>), StatusCodes.Status200OK)]
        public IActionResult GetMergeRequestsComments(DateTime startDate, DateTime endDate)
        {
            var dtos = _gitLabElasticService.GetMergeRequestsCommentsStatistics(startDate, endDate);

            return Ok(dtos);
        }

        /// <summary>
        /// Получение пользователей GitLab'а
        /// </summary>
        [HttpGet("gitlabUsers")]
        [ProducesResponseType(typeof(IEnumerable<UsersDto>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Обновление данных в elastic
        /// </summary>
        /// <returns></returns>
        [HttpGet("update-elastic")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdateRepositories()
        {
            _gitLabElasticService.Update();

            return NoContent();
        }
    }
}