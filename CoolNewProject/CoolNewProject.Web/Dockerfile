FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoolNewProject/CoolNewProject.csproj", "CoolNewProject/"]
RUN dotnet restore "CoolNewProject/CoolNewProject.csproj"
COPY . .
WORKDIR "/src/CoolNewProject"
RUN dotnet build "CoolNewProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoolNewProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoolNewProject.dll"]
