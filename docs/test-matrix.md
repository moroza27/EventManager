# Test Matrix

## Призначення документа

Цей документ описує відповідність між функціональними сценаріями системи, ризиками та тестами, які їх покривають.

Матриця тестування використовується для:

- контролю покриття критичних сценаріїв;
- перевірки наявності тестів для ключових use cases;
- відстеження негативних сценаріїв;
- оцінки стабільності системи перед релізом.


# Позначення

| Позначення | Значення |
|---|---|
| UT | Unit Test |
| IT | Integration Test |
| NEG | Негативний сценарій |
| TH | Theory Test |


# Матриця тестування

| ID | Use Case | Ризик | Тип тесту | Назва тесту | Статус |
|---|---|---|---|---|---|
| TM-01 | Створення події | Некоректний стан сутності | UT | Event_CannotBeCreated_WithPastDate | ✅ |
| TM-02 | Створення події | Некоректний capacity | TH | InvalidCapacity_ShouldThrow | ✅ |
| TM-03 | Реєстрація учасника | Duplicate registration | UT / NEG | Event_CannotRegister_DuplicateParticipant | ✅ |
| TM-04 | Реєстрація учасника | Перевищення capacity | UT / NEG | Event_CannotExceedCapacity | ✅ |
| TM-05 | Реєстрація учасника | Реєстрація після скасування | UT / NEG | Event_CannotRegister_WhenCancelled | ✅ |
| TM-06 | Скасування події | Некоректна зміна статусу | UT | Event_CancelEvent_ChangesStatus | ✅ |
| TM-07 | Observer notifications | Відсутність сповіщення | UT | CancelEvent_ShouldNotifyObservers | ✅ |
| TM-08 | Збереження даних | Втрата даних | IT | SaveAndReload_PreservesData | ✅ |
| TM-09 | Persistence | Пошкоджений JSON | IT / NEG | CorruptedJson_ReturnsFailure | ✅ |
| TM-10 | Persistence | Відсутній файл | IT / NEG | MissingFile_ReturnsEmptyCollection | ✅ |
| TM-11 | Persistence | Втрата даних при кількох save | IT | SequentialSaveOperations_DoNotLoseData | ✅ |
| TM-12 | Persistence | Некоректне відновлення стану | IT | MultipleReloads_ReturnSameData | ✅ |
| TM-13 | EventService | Event not found | UT / NEG | RegisterParticipant_ReturnsFailure_WhenEventMissing | ✅ |
| TM-14 | EventService | Repository failure | UT / NEG | CreateEvent_ReturnsFailure_WhenRepositoryFails | ✅ |
| TM-15 | Fault Handling | I/O exception | IT / NEG | SaveAsync_ReturnsFailure_OnIOException | ✅ |
| TM-16 | Analytics | Групування за категоріями | UT | EventService_GetEventCountByCategory_ReturnsCounts | ✅ |



# Аналіз покриття

## Unit Tests

Unit tests покривають:

- інваріанти;
- validation logic;
- domain rules;
- status transitions;
- fault handling;
- observer behavior.

Наразі у проєкті реалізовано понад 30 автоматизованих тестів, що охоплюють доменну і бізнес-логіку, persistence та аналітичні запити.


## Integration Tests

Integration tests покривають:

- persistence workflow;
- save/load operations;
- JSON serialization;
- fault handling;
- file system interactions.

# Непокриті ризики

Попри значний рівень покриття, залишаються ризики:

- concurrent write operations;
- race conditions;
- performance degradation при великих об’ємах даних;
- integration з зовнішніми сервісами;
- UI-specific edge cases.


# Висновок

Поточна test matrix підтверджує, що критичні бізнес-сценарії та негативні кейси покриті unit та integration tests.

Основний акцент зроблено на:

- fault tolerance;
- persistence consistency;
- стабільності доменної логіки;
- захисті від регресій.