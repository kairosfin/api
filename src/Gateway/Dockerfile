FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Kairos.sln", "."]
COPY ["src/Gateway/Kairos.Gateway.csproj", "src/Gateway/"]
COPY ["src/Account/Kairos.Account.csproj", "src/Account/"]

RUN dotnet restore "Kairos.sln"

COPY . .

WORKDIR /src/src/Gateway
RUN dotnet publish "Kairos.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

RUN adduser --system --group kairos
USER kairos

ENTRYPOINT ["dotnet", "Kairos.Gateway.dll"]