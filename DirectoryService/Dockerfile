FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5353

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

COPY ["DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj", "DirectoryService/src/DirectoryService.Presentation/"]
COPY ["DirectoryService/src/DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService/src/DirectoryService.Application/"]
COPY ["DirectoryService/src/DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService/src/DirectoryService.Domain/"]
COPY ["DirectoryService/src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService/src/DirectoryService.Contracts/"]
COPY ["DirectoryService/src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "DirectoryService/src/DirectoryService.Infrastructure/"]
COPY ["Shared/Messaging/RabbitMqMessaging.csproj", "Shared/Messaging/"]
RUN dotnet restore "DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj"
COPY . .
WORKDIR "/src/DirectoryService/src/DirectoryService.Presentation"
RUN dotnet build "DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]
