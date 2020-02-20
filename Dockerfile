#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
WORKDIR /src
COPY ["GitAnalyzer.Web.Api/", "GitAnalyzer.Web.Api/"]
COPY ["GitAnalyzer.Web.Contracts/", "GitAnalyzer.Web.Contracts/"]
COPY ["GitAnalyzer.Web.Application/", "GitAnalyzer.Web.Application/"]
WORKDIR /src/GitAnalyzer.Web.Api
RUN dotnet publish "GitAnalyzer.Web.Api.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitAnalyzer.Web.Api.dll"]