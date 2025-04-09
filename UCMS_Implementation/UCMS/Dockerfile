# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.sln .
COPY */*.csproj ./src/
RUN for file in src/*.csproj; do dotnet restore "$file"; done

# Copy the rest of the source code
COPY . .

# Publish the app (self-contained if needed)
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Optional: Set environment variables (e.g., ASPNETCORE_ENVIRONMENT=Production)
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "YourAppName.dll"]
