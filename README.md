# event-manager-service
Web API сервиса управления мероприятиями с поддержкой управления местами и синхронизированным бронированием.

## Структура проекта

Решение организовано по классической многослойной (слоевой / onion) архитектуре. Ниже перечислены проекты/слои и их назначение:

- EventManagerService (хост / API)
  - Веб-приложение / точка входа (ASP.NET Core Web API).
  - Содержит контроллеры, конфигурацию DI, запуск приложения, Swagger и настройки хоста.
  - Отвечает за перевод входящих HTTP-запросов в вызовы прикладного слоя (Application).

- EventManagerService.Application (Application / Use Cases)
  - Реализация сценариев приложения (use-cases) — сервисы прикладного уровня, координаторы операций.
  - Содержит интерфейсы для репозиториев и других зависимостей, DTO и реализацию логики, не зависящую от инфраструктуры.
  - Здесь реализуется валидация входных данных, транзакционная координация и оркестрация доменных операций.

- EventManagerService.Domain (Domain / Model)
  - Доменные сущности, value-объекты, доменные исключения и чистая бизнес-логика.
  - Реализация правил предметной области (например, управление availableSeats, TryReserveSeats/ReleaseSeats и т.д.).
  - Минимальные внешние зависимости — только на базовые библиотеки .NET.

- EventManagerService.Infrastructure (Infrastructure / Persistence)
  - Техническая реализация интерфейсов: репозитории, реализация DbContext (EF Core + Npgsql), миграции, интеграция с внешними системами.
  - Тут находятся адаптеры к базе данных, кэшам, очередям и т.п.
  - Infrastructure зависит от Domain и Application, но не наоборот.

- EventManagerService.Shared (Shared / Common)
  - Общие вспомогательные классы, константы, расширения, общие DTO/модели и вспомогательные утилиты, используемые в нескольких проектах.

- EventService.Tests (Unit Tests)
  - Модульные тесты для домена и прикладной логики, обычно с использованием InMemory-провайдера и моков.

- EventService.IntegrationTests (Integration Tests)
  - Интеграционные тесты, проверяющие поведение с реальной БД (PostgreSQL через Testcontainers/Docker) и тестируемыми конвейерами.

Направление зависимостей: API -> Application -> Domain <- Infrastructure (Infrastructure реализует контракты, объявленные в Application/Domain). Это обеспечивает чёткое разделение ответственности и упрощает тестирование.


## Требования

- .NET 10 SDK
- PostgreSQL (локально или в облаке) — требуется для запуска приложения в режиме работы с реальной базой данных.

Если вы не хотите устанавливать PostgreSQL локально для тестов, тесты используют InMemory-провайдер EF Core (см. раздел «Тесты»).

## Запуск

1. В корне решения выполните: `dotnet build`
2. Для запуска тестов: `cd .\EventService.Tests\` затем `dotnet test`
3. Для локального запуска сервиса: `cd .\EventManagerService\` затем `dotnet run -lp http` или `dotnet run`
   - По умолчанию сервис стартует на порту, указанном в выводе. Откройте `http://localhost:{port}/swagger/index.html`.

## Настройка строки подключения

1. В файле appsettings.json укажите строку подключения к PostgreSQL в формате:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=event_manager;Username=postgres;Password=your_password"
}
```

Примечание: проект настроен на использование EF Core с поставщиком Npgsql для PostgreSQL.

## Схема базы данных и миграции

Схема базы данных управляется миграциями EF Core. Для разработки и продакшена следует применять миграции вместо EnsureCreated(), чтобы сохранять версионность схемы и корректно управлять изменениями структуры данных.

Команды для создания и применения миграций (в каталоге проекта, где находится DbContext, например EventManagerService):

```powershell
# Создать новую миграцию (замените InitialCreate на осмысленное имя миграции)
dotnet ef migrations add InitialCreate -p ..\EventManagerService -s ..\EventManagerService

