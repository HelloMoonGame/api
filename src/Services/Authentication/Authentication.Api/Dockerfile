FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS "http://+:80;https://+:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path "/app/localhost.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password "plaintext"

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
RUN dotnet dev-certs https --export-path ./localhost.pfx -p plaintext

RUN mkdir -p /usr/share/man/man1
RUN apt-get update && apt-get install -y openjdk-11-jdk
RUN dotnet tool install --global dotnet-sonarscanner
RUN dotnet tool install --global coverlet.console
ENV PATH="$PATH:/root/.dotnet/tools"

ARG SONAR_PROJECT_KEY=authentication_api
ARG SONAR_OGRANIZAION_KEY=hellomoon
ARG SONAR_HOST_URL=https://sonarcloud.io
ARG SONAR_TOKEN=
ARG TARGET_BRANCH=
ARG PR_BRANCH=
ARG BRANCH=
ARG PR_ID=

RUN if [ -n "$PR_ID" ] && [ -n "$SONAR_TOKEN" ] ;\
  then \
  dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /o:"$SONAR_OGRANIZAION_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths=/coverage/coverage.opencover.xml \
  /d:sonar.pullrequest.base="$TARGET_BRANCH" \
  /d:sonar.pullrequest.branch="$PR_BRANCH" \
  /d:sonar.pullrequest.key="$PR_ID" \
  /d:sonar.exclusions=Common/** \
  ; elif [ -n "$SONAR_TOKEN" ] ;\
  then \
  dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /o:"$SONAR_OGRANIZAION_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths=/coverage/coverage.opencover.xml \
  /d:sonar.branch.name="$BRANCH" \
  /d:sonar.exclusions=Common/** \
  ; fi

COPY Services/Authentication/Authentication.Api/Authentication.Api.csproj Services/Authentication/Authentication.Api/
RUN dotnet restore Services/Authentication/Authentication.Api/Authentication.Api.csproj

COPY . .
RUN dotnet build Services/Authentication/Authentication.Api/Authentication.Api.csproj -c Release -o /app/build

RUN dotnet test Services/Authentication/Authentication.UnitTests/Authentication.UnitTests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=\"json,opencover\" \
  /p:CoverletOutput="/coverage/" \
  /p:MergeWith="/coverage/coverage.json"

RUN dotnet test Services/Authentication/Authentication.IntegrationTests/Authentication.IntegrationTests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=\"json,opencover\" \
  /p:CoverletOutput="/coverage/" \
  /p:MergeWith="/coverage/coverage.json"
  
RUN if [ -n "$SONAR_TOKEN" ] ; then dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN" ; fi

FROM build AS publish
RUN dotnet publish Services/Authentication/Authentication.Api/Authentication.Api.csproj -c Release -o /app/publish
COPY --from=build /src/localhost.pfx /app/publish/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authentication.Api.dll"]