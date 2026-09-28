FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CostingTool.csproj src/
COPY src/CostingTool.Engine/CostingTool.Engine.csproj src/CostingTool.Engine/
COPY src/CostingTool.Pdf/CostingTool.Pdf.csproj src/CostingTool.Pdf/
COPY src/packages.lock.json src/

RUN dotnet restore src/CostingTool.csproj --locked-mode

COPY src/ src/
RUN dotnet publish src/CostingTool.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .
RUN apt-get update \
    && apt-get install --no-install-recommends --yes libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/* \
    && chown -R $APP_UID:$APP_UID /app

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "CostingTool.dll"]
