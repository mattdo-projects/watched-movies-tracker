# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["DatabaseService.Tests/DatabaseService.Tests.csproj", "DatabaseService.Tests/"]
COPY ["DatabaseService/DatabaseService.csproj", "DatabaseService/"]
RUN dotnet restore "DatabaseService.Tests/DatabaseService.Tests.csproj"

# Copy all source code and build the project
COPY . .
WORKDIR "/src/DatabaseService.Tests"
RUN dotnet build -c Release

# Run tests automatically upon container start
ENTRYPOINT ["dotnet", "test", "--logger:trx", "--verbosity", "normal"]
