FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LA.Services.API/LA.Services.API.csproj", "LA.Services.API/"]
RUN dotnet restore "LA.Services.API/LA.Services.API.csproj"
COPY . .
WORKDIR "/src/LA.Services.API"
RUN dotnet build "LA.Services.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LA.Services.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LA.Services.API.dll"]
