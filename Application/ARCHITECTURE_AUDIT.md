# Архитектурный аудит проекта Property In Tbilisi Bot

## 1. Общие сведения, технологический стек и функциональность

**Дата аудита:** 2026-08-02  
**Целевая платформа:** .NET 10.0  
**Тип приложения:** ASP.NET Core веб-приложение + Telegram-бот (webhook) с админ-панелью и Telegram Mini App (TMA).

### 1.1. Стек

| Слой / аспект | Технологии |
|---------------|------------|
| Платформа | .NET 10.0, ASP.NET Core, Razor Pages, Minimal APIs / Controllers |
| Архитектура | Clean Architecture, CQRS (MediatR 12.0), Result Pattern |
| Валидация | FluentValidation 11.5 |
| БД | PostgreSQL, EF Core 10.0, Npgsql 10.0 |
| Бот | Telegram.Bot 18.0 (webhook) |
| Веб-UI | Razor Pages, Telegram WebApp SDK, JavaScript (tma-i18n.js) |
| Сериализация | Newtonsoft.Json |
| API-документация | Swashbuckle Swagger |
| Кэш / сессии | `IDistributedCache`, `IMemoryCache` (TTL 1 день) |

### 1.2. Слои и направление зависимостей

```
┌──────────────────────────────────────────────────────────────┐
│ Web (Razor Pages + API Controllers + Middleware + TMA)       │
└───────────────┬──────────────────────────────┬───────────────┘
                │                              │
        ┌───────▼───────┐              ┌───────▼───────┐
        │ Bot           │              │ Infrastructure│
        │ (Telegram)    │              │ (EF Core, PG) │
        └───────┬───────┘              └───────┬───────┘
                │                              │
                └──────────────┬───────────────┘
                               │
                       ┌───────▼───────┐
                       │ Application   │
                       │ (MediatR,     │
                       │  Validators)  │
                       └───────┬───────┘
                               │
                       ┌───────▼───────┐
                       │ Domain        │
                       │ (Entities,    │
                       │  Value Objects│
                       └───────────────┘
```

- **Domain** — чистые сущности, value objects, enum'ы; ссылка только на `Microsoft.Extensions.Configuration` (`EnumExtensions`).
- **Application** — CQRS use-cases (MediatR), pipeline behaviors, Result Pattern, FluentValidation, порты (`IBotDbContext`, `IDateTime`, `IUserNotifier`).
- **Infrastructure** — реализация `DbContext` (PostgreSQL), конфигурации EF Core, миграции, `DateTimeService`, `AuditableEntitySaveChangesInterceptor`.
- **Bot** — инициализация Telegram-бота, приём Updates, роутинг команд, сессии, локализация, уведомления.
- **Web** — `Program.cs`, Razor Pages, API-контроллеры, middleware, валидация TMA `initData`, cookie-аутентификация.

### 1.3. Функциональность

**Пользовательский поток (TMA + Bot):**
1. Пользователь запускает `/start` в боте.
2. Бот приветствует и предлагает открыть TMA (Telegram Mini App) для заполнения анкеты.
3. В TMA собираются данные: страна, профессия, наличие животных, срок аренды.
4. Пользователь возвращается в чат и пересылает пост из канала с объектом (`app`-команда / `WaitForFlatForward`).
5. Система сохраняет заявку (`Client`), привязывает `Flat`, автоматически назначает активного менеджера с наименьшей загрузкой.
6. Менеджер получает уведомление с анкетой и данными объекта.

**Административные сценарии:**
- Просмотр дашборда всех заявок, менеджеров, объектов.
- Закрытие заявок, переназначение клиентов, удаление объектов, редактирование комментариев.
- Управление администраторами: повышение/отзыв прав (SuperAdmin/Admin).
- Профиль менеджера и получение фото пользователя через Telegram API.

---

## 2. Пофайловый обзор

### 2.1. Domain

