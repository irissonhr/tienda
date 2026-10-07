FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TiendaApp.csproj ./
RUN dotnet restore TiendaApp.csproj

COPY . ./
RUN dotnet publish TiendaApp.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 10000

ENTRYPOINT ["dotnet", "TiendaApp.dll"]
