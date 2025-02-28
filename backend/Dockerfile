# Step 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:latest AS build

# Set the working directory
WORKDIR /app

# Copy project files and restore dependencies
COPY *.csproj ./  
RUN dotnet restore

# Copy the rest of the application source code
COPY . . 

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Step 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:latest

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish ./ 

# Expose the correct port
EXPOSE 8080

# Define the entrypoint to run the application
ENTRYPOINT ["dotnet", "backend.dll"]
