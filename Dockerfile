# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY *.sln ./
COPY ArcCorpBackend.Domain/*.csproj ./ArcCorpBackend.Domain/
COPY ArcCorpBackend.Services/*.csproj ./ArcCorpBackend.Services/
COPY ArcCorpBackend.Controllers/*.csproj ./ArcCorpBackend.Controllers/
COPY ArcCorpBackend/*.csproj ./ArcCorpBackend/

RUN dotnet restore

COPY . ./

RUN dotnet publish ArcCorpBackend/ArcCorpBackend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ArcCorpBackend.dll"]
