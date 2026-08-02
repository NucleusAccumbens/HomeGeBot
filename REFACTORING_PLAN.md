# План рефакторинга и отчёт код-ревью

Дата ревью: 2026-08-02  
Дата обновления: 2026-08-02  
Объект: `Property In Tbilisi bot` (.NET 10.0, Clean Architecture + CQRS + Telegram Bot)  
Критерии: принципы ООП и SOLID.  
Состояние сборки: `dotnet build` — успешно; `dotnet test` — 42 теста пройдены.

---

## 1. Сводная оценка (актуализированная)

| Принцип | Соблюдение | Комментарий |
|---------|-----------|-------------|
| **Инкапсуляция** | ⚠️ Частично | `Admin` частично инкапсулирован (`Clients`, `IsActive`). Остальные сущности Domain всё ещё имеют публичные `set` без инвариантов. |
| **Наследование** | ✅ Хорошо | Базовые классы используются корректно. |
| **Полиморфизм** | ✅ Хорошо | `IUpdateHandler`-стратегии, MediatR, pipeline behaviors, `IUpdateHandler` — хороший полиморфизм. |
| **Абстракция** | ✅ Существенно улучшено | `ITelegramBotClientProvider`, `IUpdateHandler`, `ITextCommandRouter`, `IRentalApplicationForwardProcessor`, `ITmaLabelProvider` — портов стало больше. `TelegramBot` теперь реализует интерфейс. |
| **SRP** | ✅ Частично улучшено | `CommandAnalyzer` и `AppTextCommand` декомпозированы. `GetAdminDashboardHandler`, `BaseMessage`, `Index.cshtml.cs` всё ещё перегружены. |
| **OCP** | ⚠️ Частично | Pipeline behaviors, `IUpdateHandler` расширяемы; `ApplicationMessages`, `Message.BodyEn/BodyKa`, `Other` в enum'ах требуют правок кода. |
| **LSP** | ✅ Хорошо | Нарушений не обнаружено. |
| **ISP** | ✅ Хорошо | Интерфейсы узкие. |
| **DIP** | ✅ Существенно улучшено | Web и Bot теперь зависят от абстракций (`ITelegramBotClientProvider`, `IUpdateHandler`, `IRentalApplicationForwardProcessor`). Остаётся `global using Microsoft.EntityFrameworkCore` в Application. |

**Общий вердикт**: основные критичные риски (безопасность Web, прямой доступ `TmaController` к БД, монолитные `CommandAnalyzer`/`AppTextCommand`) устранены. Проект собирается, тесты проходят. Осталась техдолг вокруг полной инкапсуляции Domain, `IEntityTypeConfiguration`, `IDateTime` и покрытия тестами Web/Bot.

---

## 2. История выполнения

| Фаза | Коммит | Статус |
|------|--------|--------|
| 0. Документация | `d52a7b9` | ✅ Обновлены README, REFACTORING_PLAN, ARCHITECTURE_AUDIT |
| 1. Безопасность и Clean Architecture | `2c5ed7d` | ✅ TMA use cases, webhook SecretToken, проксирование фото, XSS fix |
| 2. Мёртвый код | `33c037c` | ✅ Убраны `ErrorMessage`/`Failure` из Result DTO, удалены пустые `UnitTest1.cs` |
| 3. Инкапсуляция Domain | `95a1e44` | ✅ `Admin` + `ChatId.FromLong/ToLong` + `ChatIdValidationExtensions` |
| 4. Декомпозиция SRP | `17f35ff`, `4b49a04` | ✅ `ITelegramBotClientProvider`, полная декомпозиция `CommandAnalyzer`/`AppTextCommand` |
| 5. Локализация и OCP | `ebb58f9` | ✅ `ITmaLabelProvider` для TMA submit-application labels |
| 6. Производительность | `cd05a7c` | ✅ N+1 в `GetUserApplicationsHandler`, оптимизация username-lookups |
| 7. Тестирование | `a1ae0fd` | ✅ `TestDbContext` в Common, тесты `SetUserLanguage`/`GetManagerContact` |

---

## 3. Оставшиеся нарушения и план дальнейшего рефакторинга

### 3.1 Domain (средний приоритет)

