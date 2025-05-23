# Use the official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the main project and test project files
COPY ["API/TMS_API.csproj", "API/"]

# Restore dependencies for both the main API and test project
RUN dotnet restore "API/TMS_API.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
RUN dotnet build "API/TMS_API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "API/TMS_API.csproj" -c Release -o /app/publish

# Use the runtime image again to serve the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TMS_API.dll"]
