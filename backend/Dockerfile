FROM mcr.microsoft.com/dotnet/sdk:latest AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .

RUN dotnet ef migrations add Initial

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:latest

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "backend.dll"]
