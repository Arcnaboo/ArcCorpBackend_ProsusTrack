# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file first
COPY ArcCorpBackend.sln ./

# Copy csproj files for projects included in the solution
COPY ArcCorpBackend/ArcCorpBackend.csproj ArcCorpBackend/
COPY ArcCorpBackend.Domain/*.csproj ArcCorpBackend.Domain/
COPY ArcCorpBackend.core/*.csproj ArcCorpBackend.core/

# Restore dependencies
RUN dotnet restore

# Copy entire project folders
COPY ArcCorpBackend/ ArcCorpBackend/
COPY ArcCorpBackend.Domain/ ArcCorpBackend.Domain/
COPY ArcCorpBackend.core/ ArcCorpBackend.core/

# Build in release mode
WORKDIR /src/ArcCorpBackend
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ArcCorpBackend.dll"]
