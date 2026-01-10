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
RUN dotnet publish "Lyn.Web.csproj" -c Release -o /app/publish /p:RunAOTCompilation=false

# Inject BACKEND_URL into appsettings.json after publish
RUN if [ -n "$BACKEND_URL" ]; then \
      echo "{\"BACKEND_URL\": \"${BACKEND_URL}\", \"ApiBaseUrl\": \"${BACKEND_URL}\"}" > /app/publish/wwwroot/appsettings.json; \
    fi

# Verify files were published
RUN ls -la /app/publish/wwwroot/_framework/

# Serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy published WASM files
COPY --from=build /app/publish/wwwroot .

# Verify files in final image
RUN ls -la /usr/share/nginx/html/_framework/ || echo "Framework folder not found!"

# Copy nginx config
COPY --from=build /src/Lyn.Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80