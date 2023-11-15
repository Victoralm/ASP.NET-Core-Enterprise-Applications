#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/NSE.Pagamento.api/NSE.Pagamento.api.csproj", "src/services/NSE.Pagamento.api/"]
RUN dotnet restore "./src/services/NSE.Pagamento.api/./NSE.Pagamento.api.csproj"
COPY . .
WORKDIR "/src/src/services/NSE.Pagamento.api"
RUN dotnet build "./NSE.Pagamento.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NSE.Pagamento.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NSE.Pagamento.api.dll"]