FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cogfather.HQ.UI/Cogfather.HQ.UI.csproj", "Cogfather.HQ.UI/"]
RUN dotnet restore "Cogfather.HQ.UI/Cogfather.HQ.UI.csproj"
COPY . .
WORKDIR "/src/Cogfather.HQ.UI"
RUN dotnet build "./Cogfather.HQ.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cogfather.HQ.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cogfather.HQ.UI.dll"]
