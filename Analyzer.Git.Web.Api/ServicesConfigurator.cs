using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Web.Api.Hosted;
using Analyzer.Git.Web.Api.Mapper;
using Analyzer.Git.Application.Services;

namespace Analyzer.Git.Web.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<GitElasticService>();
            services.AddTransient<IGitlabServiceClient, GitlabServiceClient>();
            services.AddTransient<IGitStatisticsService, GitStatisticsService>();

            services.Configure<ElasticConfig>(configuration.GetSection("ElasticSearch"));
            services.Configure<RepositoriesConfig>(configuration.GetSection("Repositories"));
            services.Configure<WorkEstimateConfig>(configuration.GetSection("WorkEstimate"));
            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));
            services.Configure<LocalServicesConfig>(configuration.GetSection("LocalServices"));

            services.AddAutoMapper(typeof(StatisticsMapperProfile));
            services.AddHostedService<UpdateRepositoriesHostedService>();
            services.AddHostedService<ProducerHostingService>();            
        }
    }
}
