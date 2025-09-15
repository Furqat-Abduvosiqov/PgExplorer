# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci --no-audit --no-fund

# Copy source code
COPY . .

# Build the app
RUN npm run build -- --configuration=production

# Runtime stage
FROM nginx:alpine AS runtime

WORKDIR /usr/share/nginx/html

# Копируем именно browser/ (Angular SPA билды)
COPY --from=build /app/dist/pg-explorer-client/browser/ .

# Кладем свой nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 4200
CMD ["nginx", "-g", "daemon off;"]
