FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies (caching layer)
COPY ["Lyn.Backend/Lyn.Backend.csproj", "Lyn.Backend/"]
COPY ["Lyn.Shared/Lyn.Shared.csproj", "Lyn.Shared/"]
RUN dotnet restore "Lyn.Backend/Lyn.Backend.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/Lyn.Backend"
RUN dotnet build "Lyn.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lyn.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lyn.Backend.dll"]
