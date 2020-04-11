using AutoMapper;
using GitAnalyzer.Application.Dto.GitLab;
using GitAnalyzer.Web.Contracts.GitLab;

namespace GitAnalyzer.Web.Application.MapperProfiles
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