# Применить все миграции к базе данных
dotnet ef database update -p ..\EventManagerService -s ..\EventManagerService
```

Примечание: при использовании разных проектов для миграций (-p) и стартап-проекта (-s) укажите корректные относительные пути.

## Описание API

### GET /events

Query-параметры:

- `title` (string) — фильтр по части названия, регистронезависимый
- `from` (DateTime) — начальная дата; включает мероприятия, начинающиеся не позже этой даты
- `to` (DateTime) — конечная дата; включает мероприятия, завершающиеся не позже этой даты
- `page` (int) — номер страницы, от 1, по умолчанию 1
- `pageSize` (int) — размер страницы, от 10 до 100, по умолчанию 100

Формат ответа (пример):

```
{
  "events": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "string",
      "description": "string",
      "startAt": "2026-04-07T07:51:10.309Z",
      "endAt": "2026-04-07T07:51:10.309Z",
      "totalSeats": 100,
      "availableSeats": 42
    }
  ],
  "total": 0,
  "page": 1,
  "currentPageSize": 0
}
```

### Модель Event — управление местами

Поля:

- `id` (Guid)
- `title` (string) — название мероприятия
- `description` (string?) — описание
- `startAt` (DateTime) — начало
- `endAt` (DateTime) — конец
- **`totalSeats` (int)** — **общее количество мест на мероприятие (обязательное, > 0)**
- **`availableSeats` (int)** — **текущее количество свободных мест (инициализируется значением totalSeats)**

Примечания по полям и валидации:

- totalSeats — задаёт максимально возможное количество мест для мероприятия и должно быть положительным целым числом (> 0).
- availableSeats — отражает текущее число свободных мест и при создании события инициируется значением totalSeats. Это поле уменьшается при резервировании и увеличивается при освобождении мест.
- Модель предоставляет методы управления местами:
  - `TryReserveSeats(int count = 1): bool` — попытка зарезервировать `count` мест. Возвращает `true` и уменьшает `availableSeats` при успешной резервции, `false` при недостатке мест.
  - `ReleaseSeats(int count = 1): bool` — освобождает `count` мест. Возвращает `true` при успешном увеличении `availableSeats`, `false` если попытка вернуть больше мест, чем вместимость (totalSeats).

Важно: операции над availableSeats в модели — простые изменения integer. Для корректной работы в многопоточной среде используется синхронизация на уровне сервиса бронирований (см. раздел "Синхронизация").

### Формат ошибок

Используется стандартный Problem Details:

- `type` — URI типа проблемы
- `title` — краткое описание
- `status` — HTTP-статус
- `instance` — URI конкретного возникновения

### POST /events/{id}/book

Создаёт заявку (booking) на участие в мероприятии `id`.

- Успех: `202 Accepted`, в теле — `BookingDTO`, заголовок `Location: /bookings/{bookingId}`
- Ошибка: `404 Not Found`, если мероприятие не найдено
- Ошибка: `409 Conflict`, если на момент попытки бронирования свободных мест нет (в ответе — Problem Details с пояснением)

Пример запроса:

```
POST /events/3fa85f64-5717-4562-b3fc-2c963f66afa6/book
```

Пример ответа (202):

```
{ "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479", "eventId": "3fa85f64-...", "status": "Pending" }
```

Пример ошибки при отсутствии мест (409):

```
HTTP/1.1 409 Conflict
Content-Type: application/json

{
  "status": 409,
  "detail": "No available seats for this event"
}
```

### GET /bookings/{id}

Возвращает заявку по `id`.

- Успех: `200 OK`, в теле — `BookingDTO`
- Ошибка: `404 Not Found`, если заявка не найдена

Пример запроса:

```
GET /bookings/f47ac10b-58cc-4372-a567-0e02b2c3d479
```

Пример ответа (200):

```
{ "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479", "eventId": "3fa85f64-...", "status": "Confirmed" }
```

## Модель Booking

Поля:

- `id` (Guid)
- `eventId` (Guid)
- `status` (BookingStatus)
- `createdAt` (DateTime, UTC)
- `processedAt` (DateTime?, UTC)

Статусы: `Pending`, `Confirmed`, `Rejected`.

Логика: переход статуса возможен только из `Pending`; при подтверждении/отклонении заполняется `processedAt`.

## Фоновая обработка

Фоновая служба периодически (по умолчанию каждые 60 секунд) выбирает заявки в состоянии `Pending` и запускает их обработку.

- Для каждой заявки создаётся задача, ожидающая фиксированную задержку (в текущей реализации ~10 секунд), затем вызывается `ConfirmBookingAsync`.
- При успешной обработке статус меняется на `Confirmed`, заполняется `processedAt`.
- При ошибке заявка остаётся `Pending` и может быть обработана позже.

Примечание: текущая реализация эмулирует автоматическую обработку. Для продакшена рекомендуется очередь задач, retry и аудит.

## Интеграционные тесты

В проекте предусмотрены интеграционные тесты, которые проверяют поведение приложения с реальной СУБД (PostgreSQL). Для запуска интеграционных тестов требуется Docker — тесты запускают контейнер PostgreSQL или подключаются к указанной в окружении/конфигурации инстанции базы данных.

Для запуска интеграционных тестов требуется запущенный Docker. Testcontainers используется внутри тестов и сам поднимает необходимые контейнеры PostgreSQL при выполнении тестов, поэтому ручной запуск контейнера через docker run не обязателен.

Настройте переменные окружения или appsettings для тестового подключения, например:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=event_manager;Username=postgres;Password=postgres"
}
```

