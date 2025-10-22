## Stage 1: Build Angular application
FROM node:20-alpine AS deps
WORKDIR /app

# Copy only package manifests to leverage Docker layer cache for dependencies
COPY package.json package-lock.json* ./

# Prefer npm ci when a lockfile is present (reproducible, faster)
RUN if [ -f package-lock.json ]; then \
			npm ci --legacy-peer-deps; \
		else \
			npm install --legacy-peer-deps; \
		fi && npm cache clean --force

FROM node:20-alpine AS build
WORKDIR /app

# Copy node_modules from deps stage (cached) and only needed source files
COPY --from=deps /app/node_modules ./node_modules

# Copy application sources. Keep this as late as possible so dependency layer is reused
COPY . .

# Build Angular app for production (default configuration is production)
RUN npm run build -- --configuration=production --output-hashing=all

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
