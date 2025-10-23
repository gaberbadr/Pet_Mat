# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files in dependency order
COPY CoreLayer/*.csproj ./CoreLayer/
COPY RepositoryLayer/*.csproj ./RepositoryLayer/
COPY ServiceLayer/*.csproj ./ServiceLayer/
COPY petmat/*.csproj ./petmat/

# Restore dependencies
WORKDIR /src/petmat
RUN dotnet restore

# Copy everything else
WORKDIR /src
COPY . .

# Build and publish
WORKDIR /src/petmat
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create directories with proper permissions
RUN mkdir -p /app/wwwroot/files && \
    chmod -R 755 /app/wwwroot && \
    chmod -R 777 /app/wwwroot/files

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check using swagger endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=5 \
    CMD curl -f http://localhost:8080/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "petmat.dll"]