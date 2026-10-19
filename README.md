# Events API

Система для управления мероприятиями.

# Технологии

- .NET 10
- C#
- Entity Framework Core
- PostgreSQL
- OpenAPI
- Swagger UI
- xUnit
- FluentAssertions
- Docker
- Apache Kafka 

## Архитектура

Система представляет из себя три атомарных микросервиса, каждый со своей базой данных.

AuthService — Сервис управляющий регистрацией, авторизацией и аутентификацией.
EventsService — Сервис управления событиями.
BookingsService — Сервис бронирования.
Common — Общие контракты для взаимодействия.

## Запуск

1 Из каталога `EventAPI` скопировать `.env.example` в `.env` и заменить `JWT_SECRET_KEY` на длинный случайный секрет. Затем запустить всю систему: 
```bash
docker compose up -d
```

2 Альтернатива: для локального запуска без контейнеризации API (из каталога `EventAPI`, после запуска инфраструктуры)
```bash
dotnet run --project AuthService/Auth.Api/Auth.Api.csproj
dotnet run --project EventService/Events.Api/Events.Api.csproj
dotnet run --project BookingService/Booking.Api/Booking.Api.csproj
```

| Компонент / Сервис | Порт на хосте   | Назначение / БД                       |
|--------------------|-----------------|---------------------------------------|
| **Auth.Api**       | `5255` / `7090` | Имя БД: `auth`                       |
| **Events.Api**     | `5138` / `7262` | Имя БД: `events`                      |
| **Bookings.Api**   | `5047` / `7275` | Имя БД: `bookings`                    |



## Эндпоинты

#### Auth (`:5255`)
* `POST /auth/register` — Регистрация нового пользователя (Коды: 204, 400, 409)
* `POST /auth/login` — Аутентификация и выдача JWT (Коды: 200, 400, 401)
* `GET /users/{id}` — Получить профиль пользователя (Коды: 200, 401, 403, 404)

#### Events (`:5138`)
* `GET /events` — Получить список событий с пагинацией и фильтрацией (Коды: 200, 401)
* `GET /events/{id}` — Получить событие по ID (Коды: 200, 401, 404)
* `POST /events` — Создать новое событие `[Admin]` (Коды: 201, 400, 401, 403)
* `PUT /events/{id}` — Редактировать событие `[Admin]` (Коды: 204, 400, 401, 403, 404)
* `DELETE /events/{id}` — Удалить событие `[Admin]` (Коды: 200, 401, 403, 404)

#### Bookings (`:5047`)
* `POST /bookings/{eventId}` — Создать заявку на бронирование места (Коды: 202, 401, 404, 409)
* `GET /bookings/{id}` — Получить текущий статус брони (Коды: 200, 401, 403, 404)
* `DELETE /bookings/{id}` — Отменить бронирование (Коды: 200, 401, 403, 404)

## ❌ Обработка ошибок (Error Responses)
В случае неуспешного запроса API возвращает стандартный формат ошибок **Problem Details (RFC 7807)**.

### Коды ответов при ошибках
| Код                  | Причина                               | Пример сценария                                        |
|:---------------------|:--------------------------------------|:-------------------------------------------------------|
| `400 Bad Request`    | Ошибка валидации параметров.          | Не передан обязательный параметр.                      |
| `404 Not Found`      | Ресурс не найден.                     | Запрос несуществующего события.                        |
| `409 Conflict`       | Отсутствие доступных мест на событии. | Попытка забронировать места при отсутствии доступных.  |
| `500 Internal Error` | Внутренняя ошибка сервера.            | Непредвиденный сбой в базе данных или коде.            |


## Модели данных
### Событие
| Поле           | Тип      | Описание                                   |
|----------------|----------|--------------------------------------------|
| Id             | Guid     | Идентификатор                              |
| Title          | string   | Наименование                               |
| Description    | int      | Описание                                   |
| StartAt        | DateTime | Дата начала                                |
| EndAt          | DateTime | Дата окончания                             |
| TotalSeats     | int      | Общее количество мест                      |
| AvailableSeats | int      | Доступное количество мест для бронирования |

### Бронь
| Поле        | Тип           | Описание                                                                                                  |
|-------------|---------------|-----------------------------------------------------------------------------------------------------------|
| Id          | Guid          | Идентификатор брони                                                                                       |
| EventId     | Guid          | Идентификатор события                                                                                     |
| Status      | BookingStatus | Статус бронирования: Pending (создана, ожидает обработки), Confirmed (подтверждена), Rejected (отклонена) |
| CreatedAt   | DateTime      | Дата и время создания брони                                                                               |
| ProcessedAt | DateTime      | Дата и время обработки брони                                                                              |

