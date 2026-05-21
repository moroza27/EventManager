# Iteration 3

## Загальна інформація

У межах третьої ітерації проєкт було перетворено з функціонального прототипу на більш стабільне та технічно захищене рішення.

Основний акцент зроблено на:

- тестованості;
- quality control;
- fault handling;
- стабільності persistence-шару;
- захисті від регресій.

# Реалізовані покращення

## Архітектурні покращення

Було виконано цільовий рефакторинг для покращення тестованості:

- усунено hidden dependencies;
- зменшено tight coupling;
- виділено testing seams;
- покращено dependency inversion;
- відокремлено side effects від бізнес-логіки.

---

## Fault Handling

Реалізовано:

- Result pattern;
- DomainException;
- retry strategy;
- graceful failure handling;
- defensive programming.

## Тестування

Було додано:

- 20+ unit tests;
- 8+ integration tests;
- Theory tests;
- fault handling tests;
- persistence tests.

# Усунені code smells

У процесі рефакторингу усунуто:

- hidden dependencies;
- mixed responsibilities;
- direct file access inside services;
- weak error handling;
- partially duplicated validation logic.

# Coverage

Для оцінки тестового покриття використовується Coverlet.

## Поточні показники

- Line Coverage: 80%+
- Branch Coverage: 70%+

Coverage використовується лише як допоміжна метрика та не розглядається як єдиний показник якості.

# CI / Quality Gate

У GitHub Actions додано:

- автоматичну збірку;
- запуск тестів;
- coverage generation;
- pipeline validation.

Pipeline завершується помилкою у випадку failing tests або build errors.

# Поточні ризики

Попри значне покращення стабільності, перед Lab 37 залишаються такі ризики:

- відсутність concurrent write protection;
- відсутність database backend;
- synchronous observer notifications;
- часткова залежність console UI від workflow logic;
- відсутність advanced logging.

# Плани для Lab 37

У наступній ітерації планується:

- завершення Blazor UI;
- advanced logging;
- validation pipeline;
- caching;
- external storage adapters;
- performance optimization;
- concurrent operations handling.

# Висновок

У межах Lab 36 проєкт було суттєво покращено з точки зору:

- стабільності;
- тестованості;
- fault tolerance;
- якості архітектури;
- підтримуваності коду.

Реалізована тестова стратегія дозволяє значно зменшити ризик регресій та підготувати систему до подальшого розвитку у Lab 37.