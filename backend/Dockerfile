# Step 1: Build the .NET application (build image)
FROM mcr.microsoft.com/dotnet/sdk:latest AS build

# Set the working directory
WORKDIR /app

# Copy project files and restore dependencies
COPY *.csproj ./ 
RUN dotnet restore

# Install EF Core CLI tools globally (build image)
RUN dotnet tool install --global dotnet-ef

# Ensure EF Core tools are available in PATH (build image)
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the application source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Step 2: Create the runtime image (runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:latest

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build image
COPY --from=build /app/publish .

# Expose the port for the application
EXPOSE 8080

# Start the application when the container runs
ENTRYPOINT ["dotnet", "backend.dll"]