FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SurveyApp.sln ./
COPY src/SurveyApp.Core/SurveyApp.Core.csproj src/SurveyApp.Core/
COPY src/SurveyApp.Application/SurveyApp.Application.csproj src/SurveyApp.Application/
COPY src/SurveyApp.Infrastructure/SurveyApp.Infrastructure.csproj src/SurveyApp.Infrastructure/
COPY src/SurveyApp.Api/SurveyApp.Api.csproj src/SurveyApp.Api/
RUN dotnet restore src/SurveyApp.Api/SurveyApp.Api.csproj

COPY src/ src/
RUN dotnet publish src/SurveyApp.Api/SurveyApp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "SurveyApp.Api.dll"]
