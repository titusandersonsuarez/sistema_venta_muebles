FROM node:20-alpine AS build
WORKDIR /app

# Instalamos deps con lock para builds reproducibles.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite lee VITE_* de env al build. Se puede sobrescribir con --build-arg.
ARG VITE_API_URL=/api
ENV VITE_API_URL=${VITE_API_URL}

RUN npm run build

FROM nginx:1.27-alpine AS final

# Template con ${API_UPSTREAM}: envsubst nativo de la imagen lo resuelve
# al arrancar contra el env var. Default apunta al servicio de compose;
# en Azure Container Apps lo sobrescribimos con el FQDN interno del API.
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

# Restringimos envsubst a esta variable para no romper `$host`, `$uri`, etc.
ENV NGINX_ENVSUBST_FILTER=^API_UPSTREAM$ \
    API_UPSTREAM=http://api:8080

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
  CMD wget -qO- http://localhost/ >/dev/null 2>&1 || exit 1
