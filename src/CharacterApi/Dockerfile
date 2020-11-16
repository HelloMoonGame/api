FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src/CharacterApi

COPY "CharacterApi.csproj" .
RUN dotnet restore "CharacterApi.csproj"

COPY . .
RUN dotnet build "CharacterApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CharacterApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CharacterApi.dll"]