# EventManager

## Опис проєкту

EventManager — це багатошаровий застосунок для управління подіями. Проєкт реалізує функціональність створення, редагування та управління подіями, реєстрації учасників, роботи з persistence-шаром та автоматизованого тестування.

Поточна версія проєкту включає:

- багатошарову архітектуру;
- dependency inversion;
- persistence-шар із JSON serialization;
- Result pattern;
- Observer pattern;
- fault handling;
- unit та integration testing;
- code coverage;
- CI quality gate;
- асинхронні операції з файловою системою.

# Архітектура проєкту

Проєкт побудований за принципами Clean Architecture та Separation of Concerns.

## Структура шарів

```text
src/
├── EventManager.Domain
├── EventManager.Application
├── EventManager.Infrastructure
├── EventManager.Console