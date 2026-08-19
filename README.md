# Event Manager Service

Микросервисное приложение для управления событиями, бронированием мест и пользователями. Реализовано на **.NET 10** с использованием Clean Architecture и асинхронного взаимодействия сервисов через **Apache Kafka**.

## Содержание

- [Структура проекта](#структура-проекта)
- [Микросервисы](#микросервисы)
- [Схема взаимодействия микросервисов](#схема-взаимодействия-микросервисов)
- [Эндпоинты API](#эндпоинты-api)
- [Стратегия кеширования](#стратегия-кеширования)
- [Инструкция по запуску](#инструкция-по-запуску)
- [Наблюдаемость (Observability)](#наблюдаемость-observability)

## Структура проекта

Решение состоит из трёх независимых микросервисов, каждый из которых построен по слоистой архитектуре (Presentation / Application / Infrastructure / Domain), и общей библиотеки контрактов:

```
event-manager-service/
├── BookingService/                      # Сервис бронирований
│   ├── BookingService.Presentation/      # Web API, контроллеры, конфигурация, Dockerfile
│   ├── BookingService.Application/       # Бизнес-логика, DTO, интерфейсы сервисов
│   ├── BookingService.Infrastructure/     # EF Core, репозитории, Kafka producer/consumer
│   └── BookingService.Domain/             # Сущности, enum'ы, доменные правила
├── EventService/                         # Сервис событий (мероприятий)
│   ├── EventService.Presentation/
│   ├── EventService.Application/
│   ├── EventService.Infrastructure/
│   └── EventService.Domain/
├── UserService/                          # Сервис пользователей и аутентификации
│   ├── UserService.Presentation/
│   ├── UserService.Application/
│   ├── UserService.Infrastructure/
│   └── UserService.Domain/
└── Shared/
	└── Shared.Contracts/                  # Общие контракты интеграционных событий Kafka
		├── Configuration/                 # KafkaSettings
		├── Dtos/                          # Общие DTO
		├── Events/                        # Booking / Event / User интеграционные события
		└── Topics/                        # Названия Kafka-топиков (KafkaTopics)
```


## Микросервисы

| Сервис | Назначение | Порт (HTTP / HTTPS, dev) |
|---|---|---|
| **UserService** | Регистрация, аутентификация (JWT), управление пользователями | 5285 / 7270 |
| **EventService** | Управление событиями (мероприятиями): создание, редактирование, поиск, доступные места | 5013 / 7248 |
| **BookingService** | Бронирование мест на события, подтверждение/отклонение/отмена брони | 5142 / 7114 |

Каждый сервис использует собственную базу данных PostgreSQL (`UserServiceDb`, `EventServiceDb`, `BookingServiceDb`) и Apache Kafka для публикации/подписки на интеграционные события.

## Схема взаимодействия микросервисов

Сервисы взаимодействуют друг с другом асинхронно через **Apache Kafka** по паттерну событийной архитектуры (Event-Driven / Choreography). Прямые синхронные HTTP-вызовы между сервисами отсутствуют — состояние синхронизируется через интеграционные события.

```
┌───────────────┐        user-created           ┌───────────────┐
│  UserService   │ ─────────────────────────────▶│               │
│  (auth/users)  │        user-deleted           │     Kafka     │
└───────────────┘ ─────────────────────────────▶│               │
												   │               │
┌───────────────┐   event-created                │   Топики:     │
│  EventService  │ ─────────────────────────────▶│  booking-*    │
│   (events)     │   event-deleted                │  event-*      │
│                │ ─────────────────────────────▶│  user-*       │
│                │◀───────────────────────────── │               │
│                │   booking-created,             │               │
│                │   booking-cancelled            │               │
│                │   (резервирование/освобожд.    │               │
│                │    мест события)                │               │
└───────────────┘                                 │               │
												   │               │
┌───────────────┐   booking-created               │               │
│ BookingService │ ─────────────────────────────▶│               │
│  (bookings)    │   booking-confirmed            │               │
│                │ ─────────────────────────────▶│               │
│                │   booking-cancelled            │               │
│                │ ─────────────────────────────▶│               │
│                │   booking-expired              │               │
│                │ ─────────────────────────────▶│               │
└───────────────┘                                 └───────────────┘
```

### Список Kafka-топиков (`Shared.Contracts.Topics.KafkaTopics`)

| Топик | Продюсер | Событие / назначение |
|---|---|---|
| `booking-created` | BookingService | Создано новое бронирование |
| `booking-confirmed` | BookingService | Бронирование подтверждено администратором |
| `booking-cancelled` | BookingService | Бронирование отменено пользователем/системой |
| `booking-expired` | BookingService | Истёк срок действия бронирования |
| `event-created` | EventService | Создано новое событие |
| `event-deleted` | EventService | Событие удалено |
| `event-seats-reserved` | EventService | Изменение доступных мест события |
| `user-created` | UserService | Зарегистрирован новый пользователь |
| `user-deleted` | UserService | Пользователь удалён |

Аутентификация между сервисами и клиентами реализована через **JWT**, выпускаемый `UserService` (issuer `UserService`, audience `EventManagerServiceClients`). `EventService` и `BookingService` валидируют этот токен для защищённых эндпоинтов (`[Authorize]`, роли `admin`/`user`).

## Эндпоинты API

### UserService — `/auth`, `/users`

| Метод | Путь | Описание | Доступ |
|---|---|---|---|
| POST | `/auth/register` | Регистрация нового пользователя | Публичный |
| POST | `/auth/login` | Вход пользователя, получение JWT | Публичный |
| POST | `/users` | Создание пользователя | Роль `admin` |
| DELETE | `/users/{id}` | Удаление пользователя по ID | Роль `admin` |

### EventService — `/api/events`

| Метод | Путь | Описание | Доступ |
|---|---|---|---|
| GET | `/api/events` | Список событий с фильтрами (`title`, `from`, `to`) и пагинацией (`page`, `pageSize`) | Публичный |
| GET | `/api/events/top` | Топ-10 событий с наибольшим процентом проданных мест (кешируется, см. [Стратегия кеширования](#стратегия-кеширования)) | Публичный |
| GET | `/api/events/{id}` | Получение события по ID (кешируется, см. [Стратегия кеширования](#стратегия-кеширования)) | Публичный |
| POST | `/api/events` | Создание события | Роль `admin` |
| PUT | `/api/events/{id}` | Обновление события | Роль `admin` |
| DELETE | `/api/events/{id}` | Удаление события | Роль `admin` |

### BookingService — `/api/bookings`

| Метод | Путь | Описание | Доступ |
|---|---|---|---|
| POST | `/api/bookings` | Создание бронирования | Авторизованный пользователь |
| GET | `/api/bookings/{id}` | Получение бронирования по ID | Авторизованный пользователь |
| GET | `/api/bookings/user/my-bookings` | Список бронирований текущего пользователя | Авторизованный пользователь |
| GET | `/api/bookings/status/{status}` | Список бронирований по статусу | Роль `admin` |
| POST | `/api/bookings/{id}/cancel` | Отмена бронирования | Авторизованный пользователь (владелец) |
| POST | `/api/bookings/{id}/confirm` | Подтверждение бронирования | Роль `admin` |
| POST | `/api/bookings/{id}/reject` | Отклонение бронирования | Роль `admin` |

Все сервисы предоставляют Swagger UI в режиме Development (`/swagger`), а также эндпоинт проверки состояния `GET /health`.

## Стратегия кеширования

`EventService` использует **Redis** (`StackExchange.Redis`) для кеширования по паттерну **Cache-Aside**. Соединение с Redis (`IConnectionMultiplexer`) регистрируется в DI как singleton — клиент Redis потокобезопасен и рассчитан на переиспользование в течение всего времени жизни приложения.

Слой `Application` не зависит от конкретной библиотеки кеширования: работа с кешем изолирована за интерфейсом `ICacheService` (`Get`/`Set`/`Remove`), реализация `RedisCacheService` находится в `Infrastructure`.

### Что кешируется и почему

| Что кешируется | Ключ | TTL (по умолчанию) | Обоснование |
|---|---|---|---|
| Событие по ID (`GET /api/events/{id}`) | `event:{id}` | 300 секунд | Часто запрашиваемые данные с умеренной частотой изменений (обновление/удаление события, изменение доступных мест при бронировании). Короткий TTL — подстраховка на случай, если инвалидация по какой-то причине не сработает. |
| Топ-10 популярных событий (`GET /api/events/top`) | `events:top10` | 60 секунд | Агрегированный рейтинг, для которого небольшое устаревание некритично. Список меняется нечасто, поэтому TTL короче, а явная инвалидация после каждого бронирования не применяется — это было бы избыточно. |

Значения TTL и строка подключения к Redis вынесены в конфигурацию (`appsettings.json`, секция `Redis`) и могут быть переопределены переменными окружения (например, `Redis__ConnectionString`, `Redis__EventTtlSeconds`).

### Поведение при чтении (Cache-Aside)

1. Сервис проверяет кеш по соответствующему ключу.
2. При попадании в кеш (`hit`) — данные возвращаются сразу, обращения к базе данных не происходит.
3. При промахе (`miss`) — данные читаются из базы данных, результат сохраняется в кеш с TTL, затем возвращается клиенту.

### Поведение при изменении данных

Для отдельного события выбрана стратегия **инвалидации при записи**: при обновлении или удалении события соответствующий ключ `event:{id}` удаляется из кеша. Следующее чтение обратится к базе данных и прогреет кеш заново актуальными данными.

Кеш события инвалидируется в следующих местах:
- `EventService.UpdateEventAsync` / `DeleteEventAsync` (Application) — при изменении/удалении события через REST API.
- `BookingConfirmedEventHandler` / `BookingCancelledEventHandler` (Infrastructure/Kafka) — при изменении количества доступных мест события в результате обработки Kafka-событий `booking-confirmed` и `booking-cancelled`.

Во всех случаях соблюдается порядок операций: **сначала изменения сохраняются в базу данных, и только потом инвалидируется кеш**. Если выполнение прервётся между этими двумя шагами, база данных останется в актуальном состоянии, а кеш просто обновится при следующем обращении — устаревшие данные не «застревают» надолго.

Кеш топ-10 событий explicit-инвалидации не подвергается — он обновляется исключительно по истечении TTL, так как явная инвалидация при каждом бронировании была бы избыточной для рейтингового агрегата.

### Устойчивость к недоступности Redis

Работа с кешем полностью изолирована в `RedisCacheService`: любые ошибки при обращении к Redis (сетевые сбои, таймауты и т.д.) логируются как предупреждения, но не пробрасываются вызывающему коду. Если Redis недоступен:
- операции чтения возвращают «промах» (`default`), и сервис читает данные напрямую из базы данных;
- операции записи/удаления молча пропускаются.

Само соединение (`ConnectionMultiplexer`) создаётся с параметром `AbortOnConnectFail = false`, поэтому `EventService` корректно стартует и продолжает работать (без кеша, обращаясь к базе данных напрямую), даже если Redis на момент старта недоступен.

## Инструкция по запуску

### Требования

- .NET 10 SDK
- PostgreSQL (по одному экземпляру БД на каждый сервис либо один сервер с разными базами)
- Apache Kafka (например, через Docker: `confluentinc/cp-kafka` или `bitnami/kafka`)
- Docker (опционально, для запуска BookingService в контейнере — есть `Dockerfile`)

### 1. Настройка инфраструктуры

Поднимите PostgreSQL и Kafka, например, локально или через `docker run`/`docker-compose`. Значения по умолчанию (см. `appsettings.json` каждого сервиса):

- PostgreSQL: `localhost:5432`, пользователь/пароль `postgres`/`postgres`, базы: `UserServiceDb`, `EventServiceDb`, `BookingServiceDb`.
- Kafka: `localhost:9092`.

### 2. Конфигурация

Для каждого сервиса (`*.Presentation/appsettings.json` или `appsettings.Development.json`) при необходимости задайте:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Host=localhost;Port=5432;Database=<ServiceDb>;Username=user;Password=password"
  },
  "JwtSettings": {
	"Secret": "<секрет длиной не менее 32 символов, одинаковый для всех сервисов>",
	"Issuer": "UserService",
	"Audience": "EventManagerServiceClients"
  },
  "Kafka": {
	"BootstrapServers": "localhost:9092"
  }
}
```

> Важно: значение `JwtSettings:Secret` должно совпадать во всех сервисах, чтобы токен, выданный `UserService`, проходил валидацию в `EventService` и `BookingService`.

### 3. Применение миграций и запуск

Каждый сервис при старте автоматически применяет миграции EF Core (`dbContext.Database.Migrate()`), поэтому отдельного шага не требуется — достаточно, чтобы PostgreSQL был доступен.

Запуск через `dotnet run` (из корня репозитория):

```powershell
# UserService
dotnet run --project UserService/UserService.Presentation/UserService.Presentation.csproj

# EventService
dotnet run --project EventService/EventService.Presentation/EventService.Presentation.csproj

# BookingService
dotnet run --project BookingService/BookingService.Presentation/BookingService.Presentation.csproj
```

Либо через Visual Studio — настройте несколько стартовых проектов (Solution Properties → Startup Project → Multiple startup projects) и запустите решение `BookingService.slnx`.

### 4. Порты по умолчанию (Development)

| Сервис | HTTP | HTTPS |
|---|---|---|
| UserService | http://localhost:5285 | https://localhost:7270 |
| EventService | http://localhost:5013 | https://localhost:7248 |
| BookingService | http://localhost:5142 | https://localhost:7114 |

### 5. Проверка работоспособности

- Swagger UI: `https://localhost:<порт>/swagger`
- Health-check: `GET https://localhost:<порт>/health`

### 6. Запуск через Docker Compose (общий `docker-compose.yml`)

В корне репозитория есть общий файл [`docker-compose.yml`](docker-compose.yml), который поднимает всю инфраструктуру и все три сервиса одной командой.

Состав `docker-compose.yml`:

| Сервис | Образ / контейнер | Порт (host) | Назначение |
|---|---|---|---|
| `zookeeper` | `confluentinc/cp-zookeeper:7.5.0` | 2181 | Координация Kafka-кластера |
| `kafka` | `confluentinc/cp-kafka:7.5.0` | 9092, 29092 | Брокер сообщений (`PLAINTEXT_HOST` — для клиентов с хоста, `PLAINTEXT` — внутри сети Docker) |
| `postgres-users` | `postgres:16-alpine` | 5432 | БД `UserServiceDb` |
| `postgres-events` | `postgres:16-alpine` | 5433 | БД `EventServiceDb` |
| `postgres-bookings` | `postgres:16-alpine` | 5434 | БД `BookingServiceDb` |
| `redis` | `redis:7.2-alpine` | — (внутри сети Docker: `redis:6379`) | Кеш для `event-service` (см. [Стратегия кеширования](#стратегия-кеширования)) |
| `user-service` | сборка из `UserService/UserService.Presentation/Dockerfile` | 5001 → 8080 | UserService |
| `event-service` | сборка из `EventService/EventService.Presentation/Dockerfile` | 5002 → 8080 | EventService |
| `booking-service` | сборка из `BookingService/BookingService.Presentation/Dockerfile` | 5003 → 8080 | BookingService |

Все сервисы запускаются с `ASPNETCORE_ENVIRONMENT=Production`, единым `JwtSettings__Secret` и подключением к Kafka через `kafka:29092`. Порядок старта контролируется через `depends_on` с `condition: service_healthy` — сервисы ждут готовности своих БД (и Kafka, где нужно) благодаря настроенным `healthcheck`.

Запуск всей системы одной командой из корня репозитория:

```powershell
docker-compose up -d
```

Полезные команды:

```powershell
# посмотреть статус контейнеров
docker-compose ps

# посмотреть логи конкретного сервиса
docker-compose logs -f booking-service

# пересобрать образы после изменений в коде и перезапустить
docker-compose up -d --build

# остановить контейнеры (данные в volumes сохранятся)
docker-compose down

# остановить и удалить контейнеры вместе с volumes (данные БД будут потеряны)
docker-compose down -v
```

После `docker-compose up -d` сервисы доступны по следующим адресам:

| Сервис | Swagger / Health |
|---|---|
| UserService | http://localhost:5001/swagger, http://localhost:5001/health |
| EventService | http://localhost:5002/swagger, http://localhost:5002/health |
| BookingService | http://localhost:5003/swagger, http://localhost:5003/health |

> Обратите внимание: порты Docker-контейнеров (5001–5003) отличаются от портов при локальном запуске через `dotnet run` ([раздел 4](#4-порты-по-умолчанию-development)).

Данные PostgreSQL и Kafka сохраняются между перезапусками в именованных volumes (`postgres_users_data`, `postgres_events_data`, `postgres_bookings_data`), все контейнеры работают в общей сети `event-manager-network`.

## Наблюдаемость (Observability)

Во все три сервиса (`UserService`, `EventService`, `BookingService`) подключён стек наблюдаемости на базе **OpenTelemetry**:

- **Трейсинг** — автоматическая инструментация входящих HTTP-запросов (`AddAspNetCoreInstrumentation`), исходящих HTTP-запросов (`AddHttpClientInstrumentation`) и запросов к БД через EF Core (`AddEntityFrameworkCoreInstrumentation`). Трейсы экспортируются по протоколу **OTLP** в **Jaeger**.
- **Метрики** — метрики ASP.NET Core (latency, throughput, error rate) и метрики рантайма .NET (GC, thread pool) собираются через `AddAspNetCoreInstrumentation()` и `AddRuntimeInstrumentation()`, экспортируются в формате **Prometheus** через эндпоинт `/metrics` (`AddPrometheusExporter()` + `app.MapPrometheusScrapingEndpoint()`).
- **Логирование** — структурированные логи в формате **JSON** через **Serilog** (`CompactJsonFormatter`), выводятся в консоль контейнера.
- Имя сервиса-ресурса (`service.name`) задаётся через `ConfigureResource(r => r.AddService(...))`: `events-service`, `bookings-service`, `users-service` — соответственно для EventService, BookingService, UserService.

### Компоненты стека

| Инструмент | Назначение | UI / порт |
|---|---|---|
| **Prometheus** | Сбор и хранение метрик (scrape `/metrics` каждого сервиса раз в 15с, конфиг — [`prometheus.yml`](prometheus.yml)) | http://localhost:9090 |
| **Jaeger** | Приём и визуализация распределённых трейсов (OTLP gRPC на 4317) | http://localhost:16686 |
| **Grafana** | Дашборды с метриками latency/throughput/error rate; источник данных Prometheus и дашборд подключены через provisioning ([`grafana/provisioning`](grafana/provisioning)) | http://localhost:3000 (admin / admin) |

### Конфигурация в appsettings.json

```json
{
  "Otlp": {
    "Endpoint": "http://localhost:4317"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

В Docker Compose OTLP endpoint переопределяется переменной окружения `Otlp__Endpoint=http://jaeger:4317`, так как внутри сети Docker Jaeger доступен по имени контейнера.

### Запуск стека наблюдаемости

Стек поднимается вместе с остальной инфраструктурой одной командой из корня репозитория:

```powershell
docker-compose up -d
```

После запуска доступны:

- Prometheus — http://localhost:9090 (раздел **Status → Targets** покажет статус скрейпинга всех трёх сервисов);
- Jaeger UI — http://localhost:16686 (в выпадающем списке **Service** появятся `events-service`, `bookings-service`, `users-service`);
- Grafana — http://localhost:3000 (логин `admin`, пароль `admin`); источник данных Prometheus и дашборд `Event Manager Service - Observability` подключаются автоматически через provisioning, без ручной настройки.

### Проверка (Stage 8)

1. `GET http://localhost:<порт сервиса>/metrics` должен вернуть данные в текстовом формате Prometheus.
2. В Jaeger UI (http://localhost:16686) должны появляться трейсы с корректным именем сервиса и спанами HTTP/SQL-запросов.
3. В Prometheus (**Status → Targets**) все три задания (`events-service`, `bookings-service`, `users-service`) должны быть в состоянии `UP`.
4. Дашборд в Grafana должен отображать данные по latency, throughput, error rate и активным запросам.

JSON дашборда сохранён в репозитории: [`grafana/provisioning/dashboards/event-manager-dashboard.json`](grafana/provisioning/dashboards/event-manager-dashboard.json).
