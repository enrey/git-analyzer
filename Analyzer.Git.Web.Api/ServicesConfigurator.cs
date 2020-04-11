using AutoMapper;
using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Services.GitLab;
using GitAnalyzer.Web.Application.MapperProfiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GitAnalyzer.Application.Services.Hosted;
using GitAnalyzer.Application.Services.Statistics;

namespace GitAnalyzer.Web.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGitStatisticsService, GitStatisticsService>();

            services.Configure<StatisticsConfig>(configuration.GetSection("Statistics"));
            services.Configure<RepositoriesConfig>(configuration.GetSection("Repositories"));
            services.Configure<WorkEstimateConfig>(configuration.GetSection("WorkEstimate"));
            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));

            services.AddAutoMapper(typeof(StatisticsMapperProfile));
            services.AddHostedService<UpdateRepositoriesHostedService>();
        }
    }
}
