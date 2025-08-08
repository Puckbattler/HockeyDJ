FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HockeyDJ.csproj", "."]
RUN dotnet restore "HockeyDJ.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "HockeyDJ.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HockeyDJ.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for HTTPS
ENV ASPNETCORE_URLS="https://+:5001;http://+:5000"
ENV ASPNETCORE_ENVIRONMENT="Development"
ENV ASPNETCORE_HTTPS_PORT=5001

ENTRYPOINT ["dotnet", "HockeyDJ.dll"]