FROM mcr.microsoft.com/dotnet/sdk:latest AS build

WORKDIR /src

COPY API/Api.csproj ./API/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/

RUN dotnet restore ./API/Api.csproj

COPY . ./
RUN dotnet build ./API/Api.csproj -c Release -o /app/build

RUN dotnet publish ./API/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:latest
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]