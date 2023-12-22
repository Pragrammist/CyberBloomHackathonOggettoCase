FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CyberBoom.csproj", "CyberBoom.csproj"]
RUN dotnet restore "CyberBoom.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CyberBoom.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CyberBoom.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CyberBoom.dll"]
