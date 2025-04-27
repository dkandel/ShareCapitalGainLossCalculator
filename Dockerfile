FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ShareCapitalGainLossCalculator/ShareCapitalGainLossCalculator.csproj ShareCapitalGainLossCalculator/
COPY ShareCapitalGainLossCalculator.UnitTests/ShareCapitalGainLossCalculator.UnitTests.csproj ShareCapitalGainLossCalculator.UnitTests/

# Restore dependencies
RUN dotnet restore "ShareCapitalGainLossCalculator/ShareCapitalGainLossCalculator.csproj"

# Copy the rest of the source code
COPY . .

# Build the solution
RUN dotnet build "ShareCapitalGainLossCalculator/ShareCapitalGainLossCalculator.csproj" -c $BUILD_CONFIGURATION --no-restore

# Run unit tests
RUN dotnet test "ShareCapitalGainLossCalculator.UnitTests/ShareCapitalGainLossCalculator.UnitTests.csproj"

# Publish the application
RUN dotnet publish "ShareCapitalGainLossCalculator/ShareCapitalGainLossCalculator.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShareCapitalGainLossCalculator.dll"]