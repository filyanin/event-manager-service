FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["EventService/EventService.Presentation/EventService.Presentation.csproj", "EventService/EventService.Presentation/"]
COPY ["EventService/EventService.Application/EventService.Application.csproj", "EventService/EventService.Application/"]
COPY ["EventService/EventService.Infrastructure/EventService.Infrastructure.csproj", "EventService/EventService.Infrastructure/"]
COPY ["EventService/EventService.Domain/EventService.Domain.csproj", "EventService/EventService.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "EventService/EventService.Presentation/EventService.Presentation.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/EventService/EventService.Presentation"
RUN dotnet build "EventService.Presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EventService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EventService.Presentation.dll"]
