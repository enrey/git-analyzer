#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
WORKDIR /src
COPY  ./ ./

WORKDIR /src/Analyzer.GitLab.Web.Api
RUN dotnet publish "Analyzer.GitLab.Web.Api.csproj" -c Debug -o /app/publish

FROM base AS finalGitLab
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analyzer.GitLab.Web.Api.dll"]
