FROM node:latest AS build

WORKDIR /app

COPY package*.json ./

RUN npm install --include=dev

COPY . .

RUN npx ng version

RUN npm run build -- --configuration=production

FROM nginx:alpine

WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=build /app/dist/frontend/ ./

# Copy SSL certificates and nginx config
COPY localhost.crt /etc/ssl/certs/localhost.crt
COPY localhost.key /etc/ssl/private/localhost.key
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 443

ENTRYPOINT ["nginx", "-g", "daemon off;"]
