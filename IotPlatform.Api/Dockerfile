# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IotPlatform.Api/IotPlatform.Api.csproj", "IotPlatform.Api/"]
COPY ["IotPlatform.Core/IotPlatform.Core.csproj", "IotPlatform.Core/"]
COPY ["IotPlatform.Infrastructure/IotPlatform.Infrastructure.csproj", "IotPlatform.Infrastructure/"]
RUN dotnet restore "./IotPlatform.Api/IotPlatform.Api.csproj"
COPY . .
WORKDIR "/src/IotPlatform.Api"
RUN dotnet build "./IotPlatform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IotPlatform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "IotPlatform.Api.dll"]



# Добавьте эти строки
RUN mkdir -p /https
COPY ./https/iotplatformapi.pfx /https/
RUN chmod 644 /https/iotplatformapi.pfx

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IotPlatform.Api.dll"]