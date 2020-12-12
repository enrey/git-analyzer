using Analyzer.Git.Application.Dto;
using Analyzer.Git.Web.Api.Dto;
using AutoMapper;

namespace Analyzer.Git.Web.Api.Mapper
{
    /// <summary>
    /// Профиль маппинга для DTO Statistics
    /// </summary>
    public class StatisticsMapperProfile : Profile
    {
        public StatisticsMapperProfile()
        {
            CreateMap<PersonStatisticsDto, PersonStatisticsContract>();
            CreateMap<PeriodStatisticsDto, PeriodStatisticsContract>()
                .ForMember(t => t.Date, o => o.MapFrom(s => s.Date.ToString("yyyy-MM-dd")));
            CreateMap<RepositoryStatisticsDto, RepositoryStatisticsContract>();

            CreateMap<PersonWorkEstimateDto, PersonWorkEstimateContract>();
            CreateMap<RepositoryWorkEstimateDto, RepositoryWorkEstimateContract>();
            CreateMap<RepositoryLastCommitDto, RepositoryLastCommitContract>();
        }
    }
}