| Файл | Назначение | Ключевые типы | Замечания |
|------|------------|---------------|-----------|
| `Common/BaseEntity.cs` | Базовый идентификатор | `BaseEntity` (`Id`) | Все свойства `public set`; стоит убрать `set` для `Id`. |
| `Common/BaseAuditableEntity.cs` | Аудит `CreatedAt` / `LastModified` | `BaseAuditableEntity : BaseEntity` | Те же `public set` — аудит устанавливается вне сущности. |
| `Common/ChatId.cs` | Value Object для Telegram chat id | `record struct ChatId` | ✅ Неявные преобразования в `long`, `IComparable`, `ToString`. |
| `Common/EnumExtensions.cs` | `DisplayAttribute → localized name` | `EnumExtensions` | ⚠️ Зависит от `System.ComponentModel.DataAnnotations` в Domain. |
| `Entities/Admin.cs` | Администратор | `Admin` | ✅ Методы `Activate/Deactivate/AssignClient/RemoveClient`. `Role` имеет `public set`. |
| `Entities/Client.cs` | Заявка клиента | `Client` | ✅ `ChangeManager`, `Complete`, `UpdateDetails`; `Admin` инициализируется `= null!`. |
| `Entities/Flat.cs` | Объект недвижимости | `Flat` | ✅ `UpdateComment`, `UpdateDetails`; минимальная валидация. |
| `Entities/Message.cs` | Сообщение (мультиязычное) | `Message` | ⚠️ `BodyEn/BodyKa` — OCP-проблема при добавлении языков. |
| `Entities/TlgUser.cs` | Пользователь Telegram | `TlgUser` | ✅ `UpdateProfile/SetLanguage/SetKicked`; `Language` default = "ru". |
| `Enums/AdminRole.cs` | Роли | `AdminRole` | `Admin`, `SuperAdmin`. |
| `Enums/Country.cs` | Страны | `Country` | `Other` — UI-концерн в доменном enum. |
| `Enums/Term.cs` | Сроки аренды | `Term` | `Other` — UI-концерн в доменном enum. |

### 2.2. Infrastructure

| Файл | Назначение | Ключевые типы | Замечания |
|------|------------|---------------|-----------|
| `ConfigureServices.cs` | Регистрация DI | `ConfigureService` | ⚠️ Прямое `new ConnectionStringProvider` вместо DI. |
| `Persistence/HomeGeBotDbContext.cs` | EF Core контекст | `HomeGeBotDbContext : DbContext, IBotDbContext` | ✅ Приватный `ChatIdConverter` для value object. Пустой конструктор `HomeGeBotDbContext()` стоит сделать `protected`. |
| `Persistence/IConnectionStringProvider.cs` | Порт строки подключения | `IConnectionStringProvider` | ✅ Узкий интерфейс. |
| `Persistence/ConnectionStringProvider.cs` | Реализация провайдера | `ConnectionStringProvider` | ⚠️ `DefaultConnection` magic string; `string.Empty` вместо исключения. |
| `Persistence/ConnectionStringFactory.cs` | Парсинг `DATABASE_URL` | `static ConnectionStringFactory` | ⚠️ `catch (Exception)`, hardcoded SSL, статичность — сложно тестировать. |
| `Persistence/DesignTimeDbContextFactory.cs` | Фабрика для миграций | `DesignTimeDbContextFactory` | ⚠️ Дублирование логики `ConnectionStringProvider`. |
| `Persistence/Configurations/*Configuration.cs` | EF конфигурации | 5 пустых `IEntityTypeConfiguration<>` | ⚠️ Содержат только `ToTable`/`HasKey` — не настроены индексы/связи. |
| `Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs` | Аудит | `AuditableEntitySaveChangesInterceptor` | ✅ `IDateTime` через DI; `UpdateEntities` лучше сделать `private`. |
| `Persistence/Interceptors/EntityEntryExtensions.cs` | Owned entities check | `EntityEntryExtensions` | ✅ Полезный `HasChangedOwnedEntities`. |
| `Services/DateTimeService.cs` | Время | `DateTimeService : IDateTime` | ✅ `Now` = `DateTime.UtcNow`. |

### 2.3. Application

**Общие инфраструктурные файлы:**

