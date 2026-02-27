# syntax=docker/dockerfile:1.6
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY backend.sln ./
COPY API/Api.csproj ./API/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY SharedKernel/SharedKernel.csproj ./SharedKernel/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore ./API/Api.csproj

COPY . ./

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./API/Api.csproj -c Release -o /app/publish --no-restore \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:PublishTrimmed=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

RUN apk add --no-cache curl

RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D -s /bin/sh appuser

WORKDIR /app

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=1 \
    DOTNET_GCHeapHardLimit=0x10000000 \
    COMPlus_EnableDiagnostics=0

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]