Запуск интеграционных тестов:

1. Убедитесь, что контейнер PostgreSQL запущен и доступен
2. Выполните `dotnet test` в каталоге с тестами, или используйте фильтры для запуска только интеграционных тестов

Примечание: в локальной разработке для быстрого выполнения модульных тестов используется EF Core InMemory-провайдер; он не моделирует поведение транзакций и конкуренции, поэтому тесты на конкурентность могут быть помечены как пропущенные при запуске с InMemory.

## Синхронизация

Для предотвращения гонок и овербукинга используется простая блокировка на уровне сервиса бронирования:

- В BookingService применяется примитив `lock` (внутри класса — `private readonly object _bookingLock = new();`). `lock` в .NET использует Monitor под капотом и служит для взаимного исключения доступа к критической секции.
- Критическая секция охватывает проверку существования события, попытку резервирования мест (вызов `TryReserveSeats`) и добавление записи о бронировании в память.

Зачем это нужно:

- Операции чтения/записи поля `AvailableSeats` не атомарны в контексте логики (проверка + уменьшение). Без синхронизации два или более параллельных запроса могли бы увидеть одну и ту же свободную позицию и оба успешно уменьшить availableSeats — получится овербукинг.
- Блокировка гарантирует, что проверка наличия мест и их резервирование выполняются как одна атомарная операция с точки зрения сервиса, что предотвращает рассинхронизацию состояния и дубликаты бронирований.

Примечание: сам метод модели `TryReserveSeats` реализован простым изменением integer и не использует дополнительных примитивов синхронизации — ответственность за корректную последовательность вызовов возлагается на сервис.

## Пример сценария с овербукингом (и как система ему препятствует)

Сценарий (без синхронизации, иллюстрация проблемы):

1. AvailableSeats = 1
2. Клиент A делает POST /events/{id}/book — сервис проверяет availableSeats (видит 1)
3. Клиент B почти одновременно делает POST /events/{id}/book — сервис также проверяет availableSeats (видит 1)
4. Оба потока уменьшают availableSeats → результат AvailableSeats = -1 (перепродажа)

Как это работает в текущей реализации:

1. AvailableSeats = 1
2. Клиент A входит в критическую секцию `lock(_bookingLock)` и выполняет проверку + резервацию → доступен 1, резервирует, AvailableSeats становится 0, добавляется запись booking
3. Пока A в критической секции, B блокируется на `lock` и не может выполнить проверку
4. После выхода A из блокировки B получает контроль, проверяет availableSeats (0) и получает ответ об отсутствии мест — в коде это приводит к выбрасыванию NoAvailableSeatsException, которая конвертируется в 409 Conflict

Пример последовательности запросов и ответов:

```
# Клиент A
POST /events/{id}/book
=> 202 Accepted, Location: /bookings/{idA}

# Клиент B (почти одновременно)
POST /events/{id}/book
=> 409 Conflict
{
  "status": 409,
  "detail": "No available seats for this event"
}
```

Таким образом, система предотвращает овербукинг за счёт простой и эффективной блокировки в сервисе бронирований.

## Пример сценария использования

1. Создать заявку: `POST /events/{eventId}/book` → `202 Accepted`, `Location: /bookings/{bookingId}`
2. Проверить заявку: `GET /bookings/{bookingId}` → `Pending`
3. После фоновой обработки: `GET /bookings/{bookingId}` → `Confirmed`