| Файл | Назначение | Замечания |
|------|------------|-----------|
| `GlobalUsing.cs` | Глобальные using | ⚠️ `global using Microsoft.EntityFrameworkCore` в Application нарушает Clean Architecture. |
| `ConfigureServices.cs` | Регистрация MediatR, FluentValidation, behaviors | ✅ Корректно. |
| `Common/Results/Result.cs` | Result Pattern | ✅ `Success/Fail`, `Match`, фабрика. |
| `Common/Behaviors/ResultValidationBehavior.cs` | Pipeline валидации | ✅ Автоматический `IValidator<TRequest>`. |
| `Common/Behaviors/ResultAuthorizationBehavior.cs` | Admin-авторизация | ✅ Маркер `IAdminCommand`. |
| `Common/Behaviors/ResultSuperAdminAuthorizationBehavior.cs` | SuperAdmin-авторизация | ✅ Маркер `ISuperAdminCommand`. |
| `Common/Interfaces/IBotDbContext.cs` | Порт БД | ⚠️ `DbSet<T>` возвращает `DbSet<T>` — тянет EF. |
| `Common/Interfaces/IDateTime.cs` | Порт времени | ✅ Используется в Infrastructure. Почти не используется в Application (например, `GrantAdminRightsHandler` использует `DateTime.UtcNow`). |
| `Common/Interfaces/IUserNotifier.cs` | Порт уведомлений | ✅ Реализован в `Bot.Services.UserNotifier`. |
| `Common/Extensions/UserExtensions.cs` | ФИО форматирование | ✅ Тестируется. |
| `Common/Localization/ApplicationMessages.cs` | Жёстко зашитые строки ошибок | ⚠️ OCP-нарушение: добавление языка/сообщения требует правки кода. |
| `Common/Localization/SupportedLanguages.cs` | Языки | ✅ `ru`, `en`, `ka`; статичность. |
| `Common/Validation/ChatIdValidationExtensions.cs` | Валидация `ChatId` | ✅ `.MustBeValidChatId()`. |

**Feature-группы и use-cases:**

| Группа | Use-case | Авторизация | Валидатор | Примечания |
|--------|----------|-------------|-----------|------------|
| `AdminManagement` | `CloseRequest` | `IAdminCommand` | ✅ | Закрытие заявки. |
| | `GetBotUsers` | `ISuperAdminCommand` | ✅ | Список пользователей, исключая активных админов. |
| | `GrantAdminRights` | `ISuperAdminCommand` | ✅ | Использует `DateTime.UtcNow` вместо `IDateTime`. |
| | `ReassignClient` | `ISuperAdminCommand` | ✅ | Переназначение активных заявок. |
| | `RevokeAdminRights` | `ISuperAdminCommand` | ✅ | Запрет само-отзыва / отзыва SuperAdmin; перераспределение. |
| | `CheckAdminStatus` | ❌ | ❌ | Query возвращает `bool`, а не `Result<bool>`. |
| `BotStart` | `StartBot` | — | ✅ | Upsert `TlgUser`, синхронизация с `Admins`. |
| `Dashboard` | `GetAdminDashboard` | `IAdminCommand` | ✅ | Самый сложный handler; 3 приватных метода — SRP-риск. |
| | `DeleteFlat` | `IAdminCommand` | ✅ | Удаление объекта. |
| | `UpdateFlatComment` | `IAdminCommand` | ✅ | Изменение комментария. |
| `Messages` | `GetMessageBody`, `GetMessagePathToPhoto` | — | ❌ | Query без `Result<T>`. |
| `RentalApplications` | `SubmitRentalApplication` | — | ✅ | Выбор менеджера с min load, лимит 5. |
| | `GetUserApplications` | — | — | N+1 подзапрос `ManagerUsername` (проверить в текущей версии). |
| `TlgUsers` | `ToggleUserKick` | — | ✅ | Корректно не требует админ-прав (собственный chat id). |
| | `CheckUserStatus` | — | ❌ | Query без `Result<T>`. |
| `Users` | `SetUserLanguage` | — | ✅ | Изменение языка. |
| | `GetManagerContact` | — | — | Query возвращает `Result<ManagerDto>`. |

### 2.4. Bot

