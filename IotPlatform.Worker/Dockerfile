FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IotPlatform.Worker/IotPlatform.Worker.csproj", "IotPlatform.Worker/"]
COPY ["IotPlatform.Core/IotPlatform.Core.csproj", "IotPlatform.Core/"]
COPY ["IotPlatform.Infrastructure/IotPlatform.Infrastructure.csproj", "IotPlatform.Infrastructure/"]
RUN dotnet restore "IotPlatform.Worker/IotPlatform.Worker.csproj"
COPY . .
WORKDIR "/src/IotPlatform.Worker"
RUN dotnet build "IotPlatform.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IotPlatform.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IotPlatform.Worker.dll"]