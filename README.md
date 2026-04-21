# event-manager-service
Web API сервиса управления мероприятиями.

## Запуск

1. В корне решения выполните: `dotnet build`
2. Для запуска тестов: `cd .\EventService.Tests\` затем `dotnet test`
3. Для локального запуска сервиса: `cd .\EventManagerService\` затем `dotnet run -lp http` или `dotnet run`
   - По умолчанию сервис стартует на порту, указанном в выводе. Откройте `http://localhost:{port}/swagger/index.html`.

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
      "endAt": "2026-04-07T07:51:10.309Z"
    }
  ],
  "total": 0,
  "page": 1,
  "currentPageSize": 0
}
```

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

Пример запроса:

```
POST /events/3fa85f64-5717-4562-b3fc-2c963f66afa6/book
```

Пример ответа (202):

```
{ "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479", "eventId": "3fa85f64-...", "status": "Pending" }
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

## Пример сценария

1. Создать заявку: `POST /events/{eventId}/book` → `202 Accepted`, `Location: /bookings/{bookingId}`
2. Проверить заявку: `GET /bookings/{bookingId}` → `Pending`
3. После фоновой обработки: `GET /bookings/{bookingId}` → `Confirmed`

Пример последовательности:

```
POST /events/3fa85f64-.../book
=> 202 Accepted, Location: /bookings/f47ac10b-...

GET /bookings/f47ac10b-...
=> { "status": "Pending" }

/* после фоновой обработки */
GET /bookings/f47ac10b-...
=> { "status": "Confirmed" }