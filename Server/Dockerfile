FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and project files
COPY ["Trivare.sln", "."]
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain.Tests/Domain.Tests.csproj", "Domain.Tests/"]
COPY ["Application.Tests/Application.Tests.csproj", "Application.Tests/"]
COPY ["Infrastructure.Tests/Infrastructure.Tests.csproj", "Infrastructure.Tests/"]

# Restore dependencies
RUN dotnet restore "Trivare.sln"

# Copy all source code
COPY . .

# Publish the Api project
WORKDIR /src/Api
RUN dotnet publish "Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render will set PORT env var, ASP.NET Core will use it)
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "Api.dll"]
