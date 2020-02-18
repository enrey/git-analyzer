#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
WORKDIR /src
#COPY ["GitAnalyzer.Web.Api/GitAnalyzer.Web.Api.csproj", "GitAnalyzer.Web.Api/"]
#COPY ["GitAnalyzer.Web.Contracts/GitAnalyzer.Web.Contracts.csproj", "GitAnalyzer.Web.Contracts/"]
#COPY ["GitAnalyzer.Web.Application/GitAnalyzer.Web.Application.csproj", "GitAnalyzer.Web.Application/"]
#RUN dotnet restore "GitAnalyzer.Web.Api/GitAnalyzer.Web.Api.csproj"
COPY . .
WORKDIR /src/GitAnalyzer.Web.Api
RUN dotnet publish "GitAnalyzer.Web.Api.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "GitAnalyzer.Web.Api.dll"]