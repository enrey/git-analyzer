using Analyzer.Git.Application.Dto.GitLab;
using Analyzer.GitLab.Web.Api.Dto;
using AutoMapper;

namespace Analyzer.GitLab.Web.Api.Mapper
{
    /// <summary>
    /// Профиль маппинга для данных GitLab'а
    /// </summary>
    public class GitLabMapperProfile : Profile
    {
        /// <summary>
        /// Профиль маппинга для данных GitLab'а
        /// </summary>
        public GitLabMapperProfile()
        {
            CreateMap<UserMergeRequestsStatisicsDto, UserMergeRequestsStatisicsContract>();
            CreateMap<UserMergeRequestsStatisicsItemDto, UserMergeRequestsStatisicsItemContract>();
        }
    }
}