- **D1**: `Client`, `Flat`, `Message`, `TlgUser` всё ещё — data bags. Нужны конструкторы, приватные сеттеры, доменные методы.
- **D3**: связь `Admin–Client` односторонняя; `Client.AdminChatId` дублирует `Admin.Clients`.
- **D4**: `TlgUser.IsAdmin` дублирует таблицу `Admins`.
- **D5**: `Message.BodyEn/BodyKa` не расширяемы (OCP).
- **D7**: `Other` в `Country`/`Term` — UI-концерн в доменном enum.

### 3.2 Application (низкий–средний)

- **A1**: `global using Microsoft.EntityFrameworkCore` остаётся. Вынести EF-зависимость из `Application/GlobalUsing.cs` в Infrastructure.
- **A2**: `IDateTime` не используется в handler'ах напрямую. Перейти на `_dateTime.Now`.
- **A4**: `GetAdminDashboardHandler` всё ещё слишком большой; разбить на отдельные query.
- **A8**: нет validators для query (`GetMessageBody`, `GetUserLanguage` и т.п.).
- **A11**: часть query возвращает примитивы (`bool`/`string`) вместо `Result<T>`.

### 3.3 Web (средний)

- **W7**: `Index.cshtml.cs` с 5 POST-обработчиками и ручным маппингом.
- **W8**: `GetCurrentAdminChatId` возвращает `0` при отсутствии claim.
- **W9**: проверить и заменить оставшиеся `@Html.Raw` с пользовательскими данными.

### 3.4 Infrastructure (низкий)

- **I1**: `ConnectionStringFactory` статический, без интерфейса.
- **I3**: `HomeGeBotDbContext.OnConfiguring` использует `null!`.
- **I4**: нет `IEntityTypeConfiguration<>`.
- **I5**: пустая миграция `UpdateChatIdToValueObject`.
- **I6**: seeding сообщений в миграции.

### 3.5 Tests (высокий)

- Покрытие остаётся низким. Нужны `Domain.Tests`, `Infrastructure.Tests`, `Web.Tests`, тесты на `CommandAnalyzer`/handlers, валидацию, pipeline behaviors.

---

## 4. Рекомендуемые следующие шаги

### Итерация 1 — Domain-инкапсуляция (средний приоритет)

1. Добавить конструкторы и доменные методы `Client`, `TlgUser`, `Flat`, `Message`.
2. Защитить оставшиеся публичные сеттеры (`init`/`private set`).
3. Убрать дублирование `TlgUser.IsAdmin` / `Admins`.
4. Унифицировать связь `Admin–Client`.

### Итерация 2 — EF и конфигурация (низкий приоритет)

1. Добавить `IEntityTypeConfiguration<>` для всех сущностей.
2. Удалить `OnConfiguring`/`null!` в `HomeGeBotDbContext`.
3. Вынести `ConnectionStringFactory` в абстракцию.
4. Почистить пустые/устаревшие миграции.

### Итерация 3 — Расширение тестов (высокий приоритет)

1. `Domain.Tests` для `ChatId`, инвариантов, enum.
2. `Infrastructure.Tests` для `ConnectionStringFactory`, `DateTimeService`, аудита.
3. `Web.Tests` с `WebApplicationFactory`: TMA, webhook, авторизация.
4. `Bot.Tests` для `CommandAnalyzer`, `MessageUpdateHandler`, `AppTextCommand`.

### Итерация 4 — Локализация и OCP (средний)

1. Вынести `ApplicationMessages` в ресурсы/БД.
2. Вынести `Other` из `Country`/`Term` в UI-слой.
3. `Message` — заменить `BodyEn/BodyKa` на `Dictionary<string, string>`.

### Итерация 5 — Мелкие архитектурные шероховатости

1. Убрать `global using Microsoft.EntityFrameworkCore`.
2. `IDateTime` в handler'ах.
3. `Result<T>` для всех query.
4. Валидаторы для query.
5. Декомпозиция `GetAdminDashboardHandler` и `BaseMessage`.

---

## 5. Риски

- **Domain-инкапсуляция** потребует новой миграции и тщательного тестирования EF-конфигурации.
- **Удаление `TlgUser.IsAdmin`** затронет `StartBot` и авторизацию; требуется аудит всех потребителей.
- **Рефакторинг `Message`** с `BodyEn/BodyKa` → `Dictionary` затронет seeding и миграции.
- **Web-тесты** с `WebApplicationFactory` потребуют конфигурации in-memory/тестовой БД.

---

*Документ обновлён после выполнения семи фаз рефакторинга. Все завершённые фазы отражены в git-истории ветки `Upgrade`.*
