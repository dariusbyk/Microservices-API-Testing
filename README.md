# Microservices API Testing

Учебный проект по интеграционному тестированию REST API на C# и ASP.NET Core.

## Сервисы

- **UserService** — создание и получение пользователей.
- **OrderService** — создание и получение заказов.
- `OrderService` обращается к `UserService` по HTTP, чтобы получить данные пользователя по его `id`.

## Запуск

### UserService

```bash
cd UserService
dotnet run --urls http://localhost:5001
```

### OrderService

```bash
cd OrderService
dotnet run --urls http://localhost:5002
```

## Основные запросы

```text
POST http://localhost:5001/users
GET  http://localhost:5001/users/{id}

POST http://localhost:5002/orders
GET  http://localhost:5002/orders/{id}
```

Пример создания пользователя:

```json
{
  "name": "Darius",
  "email": "darius@example.com"
}
```

Пример создания заказа:

```json
{
  "userId": 1,
  "product": "Keyboard",
  "quantity": 2
}
```

## Тестирование

Для тестирования используется Postman.

Проверяются:

- успешное создание пользователя;
- успешное создание заказа;
- получение `OrderService` данных пользователя из `UserService`;
- время ответа;
- обработка недоступности `UserService`.

Если `UserService` недоступен, `OrderService` возвращает:

```text
503 Service Unavailable
```

Коллекция Postman находится в папке `postman`.
