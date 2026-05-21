# Test Strategy

## Мета тестування

Метою тестування є забезпечення стабільності архітектури та захист критичної бізнес-логіки від регресій і некоректних змін.

Тестова стратегія побудована відповідно до принципів:

- Test Pyramid;
- AAA Pattern;
- Separation of Concerns;
- Defensive Programming.

# Критичні сценарії

До критичних сценаріїв належать:

- створення події;
- реєстрація учасників;
- перевірка capacity;
- duplicate prevention;
- cancellation workflow;
- persistence consistency;
- save/load operations;
- fault handling.

# Найскладніші зони для тестування

## Persistence Layer

Файлова система є нестабільним зовнішнім ресурсом, тому persistence-шар тестується окремо integration tests.

Основні ризики:

- corrupted JSON;
- missing files;
- I/O failures;
- partial writes.

## Observer Notifications

Observer pattern створює side effects, які складніше ізолювати.

Для тестування використовуються mocks.

## Async Operations

Асинхронні save/load операції можуть спричиняти race conditions та непередбачувану поведінку при помилках I/O.

# Використання mocks

Mocks використовуються для:

- repositories;
- observers;
- logging;
- external dependencies.

Це дозволяє ізолювати бізнес-логіку від зовнішніх компонентів.

# Використання реальної інтеграції

Реальна інтеграція використовується для:

- JSON serialization;
- file system operations;
- persistence workflow;
- state restoration.

# Основні негативні сценарії

У межах Lab 36 окрему увагу приділено негативним сценаріям:

- duplicate participant registration;
- invalid capacity values;
- registration after cancellation;
- corrupted persistence file;
- missing persistence file;
- invalid state transitions;
- capacity overflow;
- I/O exceptions.

# Test Pyramid

## Unit Tests

Основний рівень тестування.

Покривають:

- domain rules;
- invariants;
- validation;
- services.

## Integration Tests

Перевіряють взаємодію між:

- repositories;
- data stores;
- persistence layer;
- serialization.

# AAA Pattern

Кожен тест побудований за схемою:

1. Arrange
2. Act
3. Assert

Це забезпечує:

- читабельність;
- підтримуваність;
- зрозумілу структуру тестів.