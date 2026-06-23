FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["FopCalculator.Web/FopCalculator.Web.csproj", "FopCalculator.Web/"]
COPY ["FopCalculator.Application/FopCalculator.Application.csproj", "FopCalculator.Application/"]
COPY ["FopCalculator.Domain/FopCalculator.Domain.csproj", "FopCalculator.Domain/"]
COPY ["FopCalculator.Infrastructure/FopCalculator.Infrastructure.csproj", "FopCalculator.Infrastructure/"]

RUN dotnet restore "./FopCalculator.Web/FopCalculator.Web.csproj"

# Копіюємо весь інший код і збираємо проєкт
COPY . .
WORKDIR "/src/FopCalculator.Web"
RUN dotnet build "./FopCalculator.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FopCalculator.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FopCalculator.Web.dll"]