# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Chat.Services.API/Chat.Services.API.csproj", "Chat.Services.API/"]
RUN dotnet restore "Chat.Services.API/Chat.Services.API.csproj"
COPY . .
WORKDIR "/src/Chat.Services.API"
RUN dotnet publish "Chat.Services.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Chat.Services.API.dll"]


