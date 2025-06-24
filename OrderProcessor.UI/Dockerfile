FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
# Restore solution rather than individual project
RUN dotnet restore "LeawareProcessingOrders.sln"

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Build solution
RUN dotnet build "LeawareProcessingOrders.sln" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/OrderProcessor.UI
RUN dotnet publish "OrderProcessor.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy project files needed for EF migrations
COPY OrderProcessor.Infrastructure /app/OrderProcessor.Infrastructure
COPY OrderProcessor.Domain /app/OrderProcessor.Domain
COPY OrderProcessor.Application /app/OrderProcessor.Application
COPY OrderProcessor.UI /app/OrderProcessor.UI
COPY LeawareProcessingOrders.sln /app/

COPY OrderProcessor.UI/entrypoint.sh .
RUN chmod +x ./entrypoint.sh

# Install EF Core tools in the final image
RUN dotnet tool install --global dotnet-ef

ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["./entrypoint.sh"] 