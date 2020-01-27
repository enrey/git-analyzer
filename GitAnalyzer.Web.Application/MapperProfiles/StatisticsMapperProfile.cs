using AutoMapper;
using GitAnalyzer.Web.Application.Dto.Statistics;
using GitAnalyzer.Web.Contracts.Statistics;

namespace GitAnalyzer.Web.Application.MapperProfiles
{
    /// <summary>
    /// Профиль маппинга для DTO Statistics
    /// </summary>
    public class StatisticsMapperProfile : Profile
    {
        public StatisticsMapperProfile()
        {
            CreateMap<PersonStatisticsDto, PersonStatisticsContract>();
            CreateMap<PeriodStatisticsDto, PeriodStatisticsContract>();
            CreateMap<RepositoryStatisticsDto, RepositoryStatisticsContract>();
        }
    }
}
