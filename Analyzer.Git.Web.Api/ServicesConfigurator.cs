using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Git.Application.Services.Statistics;
using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Web.Api.Hosted;
using Analyzer.Git.Web.Api.Mapper;

namespace Analyzer.Git.Web.Api
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
