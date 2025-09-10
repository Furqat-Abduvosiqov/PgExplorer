# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --no-audit --no-fund
COPY . .
RUN npm run build -- --configuration=production

# Runtime stage
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/pg-explorer-client/ /usr/share/nginx/html
# Nginx default.conf to route to SPA
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 4200
CMD ["nginx", "-g", "daemon off;"]
