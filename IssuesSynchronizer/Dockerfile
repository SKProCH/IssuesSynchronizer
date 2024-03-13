FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IssuesSynchronizer/IssuesSynchronizer.csproj", "IssuesSynchronizer/"]
RUN dotnet restore "IssuesSynchronizer/IssuesSynchronizer.csproj"
COPY . .
WORKDIR "/src/IssuesSynchronizer"
RUN dotnet build "IssuesSynchronizer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IssuesSynchronizer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IssuesSynchronizer.dll"]
