FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 9900
EXPOSE 9901

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency restoration
COPY ["Portal.Server/Portal.Server.csproj", "Portal.Server/"]
COPY ["Portal.Data/Portal.Data.csproj", "Portal.Data/"]
COPY ["Portal.Shared/Portal.Shared.csproj", "Portal.Shared/"]
COPY ["Portal.Client/Portal.Client.csproj", "Portal.Client/"]

# Restore Server and all referenced projects (Shared, Data, Client)
RUN dotnet restore "Portal.Server/Portal.Server.csproj"

# Copy all source code
COPY . .

# Re-restore after copy so obj/ and project.assets.json use container paths only (avoids Windows NuGet path errors)
RUN dotnet restore "Portal.Server/Portal.Server.csproj"

# Build Shared project first
WORKDIR "/src/Portal.Shared"
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore

# Build Data project
WORKDIR "/src/Portal.Data"
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore

# Build and publish Client project
WORKDIR "/src/Portal.Client"
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/client -p:PublishTrimmed=true -p:TrimMode=partial --no-restore

# Build and publish Server project
WORKDIR "/src/Portal.Server"
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish "Portal.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
# Required by Npgsql for PostgreSQL connection (GSSAPI/Kerberos)
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app

# Copy the published server application
COPY --from=build /app/publish .

# Copy the client build output to the server's wwwroot
COPY --from=build /app/client/wwwroot ./wwwroot

# Set the entry point
ENTRYPOINT ["dotnet", "Portal.Server.dll"]
