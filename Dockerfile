# BigBall.Api — imagem de produção (Railway builda este Dockerfile a cada push)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore em camada separada para aproveitar o cache do Docker
COPY global.json Directory.Build.props ./
COPY BigBall.Api/BigBall.Api.csproj BigBall.Api/
COPY BigBall.Shared/BigBall.Shared.csproj BigBall.Shared/
COPY BigBall.Domain/BigBall.Domain.csproj BigBall.Domain/
RUN dotnet restore BigBall.Api/BigBall.Api.csproj

COPY BigBall.Api/ BigBall.Api/
COPY BigBall.Shared/ BigBall.Shared/
COPY BigBall.Domain/ BigBall.Domain/
RUN dotnet publish BigBall.Api/BigBall.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# O Railway injeta PORT em runtime; fora dele, 8080 é o fallback
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet BigBall.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
