# Система управління подіями (EventManager)

## Анотація (Project Vision)
**EventManager** — це консольна інформаційна система, призначена для автоматизації процесів створення подій, реєстрації учасників та управління ресурсами (локаціями, місткістю). Проєкт розробляється як підсумковий міні-проєкт (Capstone Project) у рамках курсу об'єктно-орієнтованого програмування. 

Мета проєкту — демонстрація еволюційного підходу до розробки програмного забезпечення з використанням принципів чистої архітектури (Clean Architecture), предметно-орієнтованого проєктування (Domain-Driven Design, DDD) та патернів проєктування.

### Проблема та рішення
Ручне або неструктуроване управління реєстраціями часто призводить до дублювання учасників, перевищення ліміту місць на локації та втрати консистентності даних. Система EventManager вирішує цю проблему шляхом інкапсуляції строгих бізнес-правил (інваріантів) глибоко в доменному шарі, що унеможливлює переведення системи в некоректний стан.

## Архітектура рішення
Проєкт побудований за принципами багатошарової архітектури з жорстким дотриманням **Dependency Inversion Principle (DIP)**.

* **1. Domain Layer (`EventManager.Domain`)**: Ядро системи. Містить бізнес-сутності (`Event`, `Venue`, `Organizer`, `Participant`, `Registration`), перелічення (`EventStatus`, `EventCategory`) та інтерфейси репозиторіїв. Тут реалізовано захист інваріантів (валідація місткості, заборона реєстрації на закриті події). Шар абсолютно незалежний від інших компонентів.
* **2. Application Layer (`EventManager.Application`)**: Шар Use Cases. Відповідає за оркестрацію доменних об'єктів та інфраструктури. Використовує патерн `Result` для безпечного повернення результатів операцій без зловживання винятками (Exceptions) для очікуваних бізнес-помилок.
* **3. Infrastructure Layer (`EventManager.Infrastructure`)**: Реалізація механізмів збереження даних. На поточній ітерації використовується патерн *In-Memory Repository* для ізоляції бізнес-логіки від деталей персистенції.
* **4. Presentation Layer (`EventManager.Console`)**: Клієнтський інтерфейс (CLI). Логіка маршрутизації команд реалізована за допомогою словника делегатів (`Dictionary<string, Action>`), що задовольняє принцип Open-Closed (OCP).

## Доменна модель (UML Class Diagram)

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
    }

    class Event {
        -List~Registration~ _registrations
        +string Title
        +string Description
        +DateTime Date
        +int Capacity
        +Venue Venue
        +Organizer Organizer
        +EventStatus Status
        +EventCategory Category
        +IReadOnlyCollection~Registration~ Registrations
        +this[Guid participantId] Registration?
        +HasAvailablePlaces() bool
        +AddRegistration(Registration registration)
    }

    class Venue {
        +string Name
        +string Address
        +int MaxCapacity
    }

    class Organizer {
        +string FullName
        +string Email
    }

    class Participant {
        +string FullName
        +string Email
    }

    class Registration {
        +Guid EventId
        +Guid ParticipantId
        +DateTime RegistrationDate
    }

    BaseEntity <|-- Event
    BaseEntity <|-- Venue
    BaseEntity <|-- Organizer
    BaseEntity <|-- Participant
    BaseEntity <|-- Registration

    Event "1" *-- "1" Venue : Відбувається в
    Event "1" *-- "1" Organizer : Організується
    Event "1" *-- "0..*" Registration : Містить