### Сценарии
Овербукинг:
- Создаёте событие с TotalSeats = 10 через POST /events.
- Успешно бронируете 10 мест через POST /bookings/{eventId} (например, 10 разных пользователей).
- Пытаетесь забронировать 11-е место.
- Система возвращает ответ 409 Conflict с сообщением о том, что свободных мест нет.

Ожидаемый результат: бронь не создаётся, количество AvailableSeats остаётся равным 0.

Обработка заявок на бронирование выполняется в фоновом сервисе, при бронирование заявки находится в статусе Pending, 
после обработки в фоновом процессе заявка подтверждается или откланяется.

### Аутентификация и авторизация
В проекте реализована JWT-аутентификация с разделением прав на анонимных и авторизованных пользователей, использующая заголовок Authorization: Bearer <token>. Настройка секрета JWT выполняется через appsettings.Development.json для локальной разработки и переменные окружения (Jwt__SecretKey) в продакшене.

В системе две роли admin и user. В системе есть ограничения на запросы, только роле админ доступны следующие:
Создать новое событие, обновить существующее событие, удалить событие.

#### Инструкция по получению и использованию JWT-токена 
0. Откройте SWAGGER 
1. Выполните запрос к эндпоинту входа (`POST /auth/login`), передав валидные логин и пароль в JSON-теле запроса.
2. Из полученного JSON-ответа скопируйте текстовое значение строки токена.
3. В правом верхнем углу интерфейса Swagger нажмите кнопку Authorize.
4. В открывшемся поле ввода напишите слово Bearer, затем пробел и вставьте скопированный токен.
5. Нажмите кнопку Authorize, а затем Close. Теперь защищенные эндпоинты станут доступны для тестирования.


## Тестирование
Запуск всех тестов решения:
```bash
dotnet test
```
Интеграционные тесты проверяют взаимодействие приложения с реальной базой данных PostgreSQL. Для запуска PostgreSQL в тестах используется **Testcontainers**. Перед запуском интеграционных тестов необходимо, чтобы Docker был установлен и запущен.Testcontainers автоматически запускает временный контейнер PostgreSQL для тестов и удаляет его после завершения тестов. Благодаря этому интеграционные тесты не используют локальную базу данных разработчика.


## Работа с миграцией данных
```bash
dotnet ef migrations add <Имя> --project AuthService/Auth.Infrastructure --startup-project AuthService/Auth.Api
dotnet ef migrations add <Имя> --project EventService/Events.Infrastructure --startup-project EventService/Events.Api
dotnet ef migrations add <Имя> --project BookingService/Bookings.Infrastructure --startup-project BookingService/Bookings.Api

# Применение существующих миграций
dotnet ef database update --project AuthService/Auth.Infrastructure --startup-project AuthService/Auth.Api
dotnet ef database update --project EventService/Events.Infrastructure --startup-project EventService/Events.Api
dotnet ef database update --project BookingService/Booking.Infrastructure --startup-project BookingService/Booking.Api


## Запуск всей системы через Docker Compose

Из каталога `EventAPI`:

```bash
cp .env.example .env
# Задайте собственный JWT_SECRET_KEY в .env (не менее 32 байт)
docker compose up --build -d
```

Auth: http://localhost:5255/swagger; Events: http://localhost:5138/swagger; Bookings: http://localhost:5047/swagger; Kafka UI: http://localhost:8082.
В контейнерах установлен `ASPNETCORE_ENVIRONMENT=Development`, поэтому Swagger доступен. Настройки Kafka передаются как `Kafka__BootstrapServers=kafka:29092` и `Kafka__ConsumerGroup`; JWT-настройки и строки подключения передаются через переменные окружения. При первом запуске PostgreSQL создаёт базы `auth`, `events`, `bookings` с помощью `init.sql`; миграции EF Core применяются при запуске API. Для повторного выполнения init.sql требуется новый том БД.

## Поток BookingConfirmed

После обработки заявки фоновый обработчик Bookings переводит бронь в `Confirmed` и публикует событие `BookingConfirmed` в Kafka-топик `booking-confirmed` с ключом `EventId`. Сервис Events получает сообщение в consumer group `events-service-group`, находит событие и уменьшает `AvailableSeats`. При отсутствии мест обработчик не должен допускать отрицательного остатка. Публикация и обработка асинхронны: обновление доступных мест может происходить с задержкой.