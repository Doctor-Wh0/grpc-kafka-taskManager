# Dockerfile в TaskManagement.Api
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем проекты
COPY TaskManagement.Core/TaskManagement.Core.csproj TaskManagement.Core/
COPY TaskManagement.Api/TaskManagement.Api.csproj TaskManagement.Api/

# Восстанавливаем зависимости
RUN dotnet restore TaskManagement.Api/TaskManagement.Api.csproj

# Копируем весь код
COPY . .

# Публикуем приложение
RUN dotnet publish TaskManagement.Api/TaskManagement.Api.csproj -c Release -o out

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
