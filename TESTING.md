# TESTING

## Загальна інформація

У межах Lab 36 у проєкті реалізовано повноцінну тестову стратегію, що охоплює:

- unit testing;
- integration testing;
- fault handling testing;
- persistence testing;
- coverage analysis.

Основною метою тестування є забезпечення стабільності критичної бізнес-логіки та захист від регресій.

# Test Pyramid

У проєкті використовується підхід Test Pyramid:

- unit tests перевіряють доменну логіку;
- integration tests перевіряють взаємодію між компонентами;
- end-to-end сценарії частково покривають persistence workflow.

# Unit Tests

Unit tests покривають:

- інваріанти сутностей;
- бізнес-правила;
- domain services;
- validation logic;
- status transitions;
- Result pattern;
- observer notifications.

## Основні негативні сценарії

- duplicate registrations;
- invalid capacity;
- registration after cancellation;
- invalid state transitions;
- empty collections;
- missing entities.


# Integration Tests

Integration tests перевіряють:

- save/load workflow;
- persistence consistency;
- JSON serialization;
- corrupted files;
- missing files;
- sequential operations;
- state restoration після reload.


# Fault Handling

Для fault handling тестуються:

- I/O exceptions;
- corrupted JSON;
- retry strategy;
- invalid data states;
- graceful failure handling.


# Результат тестів

![Screenshot](iteration-3.png)