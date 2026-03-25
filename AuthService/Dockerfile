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


COPY AuthService.Web/*.csproj ./AuthService.Web/
RUN dotnet restore "AuthService.Web/AuthService.Web.csproj"
COPY . .
WORKDIR "/src/AuthService.Web"
RUN dotnet build "./AuthService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Web.dll"]
