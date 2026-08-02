# План рефакторинга и отчёт код-ревью

Дата ревью: 2026-08-02  
Дата обновления: 2026-08-02  
Объект: `Property In Tbilisi bot` (.NET 10.0, Clean Architecture + CQRS + Telegram Bot)  
Критерии: принципы ООП и SOLID.  
Состояние сборки: `dotnet build` — успешно; `dotnet test` — 42 теста пройдены.  
База данных: применены миграции `20260802133650_RemoveTlgUserIsAdmin` и `20260802140014_UnifyClientAdminRelationship`.

---

## 1. Сводная оценка (актуализированная)

| Принцип | Соблюдение | Комментарий |
|---------|-----------|-------------|
| **Инкапсуляция** | ✅ Существенно улучшено | `Admin`, `Client`, `TlgUser`, `Flat`, `Message` обзавелись конструкторами, приватными сеттерами и доменными методами. |
| **Наследование** | ✅ Хорошо | Базовые классы используются корректно. |
| **Полиморфизм** | ✅ Хорошо | `IUpdateHandler`-стратегии, MediatR, pipeline behaviors, роутеры команд. |
| **Абстракция** | ✅ Существенно улучшено | `ITelegramBotClientProvider`, `IUpdateHandler`, `ITextCommandRouter`, `IRentalApplicationForwardProcessor`, `ITmaLabelProvider`, `IConnectionStringProvider`. |
| **SRP** | ✅ Существенно улучшено | `CommandAnalyzer` и `AppTextCommand` декомпозированы. `GetAdminDashboardHandler`, `BaseMessage`, `Index.cshtml.cs` всё ещё перегружены. |
| **OCP** | ⚠️ Частично | Pipeline behaviors, `IUpdateHandler` расширяемы; `ApplicationMessages`, `Message.BodyEn/BodyKa`, `Other` в enum'ах требуют правок кода. |
| **LSP** | ✅ Хорошо | Нарушений не обнаружено. |
| **ISP** | ✅ Хорошо | Интерфейсы узкие. |
| **DIP** | ✅ Существенно улучшено | Web и Bot зависят от абстракций. `global using Microsoft.EntityFrameworkCore` в Application всё ещё остаётся. |

**Общий вердикт**: основные критичные риски (безопасность Web, прямой доступ `TmaController` к БД, монолитные `CommandAnalyzer`/`AppTextCommand`, дублирование `IsAdmin`/`AdminChatId`) устранены. Проект собирается, тесты проходят, миграции применены. Остался техдолг вокруг расширяемой локализации `Message`, `IEntityTypeConfiguration`, `IDateTime` и покрытия тестами Web/Bot.

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
| 8. Удаление `TlgUser.IsAdmin` | `a1e24a5` | ✅ Убрано дублирование; проверка админ-статуса только через `Admins` |
| 9. Унификация `Client–Admin` | `161c57a` | ✅ `Client.Admin` + `AdminId` FK; удалена `Client.AdminChatId`; миграция применена |

---

## 3. Оставшиеся нарушения и план дальнейшего рефакторинга

### 3.1 Domain (средний приоритет)

- **D5**: `Message.BodyEn/BodyKa` не расширяемы (OCP). При добавлении языка нужно менять сущность и миграции.
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

- **I1**: `ConnectionStringFactory` вынесен в `IConnectionStringProvider` / `ConnectionStringProvider`, но `ConnectionStringFactory` остаётся статическим помощником для парсинга URL.
- **I4**: добавлены начальные `IEntityTypeConfiguration<>`, но они минимальны (только `ToTable`/`HasKey`). Нужно расширять: индексы, ограничения, связи, `HasData` seeding.
- **I5**: пустая миграция `UpdateChatIdToValueObject` остаётся.
- **I6**: seeding сообщений в миграции.

### 3.5 Tests (высокий)

- Покрытие остаётся низким. Нужны `Domain.Tests`, `Infrastructure.Tests`, `Web.Tests`, тесты на `CommandAnalyzer`/handlers, валидацию, pipeline behaviors.

---

## 4. Рекомендуемые следующие шаги

### Итерация 1 — Расширяемая локализация `Message` (средний приоритет)

1. Вынести переводы `BodyEn/BodyKa` в отдельную сущность `MessageTranslation` (или JSONB-колонку).
2. Обновить `GetMessageBodyQuery` для выбора перевода по коду языка.
3. Добавить seeding через `IEntityTypeConfiguration.HasData` вместо `migrationBuilder.Sql`.
4. Сгенерировать и применить миграцию.

### Итерация 2 — Расширение `IEntityTypeConfiguration` (низкий приоритет)

1. Добавить индексы (`TlgUser.ChatId`, `Admin.ChatId`, `Client.ChatId`, `Message.Name`).
2. Указать `IsRequired` для обязательных строк и FK.
3. Настроить каскадное удаление/ограничение по `Client.AdminId`.
4. Перенести seeding сообщений из миграций в `MessageConfiguration.HasData`.

### Итерация 3 — Расширение тестов (высокий приоритет)

1. `Domain.Tests` для `ChatId`, инвариантов сущностей, enum.
2. `Infrastructure.Tests` для `ConnectionStringFactory`, `DateTimeService`, `AuditableEntitySaveChangesInterceptor`.
3. `Web.Tests` с `WebApplicationFactory`: TMA, webhook, авторизация.
4. `Bot.Tests` для `CommandAnalyzer`, `MessageUpdateHandler`, `AppTextCommand`.

### Итерация 4 — Мелкие архитектурные шероховатости

1. Убрать `global using Microsoft.EntityFrameworkCore`.
2. `IDateTime` в handler'ах.
3. `Result<T>` для всех query.
4. Валидаторы для query.
5. Декомпозиция `GetAdminDashboardHandler` и `BaseMessage`.

### Итерация 5 — Очистка миграций

1. Схлопнуть историю миграций в одну начальную, когда схема стабилизируется (потребует пересоздания БД).
2. Удалить пустую `UpdateChatIdToValueObject`.

---

## 5. Как применять миграции

Текущий `DesignTimeDbContextFactory` использует переменную окружения `DATABASE_URL`. Для локальной БД используйте:

```powershell
$env:DATABASE_URL = "postgres://user:password@host:5432/db"
dotnet ef database update -p Infrastructure -s Web --no-build
```

или передайте строку подключения явно:

```powershell
dotnet ef database update -p Infrastructure -s Web --no-build `
  --connection "Host=localhost;Port=5432;Database=homeGeBot;Username=...;Password=..."
```

> `appsettings.json` с паролем не должен попадать в git. Убедитесь, что он указан в `.gitignore`.

---

## 6. Риски

- **Рефакторинг `Message`** с `BodyEn/BodyKa` → отдельная таблица/JSON затронет seeding и миграции.
- **Web-тесты** с `WebApplicationFactory` потребуют конфигурации in-memory/тестовой БД.
- **Схлопывание миграций** потребует пересоздания БД в проде.

---

*Документ обновлён после выполнения всех семи плановых фаз, удаления `TlgUser.IsAdmin`, унификации `Client–Admin` и применения миграций.*
