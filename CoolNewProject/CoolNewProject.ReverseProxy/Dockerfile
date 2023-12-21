FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CoolNewProject.ReverseProxy/CoolNewProject.ReverseProxy.csproj", "CoolNewProject.ReverseProxy/"]
RUN dotnet restore "CoolNewProject.ReverseProxy/CoolNewProject.ReverseProxy.csproj"
COPY . .
WORKDIR "/src/CoolNewProject.ReverseProxy"
RUN dotnet build "CoolNewProject.ReverseProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoolNewProject.ReverseProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoolNewProject.ReverseProxy.dll"]
