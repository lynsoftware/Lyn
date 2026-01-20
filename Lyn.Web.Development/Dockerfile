FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["Lyn.Web/Lyn.Web.csproj", "Lyn.Web/"]
COPY ["Lyn.Shared/Lyn.Shared.csproj", "Lyn.Shared/"]
RUN dotnet restore "Lyn.Web/Lyn.Web.csproj"

# Copy all source files
COPY . .

# Build-time argument for backend URL
ARG BACKEND_URL
ENV BACKEND_URL=${BACKEND_URL}

# Publish
WORKDIR "/src/Lyn.Web"
RUN dotnet publish "Lyn.Web.csproj" -c Release -o /app/publish

# Inject BACKEND_URL into appsettings.json after publish
RUN if [ -n "$BACKEND_URL" ]; then \
    apt-get update && apt-get install -y jq && \
    jq --arg url "$BACKEND_URL" \
    '. + {BACKEND_URL: $url, ApiBaseUrl: $url}' \
    /app/publish/wwwroot/appsettings.json > /tmp/appsettings.json && \
    mv /tmp/appsettings.json /app/publish/wwwroot/appsettings.json && \
    rm -rf /var/lib/apt/lists/*; \
    fi

# Debug: List published files
RUN echo "=== Published files ===" && \
    ls -la /app/publish/wwwroot/ && \
    echo "=== Framework files ===" && \
    ls -la /app/publish/wwwroot/_framework/ | head -20

# Serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy ALL wwwroot contents (including subdirectories)
COPY --from=build /src/Lyn.Web.Development/nginx.conf /etc/nginx/nginx.conf

# Debug: Verify copy worked
RUN echo "=== Final nginx files ===" && \
    ls -la /usr/share/nginx/html/ && \
    echo "=== Final framework files ===" && \
    ls -la /usr/share/nginx/html/_framework/ | head -20

# Copy nginx config
COPY --from=build /src/Lyn.Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80