FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["UserService/UserService.Presentation/UserService.Presentation.csproj", "UserService/UserService.Presentation/"]
COPY ["UserService/UserService.Application/UserService.Application.csproj", "UserService/UserService.Application/"]
COPY ["UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService/UserService.Infrastructure/"]
COPY ["UserService/UserService.Domain/UserService.Domain.csproj", "UserService/UserService.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "UserService/UserService.Presentation/UserService.Presentation.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/UserService/UserService.Presentation"
RUN dotnet build "UserService.Presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "UserService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UserService.Presentation.dll"]
