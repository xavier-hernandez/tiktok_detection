##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine AS base
#RUN apk add --update --no-cache python3 py3-pip bash
#RUN pip install apprise --break-system-packages
#RUN apk add chromium
#
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["TikTokDetection.csproj", "."]
#RUN dotnet restore "./TikTokDetection.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "./TikTokDetection.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./TikTokDetection.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TikTokDetection.dll"]

FROM xavierh/emgucv:8.0-chrome-1097615

RUN pip install apprise

ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["TikTokDetection.csproj", "."]
RUN dotnet restore "./TikTokDetection.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "./TikTokDetection.csproj" -c $BUILD_CONFIGURATION -o /app /p:UseAppHost=false

WORKDIR /app
ENTRYPOINT ["dotnet", "TikTokDetection.dll"]