| Файл / группа | Назначение | Замечания |
|---------------|------------|-----------|
| `Common/TelegramBot.cs` | `ITelegramBotClientProvider` + `WebhookSetupService` | ⚠️ Два класса в одном файле; `SemaphoreSlim` double-check locking; `TelegramBot` зависит от `TelegramBotConfiguration`. |
| `Common/CommandAnalyzer.cs` | Диспетчер `Update` | ⚠️ SRP: выбор handler + обработка исключений. |
| `Common/Abstractions/BaseTextCommand.cs` | Базовый класс текстовых команд | ✅ Простой, чистый. |
| `Common/Abstractions/BaseCallbackCommand.cs` | Базовый класс callback-команд | ⚠️ Проверка первого символа может привести к коллизиям. |
| `Common/Abstractions/BaseMessage.cs` | Базовый класс сообщений | ⚠️ SRP: язык, текст, отправка, inline keyboard в одном. |
| `Routers/TextCommandRouter.cs` | Маршрутизация текстовых команд | ⚠️ `IEnumerable` + линейный поиск; fallback внутри роутера. |
| `Routers/CallbackCommandRouter.cs` | Маршрутизация callback | ⚠️ Выполняет **все** подходящие команды, а не первую. |
| `UpdateHandlers/*UpdateHandler.cs` | Обработчики `UpdateType` | ✅ `CanHandle/HandleAsync`, `IUpdateHandler`. |
| `Services/MessageService.cs` | Отправка/редактирование сообщений | ⚠️ `IMessageService` слишком широк (7 методов — ISP). Silent fail в `SendMessage` если путь null. |
| `Services/BotI18n.cs` | Локализация бота | ⚠️ Словарь захардкожен в коде; OCP-нарушение при добавлении языков. |
| `Services/LocalizedMessageResolver.cs` | Резолвер сообщений из БД | ✅ Тонкий шлюз в Application. |
| `Services/ManagerNotificationFormatter.cs` | Форматирование уведомлений | ⚠️ Вложенные тернарные операторы, хардкод Yes/No. |
| `Services/RentalApplicationForwardProcessor.cs` | Форвард заявки менеджеру | ⚠️ Длинный метод; смешивает CQRS, пересылку, форматирование, очистку сессии. |
| `Services/UserNotifier.cs` | Отправка уведомлений пользователям | ✅ Корректно. |
| `Services/UserStatusChecker.cs` | Проверка `IsKicked` | ✅ Корректно. |
| `Session/BotSession.cs` | Сессия пользователя | ⚠️ Все свойства `public set`; нет валидации. |
| `Session/DistributedBotSessionStore.cs` | Хранилище в `IDistributedCache` | ⚠️ Hardcoded TTL 1 день, прямая зависимость от Newtonsoft.Json. |
| `Session/MemoryBotSessionStore.cs` | Хранилище в памяти | ✅ Альтернативная реализация. |
| `Session/IBotSessionStore.cs` | Порт хранилища | ✅ Узкий интерфейс. |
| `Configuration/*Configuration.cs` | Конфигурации | ⚠️ Нет валидации; пустые значения по умолчанию. |
| `Exceptions/*Exception.cs` | Свои исключения | ✅ Простые. `SessionExpiredException` — русский текст захардкожен. |

### 2.5. Web

