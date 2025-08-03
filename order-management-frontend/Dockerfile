# ===== DOCKERFILE PARA FRONTEND =====
# Multi-stage build para optimizar tamaño de imagen final

# Stage 1: Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copiar archivos de dependencias primero (para aprovechar cache de Docker)
COPY package.json package-lock.json ./

# Instalar dependencias
RUN npm ci --only=production --no-audit --prefer-offline

# Copiar código fuente
COPY . .

# Build de la aplicación React
RUN npm run build

# Stage 2: Production stage con Nginx
FROM nginx:1.25-alpine AS production

# Crear usuario no-root para seguridad
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nextjs -u 1001

# Copiar archivos build desde stage anterior
COPY --from=build /app/dist /usr/share/nginx/html

# Configuración personalizada de nginx para React SPA
COPY <<EOF /etc/nginx/conf.d/default.conf
server {
    listen 80;
    listen [::]:80;
    server_name localhost;
    
    root /usr/share/nginx/html;
    index index.html index.htm;
    
    # Configuración para React Router (SPA)
    location / {
        try_files \$uri \$uri/ /index.html;
    }
    
    # Cache para assets estáticos
    location /assets/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml+rss application/json;
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    
    # Health check endpoint
    location /health {
        access_log off;
        return 200 "healthy\n";
        add_header Content-Type text/plain;
    }
}
EOF

# Cambiar permisos
RUN chown -R nextjs:nodejs /usr/share/nginx/html && \
    chown -R nextjs:nodejs /var/cache/nginx && \
    chown -R nextjs:nodejs /var/log/nginx && \
    chown -R nextjs:nodejs /etc/nginx/conf.d

# Cambiar a usuario no-root
USER nextjs

# Exponer puerto 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

# Comando por defecto
CMD ["nginx", "-g", "daemon off;"]
