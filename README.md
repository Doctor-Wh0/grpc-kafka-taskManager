# Task Management Service

Реализация выполнена в сборке docker-compose

старт: docker-compose up --build или docker compose up --build
выключение: docker-compose down -v (остановка и удаление контейнеров вместе с volumes)

1) Структуру папок можно организовать разными путями: Infrastructure, Contracts, Data...
 В данном случае, текущий - самый компактный. Всё зависит от размеров проекта.

2) Запуск Swagger с АПИ по адресу http://localhost:5030/swagger/index.html

TODO:
1) Написать цветовой вывод сообщений Update - жёлтый, Delete - красный, Create - зелёный

2) Дописать автоматическую очистку старых токенов

3) Доработать контейнер KafkaService - периодически падает. Поставить depends_on

4) Доработать CQRS с помощью Mediatr.

5) Дописать больше тестов.

# Задание 2
Приведено в файле task2.sql
Выполнено для Postgresql