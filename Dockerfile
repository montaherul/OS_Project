FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EDFRR/EDFRR.csproj", "EDFRR/"]
RUN dotnet restore "EDFRR/EDFRR.csproj"

COPY . .
WORKDIR "/src/EDFRR"
RUN dotnet build "EDFRR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EDFRR.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends libgdiplus libc6-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "EDFRR.dll"]
