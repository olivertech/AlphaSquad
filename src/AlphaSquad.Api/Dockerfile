FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AlphaSquad.Api/AlphaSquad.Api.csproj", "src/AlphaSquad.Api/"]
COPY ["src/AlphaSquad.Infrastructure/AlphaSquad.Infrastructure.csproj", "src/AlphaSquad.Infrastructure/"]
COPY ["src/AlphaSquad.Shared/AlphaSquad.Shared.csproj", "src/AlphaSquad.Shared/"]
RUN dotnet restore "src/AlphaSquad.Api/AlphaSquad.Api.csproj"

COPY . .
WORKDIR /src/src/AlphaSquad.Api
RUN dotnet publish "AlphaSquad.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AlphaSquad.Api.dll"]
