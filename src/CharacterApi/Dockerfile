FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS "https://+:80;https://+:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path "/app/localhost.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password "plaintext"

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src/CharacterApi
RUN dotnet dev-certs https --export-path ./localhost.pfx -p plaintext

COPY "CharacterApi.csproj" .
RUN dotnet restore "CharacterApi.csproj"

COPY . .
RUN dotnet build "CharacterApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CharacterApi.csproj" -c Release -o /app/publish
COPY --from=build /src/CharacterApi/localhost.pfx /app/publish/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CharacterApi.dll"]