FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Users.Services.API/Users.Services.API.csproj", "Users.Services.API/"]
RUN dotnet restore "Users.Services.API/Users.Services.API.csproj"
COPY . .
WORKDIR "/src/Users.Services.API"
RUN dotnet build "Users.Services.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Users.Services.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.Services.API.dll"]
