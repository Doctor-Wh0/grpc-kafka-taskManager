# Dockerfile в TaskManagement.GrpcService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем проекты
COPY TaskManagement.Core/TaskManagement.Core.csproj TaskManagement.Core/
COPY TaskManagement.GrpcService/TaskManagement.GrpcService.csproj TaskManagement.GrpcService/ 

# Восстанавливаем зависимости
RUN dotnet restore TaskManagement.GrpcService/TaskManagement.GrpcService.csproj

# Копируем весь код
COPY . .

# Публикуем приложение
RUN dotnet publish TaskManagement.GrpcService/TaskManagement.GrpcService.csproj -c Release -o out

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "TaskManagement.GrpcService.dll"]
