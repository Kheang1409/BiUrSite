FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /src

# Copy only project files first to leverage Docker layer caching for restore
COPY backend.sln ./
COPY API/Api.csproj ./API/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY SharedKernel/SharedKernel.csproj ./SharedKernel/

# Restore dependencies
RUN dotnet restore ./API/Api.csproj

# Copy the rest of the source
COPY . ./

# Build and publish (no restore again); strip debug symbols for smaller output
# Avoid ReadyToRun here to keep framework-dependent publish without RID requirements
RUN dotnet publish ./API/Api.csproj -c Release -o /app/publish --no-restore \
	-p:DebugType=None -p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Listen on 8080 inside the container (matches docker-compose mapping)
ENV ASPNETCORE_URLS=http://+:8080 \
	DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]