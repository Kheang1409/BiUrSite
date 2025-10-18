## Stage 1: Build Angular application
FROM node:20-alpine AS deps
WORKDIR /app

# Install dependencies separately to improve caching
COPY package*.json ./
RUN npm install --legacy-peer-deps && npm cache clean --force

FROM node:20-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build Angular app for production (default configuration is production)
RUN npm run build -- --configuration=production

## Stage 2: Serve with Nginx
FROM nginx:1.27-alpine AS runtime

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Remove default Nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy built Angular files from build stage
COPY --from=build /app/dist/frontend /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start Nginx server
CMD ["nginx", "-g", "daemon off;"]
