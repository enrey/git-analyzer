using AutoMapper;
using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Services.GitLab;
using GitAnalyzer.Web.Application.MapperProfiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace GitAnalyzer.Web.GitLab.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGitLabService, GitLabService>();

            services.Configure<RepositoriesConfig>(configuration.GetSection("Repositories"));
            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));

            services.AddAutoMapper(typeof(GitLabMapperProfile));
        }
    }
}