## Аутентификация и авторизация

Сервис использует JWT (JSON Web Token) для защиты API. Все защищённые эндпоинты требуют действительного токена в заголовке `Authorization: Bearer {token}`.

### Ролевая модель и разграничение прав

В системе реализованы следующие роли:

#### 1. **User (пользователь)** — роль по умолчанию
- Имеет доступ к просмотру всех мероприятий: `GET /events`, `GET /events/{id}`
- Может создавать бронирования: `POST /events/{id}/book`
- Может просматривать свои бронирования: `GET /bookings/{id}`
- Может отменять только свои собственные бронирования: `DELETE /bookings/{id}`
- При попытке отменить бронирование другого пользователя получит ошибку `403 Forbidden`

#### 2. **Admin (администратор)**
- Имеет все права пользователя (просмотр, бронирование)
- Может создавать новые мероприятия: `POST /events` требует роль `admin`
- Может обновлять существующие мероприятия: `PUT /events/{id}` требует роль `admin`
- Может удалять мероприятия: `DELETE /events/{id}` требует роль `admin`
- Может отменять бронирования других пользователей (правило бизнес-логики)

### Таблица разграничения доступа

| Эндпоинт | GET | POST | PUT | DELETE | User | Admin | ТребуетAuth |
|---|---|---|---|---|---|---|---|
| `/auth/register` | - | ✓ | - | - | ✓ | ✓ | Нет |
| `/auth/login` | - | ✓ | - | - | ✓ | ✓ | Нет |
| `/events` | ✓ | ✓ | - | - | ✓ | ✓ | Нет (GET), Да (POST) |
| `/events/{id}` | ✓ | - | ✓ | ✓ | ✓ | ✓ | Нет (GET), Да (PUT/DELETE) |
| `/events/{id}/book` | - | ✓ | - | - | ✓ | ✓ | Да |
| `/bookings/{id}` | ✓ | - | - | ✓ | ✓ | ✓ | Да |

### Получение JWT-токена через Swagger

#### 1. Регистрация нового пользователя

- Откройте Swagger UI: `http://localhost:{port}/swagger/index.html`
- Перейдите в секцию **Authentication**
- Кликните на эндпоинт **POST /auth/register** и нажмите **Try it out**
- Заполните тело запроса:

```json
{
  "login": "john_doe",
  "password": "securePassword123",
  "role": "user"
}
```

Допустимые значения `role`:
- `"user"` — обычный пользователь (по умолчанию)
- `"admin"` — администратор с полными правами

Пример ответа (200 OK):

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "login": "john_doe",
  "role": "user",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJyb2xlIjoidXNlciIsImV4cCI6MTcwNDcxMTEwMCwiaXNzIjoiRXZlbnRNYW5hZ2VyU2VydmljZSIsImF1ZCI6IkV2ZW50TWFuYWdlclNlcnZpY2VDbGllbnRzIn0.SIGNATURE"
}
```

#### 2. Вход существующего пользователя

- В Swagger перейдите на **POST /auth/login**
- Нажмите **Try it out**
- Заполните тело запроса:

```json
{
  "login": "john_doe",
  "password": "securePassword123"
}
```

Пример ответа (200 OK) — будет возвращён новый JWT-токен.

#### 3. Использование токена в запросах

После получения токена скопируйте значение поля `token` из ответа.

В Swagger:

1. Нажмите кнопку **Authorize** в правом верхнем углу
2. В диалоговом окне введите в поле значение:

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

3. Нажмите **Authorize** → все последующие запросы будут отправляться с этим токеном в заголовке `Authorization`

Альтернативно, при использовании `curl` или другого HTTP-клиента:

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" http://localhost:{port}/bookings/{id}
```

### Конфигурация JWT в приложении

JWT-параметры хранятся в `appsettings.json` и управляются через класс `JwtSettings`:

```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-at-least-32-characters-long-for-security",
    "Issuer": "EventManagerService",
    "Audience": "EventManagerServiceClients",
    "ExpirationMinutes": 60
  }
}
```

#### Описание параметров

- **Secret** — секретный ключ для подписи и проверки токенов
  - Используется для подписания токена на сервере и проверки его подлинности
  - Должен быть достаточно длинным (минимум 32 символа для HS256)
  - **ВАЖНО: В production среде используйте криптографически стойкое значение и не коммитьте его в репозиторий!**