| Файл / группа | Назначение | Замечания |
|---------------|------------|-----------|
| `Program.cs` | DI, middleware pipeline | ✅ Стандартная конфигурация. Дублирование `CookieSecurePolicy`. |
| `Controllers/TelegramBotController.cs` | Webhook endpoint | ⚠️ Валидация SecretToken внутри контроллера, а не middleware; `catch` внутри action. |
| `Controllers/TmaController.cs` | TMA API | 🔴 Массовое дублирование валидации `initData` (5 методов); `GetManager` не проверяет `userId`. |
| `Controllers/AdminAuthController.cs` | Аутентификация админов | ⚠️ `AdminAuthRequest` в том же файле; `AuthDebug` доступен в коде. |
| `Controllers/DashboardApiController.cs` | API дашборда | ⚠️ `GetUserPhoto` делает слишком много: API, скачивание, файл; ручной `try-catch` при наличии middleware. |
| `Middleware/GlobalExceptionMiddleware.cs` | Глобальная обработка ошибок | ⚠️ Switch expression для 3 типов исключений — OCP-нарушение. |
| `Pages/Index.cshtml.cs` | Razor Page дашборда | 🔴 God object: 5 POST-действий + ручной маппинг; `GetCurrentAdminChatId` возвращает `0` при отсутствии claim. |
| `Pages/Login.cshtml.cs` | Страница входа | ✅ Простая. |
| `Pages/Tma.cshtml.cs` | TMA PageModel | ✅ Пустая, можно убрать code-behind. |
| `Pages/Shared/Tma/_TmaPartialsModels.cs` | Модели partials | ⚠️ Имена с `_` в начале — нарушение C# naming. |
| `Models/*.cs` | DTO ViewModel | ✅ Простые; лучше использовать `record` и Data Annotations. |
| `Models/Validators/TmaApplicationRequestValidator.cs` | Валидация TMA | ✅ FluentValidation. |
| `Services/TmaValidationService.cs` | Валидация и парсинг `initData` | 🔴 SRP + ISP: валидация, парсинг, извлечение `userId/username/userdata` в одном сервисе. |
| `Services/BotInitializationService.cs` | IHostedService инициализации | ✅ Корректно. |
| `Services/AdminClaimsFactory.cs` | ClaimsPrincipal | ✅ Чистая фабрика; magic strings. |

### 2.6. Tests

| Проект / файл | Что тестируется | Замечания |
|---------------|-----------------|-----------|
| `Application.Tests/Common/TestDbContext.cs` | In-memory `IBotDbContext` | ✅ Удобный тестовый контекст. |
| `Application.Tests/Extensions/UserExtensionsTests.cs` | `GetFullName` | ✅ Параметризованные; хорошее покрытие. |
| `Application.Tests/Localization/*` | `ApplicationMessages`, `SupportedLanguages` | ✅ Theory. Хардкод строк — хрупко. |
| `Application.Tests/Mappings/FlatMappingExtensionsTests.cs` | `Flat → FlatDto` | ✅ Граничные случаи. |
| `Application.Tests/RentalApplications/SubmitRentalApplicationHandlerTests.cs` | Главный use-case | ✅ In-memory + NSubstitute. Недостаточно edge cases. |
| `Application.Tests/Users/*` | `SetUserLanguage`, `GetManagerContact` | ✅ Базовые сценарии. |
| `Bot.Tests/Services/BotI18nTests.cs` | Локализация | ✅ Fallback. Хардкод строк. |

**Проблема покрытия:** нет `Domain.Tests`, `Infrastructure.Tests`, `Web.Tests`, тестов на `Bot` handlers/routers.

---

## 3. ООП и SOLID — итоговая оценка

### 3.1. ООП

| Принцип | Соблюдение | Комментарий |
|---------|------------|-------------|
| **Инкапсуляция** | ⚠️ | Domain-сущности в целом с `private set`, но `Admin.Role` и `BaseEntity.Id` — `public set`; `BotSession`, `RentalApplicationDraft` — публичные сеттеры. |
| **Наследование** | ✅ | `BaseEntity → BaseAuditableEntity → Entities`; `BaseTextCommand/BaseCallbackCommand/BaseMessage`; `IUpdateHandler` стратегии. |
| **Полиморфизм** | ✅ | MediatR handlers, pipeline behaviors, `IUpdateHandler`, `ICommandAnalyzer`, `ITextCommandRouter` подменяются через DI. |
| **Абстракция** | ✅ | Интерфейсы в `Application.Common.Interfaces` и `Bot.Common.Interfaces`; `IBotDbContext`, `IDateTime`, `IUserNotifier` и т.д. |

### 3.2. SOLID

