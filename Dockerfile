# Usado somente pelo Render (que não tem runtime .NET nativo). Não use para desenvolvimento local.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/MOps.Core/MOps.Core.csproj src/MOps.Core/
COPY src/MOps.Infrastructure/MOps.Infrastructure.csproj src/MOps.Infrastructure/
COPY src/MOps.Api/MOps.Api.csproj src/MOps.Api/
RUN dotnet restore src/MOps.Api/MOps.Api.csproj
COPY src/MOps.Core/ src/MOps.Core/
COPY src/MOps.Infrastructure/ src/MOps.Infrastructure/
COPY src/MOps.Api/ src/MOps.Api/
RUN dotnet publish src/MOps.Api -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "MOps.Api.dll"]