- **Issuer** — издатель токена
  - Должен совпадать с `ValidIssuer` при валидации токена
  - По умолчанию: `EventManagerService`

- **Audience** — аудитория токена
  - Должна совпадать с `ValidAudience` при валидации
  - По умолчанию: `EventManagerServiceClients`

- **ExpirationMinutes** — время жизни токена в минутах
  - Определяет, как долго токен остаётся действительным после выпуска
  - По умолчанию: 60 минут
  - Рекомендуется для production: 15–30 минут для повышения безопасности

#### Рекомендации по безопасности для production

1. **Использование переменных окружения**

   Вместо хранения секрета в `appsettings.json`, используйте переменные окружения:

   ```csharp
   var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
   var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "EventManagerService";
   var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "EventManagerServiceClients";
   ```

2. **Azure Key Vault** (рекомендуется для production)

   Для Azure-hosted приложений используйте Azure Key Vault:

   ```csharp
   var keyVaultUrl = new Uri("https://your-keyvault.vault.azure.net/");
   var credential = new DefaultAzureCredential();
   var client = new SecretClient(keyVaultUrl, credential);
   var secret = client.GetSecret("JwtSecret").Value.Value;
   ```

3. **AWS Secrets Manager** или **HashiCorp Vault** для других облачных платформ

4. **Требования к секрету**
   - Минимальная длина: 32 символа
   - Используйте случайные символы, цифры, специальные символы
   - Пример сильного секрета:
     ```
     7&mK9#xL2$qR5*wP1!vN4@bD8%fG3^hJ6
     ```

5. **Принцип наименьших привилегий**
   - Предоставляйте только необходимые роли пользователям
   - Регулярно проверяйте и отзывайте неиспользуемые токены
   - Используйте short-lived токены (< 30 минут) в production

6. **Слушайте критичные события**
   - Логируйте все попытки несанкционированного доступа
   - Мониторьте паттерны использования токенов
   - Настройте alerts на повторяющиеся ошибки аутентификации

#### Пример конфигурации для разработки

`appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "Secret": "development-super-secret-key-at-least-32-characters-for-testing",
    "Issuer": "EventManagerService",
    "Audience": "EventManagerServiceClients",
    "ExpirationMinutes": 120
  }
}
```

#### Пример конфигурации для production (с переменными окружения)

```powershell
# Установка переменных окружения перед запуском приложения
$env:JwtSettings__Secret = "your-strong-production-secret-key-here-with-32-plus-chars"
$env:JwtSettings__Issuer = "EventManagerService"
$env:JwtSettings__Audience = "EventManagerServiceClients"
$env:JwtSettings__ExpirationMinutes = "15"

dotnet run
```

Или через системные переменные окружения (Windows):

```powershell
setx JwtSettings__Secret "your-strong-production-secret-key-here-with-32-plus-chars"
setx JwtSettings__Issuer "EventManagerService"
setx JwtSettings__Audience "EventManagerServiceClients"
setx JwtSettings__ExpirationMinutes "15"
```

### Обработка токенов в API

При запросе к защищённому эндпоинту:

1. Клиент отправляет токен в заголовке: `Authorization: Bearer {token}`
2. Middleware аутентификации проверяет:
   - Подпись токена (используя `Secret`)
   - Издателя (Issuer)
   - Аудиторию (Audience)
   - Срок действия (ExpirationMinutes)
3. Если валидация прошла успешно, извлекаются Claims (userId, role и т.д.)
4. Claims становятся доступны в контроллерах через `User.FindFirst(ClaimTypes.NameIdentifier)` и т.д.
5. Авторизация проверяет права доступа (например, `[Authorize(Roles = "admin")]`)

### Пример использования Claims в коде

```csharp
[HttpPost("events/{id:guid}/book")]
[Authorize]
public async Task<ActionResult<BookingDTO>> CreateBooking(Guid id)
{
    // Получаем userId из JWT-токена
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized(new { message = "Invalid or missing user ID in token" });
    }

    // Получаем роль пользователя
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "user";

    var booking = await _bookingService.CreateBookingAsync(id, userId);
    return AcceptedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
}
```