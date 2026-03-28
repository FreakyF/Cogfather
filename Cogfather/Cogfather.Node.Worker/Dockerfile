FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cogfather.Node.Worker/Cogfather.Node.Worker.csproj", "Cogfather.Node.Worker/"]
RUN dotnet restore "Cogfather.Node.Worker/Cogfather.Node.Worker.csproj"
COPY . .
WORKDIR "/src/Cogfather.Node.Worker"
RUN dotnet build "./Cogfather.Node.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cogfather.Node.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cogfather.Node.Worker.dll"]
