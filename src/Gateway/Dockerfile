FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Kairos.sln", "."]
COPY ["src/Gateway/Kairos.Gateway.csproj", "src/Gateway/"]
COPY ["src/Account/Kairos.Account.csproj", "src/Account/"]
COPY ["src/Shared/Kairos.Shared.csproj", "src/Shared/"]
COPY ["tests/Account.UnitTests/Kairos.Account.UnitTests.csproj", "tests/Account.UnitTests/"]

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

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true \
    LC_ALL=pt_BR.UTF-8 \
    LANG=pt_BR.UTF-8 \
    TZ=America/Sao_Paulo

ENTRYPOINT ["dotnet", "Kairos.Gateway.dll"]