FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/PartnerIntegration.Api/PartnerIntegration.Api.csproj", "src/PartnerIntegration.Api/"]

RUN dotnet restore "src/PartnerIntegration.Api/PartnerIntegration.Api.csproj"

COPY . .

RUN dotnet publish \
    "src/PartnerIntegration.Api/PartnerIntegration.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PartnerIntegration.Api.dll"]