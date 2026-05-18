# Діаграма класів EventManager

## Основні сутності

- BaseEntity
- Event
- Participant
- Registration
- Venue
- Organizer

## Перерахування (Enums)

- EventStatus
- EventCategory

## Інтерфейси

- IEventRepository
- IRegistrationRepository


## Сервіси

- EventService

## Зв’язки між сутностями

- Event пов’язаний з Venue
- Event пов’язаний з Organizer
- Event містить Registration
- Registration пов’язує Event та Participant

## Межі шарів

Console -> Application -> Domain -> Infrastructure

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