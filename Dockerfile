FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution and projects
COPY *.sln .
COPY src/BankingEventSourcing.Domain/*.csproj ./src/BankingEventSourcing.Domain/
COPY src/BankingEventSourcing.Application/*.csproj ./src/BankingEventSourcing.Application/
COPY src/BankingEventSourcing.Infrastructure/*.csproj ./src/BankingEventSourcing.Infrastructure/
COPY src/BankingEventSourcing.API/*.csproj ./src/BankingEventSourcing.API/

# Restore packages
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /source/src/BankingEventSourcing.API
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BankingEventSourcing.API.dll"]
