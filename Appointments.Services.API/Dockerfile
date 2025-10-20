FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Appointments.Services.API/Appointments.Services.API.csproj", "Appointments.Services.API/"]
RUN dotnet restore "Appointments.Services.API/Appointments.Services.API.csproj"
COPY . .
WORKDIR "/src/Appointments.Services.API"
RUN dotnet build "Appointments.Services.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Appointments.Services.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Appointments.Services.API.dll"]