| Принцип | Соблюдение | Комментарий |
|---------|------------|-------------|
| **S — SRP** | ⚠️ | `GetAdminDashboardHandler`, `Index.cshtml.cs`, `TmaController`, `RentalApplicationForwardProcessor`, `BaseMessage` и `CommandAnalyzer` — несколько обязанностей. |
| **O — OCP** | ⚠️ | Pipeline и `IUpdateHandler` расширяемы; но `ApplicationMessages`, `Message.BodyEn/BodyKa`, `BotI18n` словари, `GlobalExceptionMiddleware` switch требуют правки кода. |
| **L — LSP** | ✅ | Наследники `BaseTextCommand`, `BaseMessage`, `BaseCallbackCommand` корректны; no LSP-нарушений. |
| **I — ISP** | ⚠️ | `IMessageService` (7 методов), `ITmaValidationService` (валидация+парсинг) широкие; `IBotDbContext` узкий. |
| **D — DIP** | ⚠️ | Web/Bot зависят от интерфейсов; `global using Microsoft.EntityFrameworkCore` в Application, `ConfigureServices` делает `new ConnectionStringProvider`, `ConnectionStringFactory` статический. |

---

## 4. Критичные проблемы и план рефакторинга

### 4.1. Критичный приоритет

1. **Web: убрать дублирование валидации `initData`**
   - Создать атрибут `[ValidateTmaInitData]` / фильтр или базовый контроллер `TmaControllerBase`.
   - `GetManager` должен валидировать пользователя.

2. **Web: валидация webhook SecretToken в middleware**
   - Вынести из `TelegramBotController` в middleware.
   - Использовать константу для заголовка `X-Telegram-Bot-Api-Secret-Token`.

3. **Web / Tma: разделить `TmaValidationService` на `ITmaInitDataParser` + `ITmaInitDataValidator`**
   - Кэшировать результат парсинга.
   - Вынести `WebAppData` в константу.

4. **Bot: `CallbackCommandRouter` должен выполнять только первую подходящую команду**
   - Заменить `IEnumerable` на `Dictionary` для O(1) поиска.

5. **Bot: `MessageService` не должен делать silent fail**
   - При null `pathToPhoto` выбрасывать исключение или логировать явно.

6. **Application: убрать `global using Microsoft.EntityFrameworkCore`**
   - Вынести EF-зависимость в Infrastructure; в Application работать через `IQueryable` из `IBotDbContext` без глобального using.

7. **Infrastructure: исправить `ConnectionStringProvider/Factory` зависимости**
   - Убрать прямое `new ConnectionStringProvider` в `ConfigureServices`.
   - `ConnectionStringFactory` сделать нестатическим / заменить на `Uri` + `NpgsqlConnectionStringBuilder`.
   - `catch (Exception)` заменить на `catch (UriFormatException)`.

### 4.2. Высокий приоритет

8. **Domain: усилить инкапсуляцию и валидацию**
   - `Admin.Role` → `private set` + `SetRole`.
   - `BaseEntity.Id` убрать setter.
   - Добавить null/role проверки в `Client.ChangeManager`, `Admin.AssignClient`.

9. **Domain: локализация `Message` — OCP**
   - Вынести `BodyEn/BodyKa` в сущность/JSONB `MessageTranslation`.
   - `Message.GetLocalizedBody(language)` в Domain.

10. **Application: `IDateTime` в handler'ах**
    - Заменить `DateTime.UtcNow` на `_dateTime.Now`.

11. **Application: `Result<T>` для всех Query и Validators**
    - `CheckAdminStatus`, `GetMessageBody`, `GetUserLanguage`, `CheckUserStatus` — `Result<T>` + валидаторы.

12. **Application: декомпозиция `GetAdminDashboardHandler`**
    - Разбить на 3 отдельных query: `GetDashboardApplications`, `GetDashboardFlats`, `GetDashboardManagers`.

13. **Bot: `BaseMessage` SRP/ISP**
    - Вынести язык в `ILanguageResolver`.
    - Разделить `IMessageService` на `IMessageSender`, `IMessageEditor`, `IMessageRepository`.

14. **Tests: расширить покрытие**
    - `Domain.Tests` (ChatId, invariants).
    - `Infrastructure.Tests` (ConnectionString, DateTime, Interceptor).
    - `Web.Tests` (`WebApplicationFactory`, TMA, webhook).
    - `Bot.Tests` (`CommandAnalyzer`, `TextCommandRouter`, `MessageUpdateHandler`).

