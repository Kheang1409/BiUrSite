# Stage 1: Build Angular application
FROM node:latest AS build

WORKDIR /app

# Install Bun
RUN npm install -g bun

# Copy package.json and bun.lockb for better caching
COPY package*.json ./

# Install dependencies with Bun
RUN bun install

# Copy the rest of the source code
COPY . .

# Build Angular app (it will use environment.prod.ts automatically)
RUN bun run build --configuration=production

# Stage 2: Serve with Nginx
FROM nginx:alpine

WORKDIR /usr/share/nginx/html

# Remove default Nginx HTML files
RUN rm -rf ./*

# Copy built Angular files from build stage
COPY --from=build /app/dist/frontend/ ./

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for serving
EXPOSE 80

# Start Nginx server
ENTRYPOINT ["nginx", "-g", "daemon off;"]