using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Git.Application.Services.GitLab;
using Analyzer.Git.Application.Configuration;
using Analyzer.GitLab.Web.Api.Mapper;

namespace Analyzer.GitLab.Web.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGitLabService, GitLabService>();

            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));

            services.AddAutoMapper(typeof(GitLabMapperProfile));
        }
    }
}
