FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["BookingService/BookingService.Presentation/BookingService.Presentation.csproj", "BookingService/BookingService.Presentation/"]
COPY ["BookingService/BookingService.Application/BookingService.Application.csproj", "BookingService/BookingService.Application/"]
COPY ["BookingService/BookingService.Infrastructure/BookingService.Infrastructure.csproj", "BookingService/BookingService.Infrastructure/"]
COPY ["BookingService/BookingService.Domain/BookingService.Domain.csproj", "BookingService/BookingService.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "BookingService/BookingService.Presentation/BookingService.Presentation.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/BookingService/BookingService.Presentation"
RUN dotnet build "BookingService.Presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "BookingService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BookingService.Presentation.dll"]
