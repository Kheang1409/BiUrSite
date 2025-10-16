FROM mcr.microsoft.com/dotnet/sdk:latest AS build

WORKDIR /src

COPY API/Api.csproj ./API/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY SharedKernel/SharedKernel.csproj ./SharedKernel/

RUN dotnet restore API/Api.csproj

COPY . ./

RUN dotnet publish API/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:latest
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]