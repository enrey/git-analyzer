using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Git.Application.Configuration;
using Analyzer.GitLab.Web.Api.Mapper;
using Analyzer.Gitlab.Application.Services;
using Analyzer.GitLab.Web.Api.Hosted;

namespace Analyzer.GitLab.Web.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGitLabService, GitLabService>();
            services.AddTransient<GitLabElasticService>();

            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));
            services.Configure<ElasticConfig>(configuration.GetSection("ElasticSearch"));

            services.AddAutoMapper(typeof(GitLabMapperProfile));

            services.AddHostedService<ProducerHostingService>();
        }
    }
}
