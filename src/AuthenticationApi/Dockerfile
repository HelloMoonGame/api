FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS "http://+:80;https://+:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path "/app/localhost.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password "plaintext"

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src/AuthenticationApi
RUN dotnet dev-certs https --export-path ./localhost.pfx -p plaintext

COPY "AuthenticationApi.csproj" .
RUN dotnet restore "AuthenticationApi.csproj"

COPY . .
RUN dotnet build "AuthenticationApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthenticationApi.csproj" -c Release -o /app/publish
COPY --from=build /src/AuthenticationApi/localhost.pfx /app/publish/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthenticationApi.dll"]