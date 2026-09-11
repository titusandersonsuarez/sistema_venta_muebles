FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Cache-friendly: primero csproj para restaurar deps sin invalidar la
# capa cuando cambia código.
COPY NogalApi.csproj .
RUN dotnet restore "NogalApi.csproj"

COPY . .
RUN dotnet publish "NogalApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Ejecutamos como usuario no-root (mejor postura de seguridad).
RUN useradd --create-home --uid 1001 nogal && chown -R nogal:nogal /app
USER nogal

COPY --from=build --chown=nogal:nogal /app/publish .

# Kestrel escucha en 8080 dentro del contenedor.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "NogalApi.dll"]
