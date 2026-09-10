FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src

RUN dotnet nuget add source https://nuget.pkg.github.com/contempl/index.json \
    -n github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text

COPY ["FileService/FileService.Web/FileService.Web.csproj", "FileService/FileService.Web/"]
COPY ["FileService/FileService.Core/FileService.Core.csproj", "FileService/FileService.Core/"]
COPY ["FileService/FileService.Domain/FileService.Domain.csproj", "FileService/FileService.Domain/"]
COPY ["FileService/FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj", "FileService/FileService.Infrastructure.Postgres/"]
COPY ["FileService/FileService.Infrastructure.S3/FileService.Infrastructure.S3.csproj", "FileService/FileService.Infrastructure.S3/"]
COPY ["FileService/FileService.Contracts/FileService.Contracts.csproj", "FileService/FileService.Contracts/"]
COPY ["FileService/FileService.VideoProcessingService/FileService.VideoProcessingService.csproj", "FileService/FileService.VideoProcessingService/"]
COPY ["Shared/Messaging/RabbitMqMessaging.csproj", "Shared/Messaging/"]
RUN dotnet restore "FileService/FileService.Web/FileService.Web.csproj"
COPY . .
WORKDIR "/src/FileService/FileService.Web"
RUN dotnet build "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Web.dll"]
