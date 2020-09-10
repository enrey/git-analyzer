docker build -t 172.17.21.6:8888/analyzergitweb:latest -f Analyzer.Git.Web.Api/Dockerfile . --no-cache
docker build -t 172.17.21.6:8888/analyzergitlabweb:latest -f Analyzer.Gitlab.Web.Api/Dockerfile . --no-cache
docker build -t 172.17.21.6:8888/analyzerjiraweb:latest -f Analyzer.Jira.Web.Api/Dockerfile . --no-cache
docker push 172.17.21.6:8888/analyzergitweb:latest
docker push 172.17.21.6:8888/analyzergitlabweb:latest
docker push 172.17.21.6:8888/analyzerjiraweb:latest