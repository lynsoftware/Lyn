FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["Lyn.Web/Lyn.Web.csproj", "Lyn.Web/"]
COPY ["Lyn.Shared/Lyn.Shared.csproj", "Lyn.Shared/"]
RUN dotnet restore "Lyn.Web/Lyn.Web.csproj"

# Copy all source files INCLUDING nginx.conf
COPY . .

# Publish to wwwroot
WORKDIR "/src/Lyn.Web"
RUN dotnet publish "Lyn.Web.csproj" -c Release -o /app/publish

# Serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy published WASM files
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx config FROM BUILD STAGE
COPY --from=build /src/Lyn.Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80