FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS http://*:5001
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY API/API.csproj API/
RUN dotnet restore "API/API.csproj"
COPY . .
RUN dotnet build "API/API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["/DB/dev", "/DB/"]
EXPOSE 5001
ENTRYPOINT ["dotnet", "API.dll"]