### 4.3. Средний и низкий приоритет

15. **Bot: локализация из кода → ресурсы/JSON**
    - `BotI18n` словарь перенести в конфигурационный файл.

16. **Bot: `TelegramBot.cs` разделить на `TelegramBotClientProvider.cs` и `WebhookSetupService.cs`**
    - Рассмотреть `Lazy<TelegramBotClient>`.

17. **Web: `Index.cshtml.cs` PageModel**
    - Вынести POST-handlers в отдельные PageModels / API endpoints.
    - Вынести маппинг в `DashboardMapper`.
    - `GetCurrentAdminChatId` возвращать `long?`.

18. **Web: `GlobalExceptionMiddleware` OCP**
    - `Dictionary<Type, HttpStatusCode>` или `IExceptionHandler` стратегии.

19. **Infrastructure: наполнить `IEntityTypeConfiguration<>`**
    - Индексы (`TlgUser.ChatId`, `Admin.ChatId`, `Client.ChatId`, `Message.Name`).
    - `IsRequired`, ограничения, связи, `HasData` seeding.

20. **Infrastructure: `HomeGeBotDbContext` пустой конструктор**
    - Сделать `protected` для использования только EF proxy / design-time.

21. **Общее: убрать magic strings**
    - `"ru"`, `"DefaultConnection"`, `"DATABASE_URL"`, `"WebAppData"`, `"AdminAuth"`, `"X-Telegram-Bot-Api-Secret-Token"` — вынести в константы / options.

---

## 5. Общий вывод

Проект построен на хорошей архитектурной основе: Clean Architecture, CQRS, MediatR, EF Core, Telegram.Bot. Доменная модель в целом инкапсулирована, Web и Bot зависят от абстракций, pipeline behaviors обеспечивают централизованную валидацию и авторизацию. Основные риски связаны с **дублированием валидации `initData` в Web**, **широкими интерфейсами**, **анемичными EF-конфигурациями**, **захардкоженной локализацией** и **недостаточным тестовым покрытием** Web/Bot. Рекомендуется начать с критичных Web-правок (валидация TMA/webhook) и устранения EF-зависимости в Application, затем перейти к декомпозиции тяжёлых классов и расширению тестов.

---

## 6. Выполненная первая итерация рефакторинга (2026-08-02)

### 6.1. Что сделано

- Создан `ValidateTmaInitDataAttribute` (`Web/Filters`) — action filter на базе `TypeFilter` с DI-получением `ITmaValidationService`.
- Добавлен `TmaControllerBase` (`Web/Controllers`) с `TmaUserId`, `TmaUserData`, `TmaInitData`.
- Переписаны `TmaController` и `AdminAuthController`: убрано повторяющееся `if (!_tmaValidation.ValidateInitData(...))` и `if (userId == null)`; `initData` валидируется через `[ValidateTmaInitData]`.
- Сборка прошла успешно: `dotnet build` — 0 ошибок, 0 предупреждений.

### 6.2. Текущее состояние

- Критичное дублирование валидации `initData` в Web устранено.
- Валидация webhook SecretToken вынесена из `TelegramBotController` в middleware.
- Остальные критичные и приоритетные пункты из раздела 4 остаются на последующие итерации.

### 6.3. Вторая итерация: валидация webhook в middleware (2026-08-02)

- Создан `ValidateTelegramWebhookMiddleware` (`Web/Middleware`).
- Middleware проверяет путь `/api/message/update` и заголовок `X-Telegram-Bot-Api-Secret-Token`.
- В dev-режиме отсутствие `SecretToken` допускается, в production — запрос отклоняется.
- `TelegramBotController` очищен: удалены `_webhookConfig`, `_environment` и метод `ValidateWebhookRequest`.
- Middleware зарегистрировано в `Program.cs` между `UseForwardedHeaders` и `UseStaticFiles`.
- Сборка прошла успешно: `dotnet build Web -p:UseAppHost=false` — 0 ошибок, 0 предупреждений.