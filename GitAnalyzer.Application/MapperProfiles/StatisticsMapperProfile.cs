using AutoMapper;
using GitAnalyzer.Application.Dto.Statistics;
using GitAnalyzer.Web.Contracts.Statistics;

namespace GitAnalyzer.Application.MapperProfiles
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
        }
    }
}
