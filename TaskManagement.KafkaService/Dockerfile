FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем проекты
COPY TaskManagement.Core/TaskManagement.Core.csproj TaskManagement.Core/
COPY TaskManagement.KafkaService/TaskManagement.KafkaService.csproj TaskManagement.KafkaService/

# Восстанавливаем зависимости
RUN dotnet restore TaskManagement.KafkaService/TaskManagement.KafkaService.csproj

# Копируем весь код
COPY . .

# Публикуем приложение
RUN dotnet publish TaskManagement.KafkaService/TaskManagement.KafkaService.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "TaskManagement.KafkaService.dll"]