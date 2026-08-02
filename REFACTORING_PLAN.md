# План рефакторинга и отчёт код-ревью

Дата ревью: 2026-08-02
Объект: `Property In Tbilisi bot` (.NET 10.0, Clean Architecture + CQRS)
Критерии: принципы ООП (инкапсуляция, наследование, полиморфизм, абстракция) и SOLID (SRP, OCP, LSP, ISP, DIP).

---

## 📑 Содержание
- [1. Сводная оценка](#1-сводная-оценка)
- [2. Нарушения по слоям](#2-нарушения-по-слоям)
  - [2.1 Domain](#21-domain)
  - [2.2 Application](#22-application)
  - [2.3 Infrastructure](#23-infrastructure)
  - [2.4 Bot](#24-bot)
  - [2.5 Web](#25-web)
- [3. Сквозные проблемы](#3-сквозные-проблемы)
- [4. План рефакторинга](#4-план-рефакторинга)
  - [Фаза 1 — Критичные исправления безопасности и корректности](#фаза-1--критичные-исправления-безопасности-и-корректности)
  - [Фаза 2 — Устранение мёртвого кода и дублирования](#фаза-2--устранение-мёртвого-кода-и-дублирования)
  - [Фаза 3 — Избавление от статических сервисов (DIP)](#фаза-3--избавление-от-статических-сервисов-dip)
  - [Фаза 4 — Декомпозиция по SRP](#фаза-4--декомпозиция-по-srp)
  - [Фаза 5 — Локализация и OCP](#фаза-5--локализация-и-ocp)
  - [Фаза 6 — Инфраструктурные улучшения](#фаза-6--инфраструктурные-улучшения)
  - [Фаза 7 — Тестирование](#фаза-7--тестирование)
- [5. Приоритеты и риски](#5-приоритеты-и-риски)

---

## 1. Сводная оценка

| Принцип | Соблюдение | Комментарий |
|---------|-----------|-------------|
| **Инкапсуляция** | ⚠️ Частично | Сущности имеют только публичные auto-properties; нет инвариантов; `List<Client>` публичен и изменяем извне. |
| **Наследование** | ✅ Хорошо | `BaseEntity` → `BaseAuditableEntity` → сущности; `BaseTextCommand`/`BaseCallbackCommand`/`BaseMessage` — корректные абстракции. |
| **Полиморфизм** | ✅ Хорошо | MediatR handlers, pipeline behaviors, marker-интерфейсы — полиморфизм используется уместно. |
| **Абстракция** | ⚠️ Частично | `IBotDbContext`, `ICommandAnalyzer`, `IBotSessionStore`, `ITmaValidationService` — хорошо; но `MessageService`, `BotI18n`, `ConnectionStringFactory` — статические классы без абстракций. |
| **SRP** | ❌ Слабо | `CommandAnalyzer`, `AppTextCommand`, `GetAdminDashboardHandler`, `TmaController`, `Index.cshtml.cs`, `TelegramBot` делают слишком много. |
| **OCP** | ⚠️ Частично | Команды расширяемы через `IEnumerable<BaseTextCommand>`, но локализация, типы Update, статические сервисы требуют изменения существующего кода. |
| **LSP** | ✅ Хорошо | Нарушений не обнаружено. |
| **ISP** | ✅ Хорошо | Интерфейсы узкие и предметно-ориентированные. |
| **DIP** | ❌ Слабо | Статические `MessageService`/`BotI18n` используются везде; `CommandAnalyzer` зависит от конкретного `ClientStartMessage`; контроллеры Web работают с `IBotDbContext` напрямую (допустимо, но в идеале — через use cases). |

**Общий вердикт**: архитектура проекта здравая и хорошо структурированная для своего масштаба, но накопился ряд нарушений DIP (статические сервисы) и SRP (крупные классы-«боги»), а также присутствуют критичные пробелы в авторизации (`ToggleUserKickCommand`, `CloseRequestRequest`) и неиспользуемые behavior-классы.

---

## 2. Нарушения по слоям

### 2.1 Domain

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| D1 | `Entities/Admin.cs`, `Client.cs`, `Flat.cs`, `Message.cs`, `TlgUser.cs` | Все свойства — публичные auto-properties с `set`; нет инвариантов, нет инкапсуляции состояния. Любой код может перевести сущность в невалидное состояние. | Инкапсуляция | Средняя |
| D2 | `Entities/Admin.cs` | `public List<Client> Clients { get; set; } = new();` — публичная изменяемая коллекция. Нарушает инкапсуляцию агрегата. | Инкапсуляция | Средняя |
| D3 | `Common/ChatId.cs` | `record struct` с `implicit operator` к/из `long` — неявные преобразования фактически сводят на нет типобезопасность (любой `long` тихо становится `ChatId`). | Абстракция | Низкая |
| D4 | `Common/EnumExtensions.cs` | Рефлексия при каждом вызове `GetDisplayName` без кэширования. | — | Низкая |
| D5 | `Domain.csproj` | Зависимость от `Microsoft.Extensions.Configuration` в Domain — нарушение чистоты слоя (Domain не должен зависеть от инфраструктурных пакетов). | DIP | Низкая |
| D6 | `Exception/` | Пустая папка, объявленная в `.csproj` через `<Folder Include="Exception\" />`. Мёртвая структура. | — | Низкая |

### 2.2 Application

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| A1 | `Common/Behaviors/ValidationBehavior.cs`, `AuthorizationBehavior.cs`, `SuperAdminAuthorizationBehavior.cs` | Классы объявлены, но **не зарегистрированы** в `ConfigureServices.cs`. Дублируют `Result*`-версии. Мёртвый код. | YAGNI/DIP | Высокая |
| A2 | `AdminManagement/Commands/CloseRequest/CloseRequestRequest.cs` | Содержит `AdminChatId`, но **не реализует** `IAdminCommand` → авторизация через behavior не применяется. | Безопасность/SRP | **Критичная** |
| A3 | `AdminManagement/Commands/CloseRequest/` | Отсутствует валидатор. | — | Средняя |
| A4 | `TlgUsers/Commands/ToggleUserKick/ToggleUserKickCommand.cs` | Не реализует `IAdminCommand`/`ISuperAdminCommand`, нет валидатора, не возвращает `Result`. Любой пользователь может кикнуть любого через `MyChatMember` update. | Безопасность | **Критичная** |
| A5 | `Common/Interfaces/IDateTime.cs` | Интерфейс объявлен и реализован в Infrastructure, но **нигде не используется** в handler'ах — везде `DateTime.UtcNow` напрямую. | DIP | Средняя |
| A6 | `Dashboard/Commands/GetAdminDashboard/GetAdminDashboardHandler.cs` | Нарушение SRP: один handler собирает заявки, квартиры, менеджеров, имена, флаг супер-админа. Несколько приватных методов с разной логикой. | SRP | Средняя |
| A7 | `Dashboard/Commands/DeleteFlat/DeleteFlatHandler.cs`, `UpdateFlatComment/UpdateFlatCommentHandler.cs`, `GetAdminDashboard/GetAdminDashboardHandler.cs` | Дублирование маппинга `Flat` → `FlatDto`. | DRY | Средняя |
| A8 | `RentalApplications/Commands/SubmitRentalApplication/SubmitRentalApplicationHandler.cs` | Локализованные сообщения об ошибках зашиты в код вместо `Messages`/ресурсов. | OCP/SRP | Средняя |
| A9 | `Users/Queries/GetUserLanguage/GetUserLanguageQuery.cs` | `ChatId` объявлен как `long`, а не как `Domain.Common.ChatId` — несоответствие с остальным кодом. | Согласованность | Низкая |
| A10 | `RentalApplications/Queries/GetUserApplications/GetUserApplicationsHandler.cs` | N+1: подзапрос `ManagerUsername` в `Select` выполняется для каждой заявки. | Производительность | Низкая |
| A11 | `AdminManagement/Commands/GrantAdminRights/GrantAdminRightsHandler.cs` | Использует `DateTime.UtcNow` напрямую вместо `IDateTime`. | DIP | Низкая |
| A12 | `Application.csproj` | `Microsoft.EntityFrameworkCore` 7.0.0 в Application при том, что Infrastructure использует 10.0.10 — рассинхронизация версий. | — | Средняя |

### 2.3 Infrastructure

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| I1 | `Persistence/ConnectionStringFactory.cs` | Статический класс — нельзя подменить в тестах, нарушение DIP. | DIP | Средняя |
| I2 | `Persistence/ConnectionStringFactory.cs` | Использование `null!` подавляет nullable-проверки. | — | Средняя |
| I3 | `Persistence/ConnectionStringFactory.cs` | Magic string `"DefaultConnection"`. | — | Низкая |
| I4 | `Persistence/HomeGeBotDbContext.cs` | Пустой конструктор без параметров для миграций + `null!` при вызове `ConnectionStringFactory.GetConnectionString(null!)`. | — | Средняя |
| I5 | `Persistence/HomeGeBotDbContext.cs` | `ChatIdConverter` как вложенный класс — затрудняет переиспользование. | — | Низкая |
| I6 | `Persistence/HomeGeBotDbContext.cs` | `SaveChangesAsync` переопределён, но просто вызывает базовый — мёртвый код. | YAGNI | Низкая |
| I7 | `Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs` | Extension-метод `HasChangedOwnedEntities` в том же файле — нарушение «один класс — один файл». | — | Низкая |
| I8 | `ConfigureServices.cs` | Класс `ConfigureServices` назван корректно, но namespace `Microsoft.Extensions.DependencyInjection` загрязняет чужой namespace. | — | Низкая |
| I9 | `Services/DateTimeService.cs` | Зарегистрирован как Transient, хотя должен быть Singleton (`DateTime.UtcNow` — stateless). | — | Низкая |
| I10 | `Migrations/20260730184210_UpdateChatIdToValueObject.cs` | Пустая миграция (нет операций Up/Down). | — | Низкая |
| I11 | `Migrations/20260502000000_SeedMessages.cs` | Seeding локализованных текстов в миграции — трудно поддерживать. | — | Низкая |
| I12 | `Infrastructure.csproj` | Отсутствуют `IEntityTypeConfiguration<>` файлы — конфигурации сущностей неявны. | — | Низкая |

### 2.4 Bot

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| B1 | `Services/MessageService.cs` | **Статический класс** — главное нарушение DIP в проекте. Используется повсеместно, невозможно подменить в тестах, жёсткая привязка к `ParseMode.Html`. | DIP | **Высокая** |
| B2 | `Services/BotI18n.cs` | Статический класс с захардкоженными переводами. Невозможно расширить без изменения кода. | OCP/DIP | Высокая |
| B3 | `Common/CommandAnalyzer.cs` | Нарушение SRP: маршрутизация + проверка `isKicked` + логирование + обработка ошибок + бизнес-логика «перенаправить в TMA» в одном классе. | SRP | Высокая |
| B4 | `Common/CommandAnalyzer.cs` | Зависит от конкретного `ClientStartMessage` вместо абстракции (DIP). | DIP | Средняя |
| B5 | `Common/CommandAnalyzer.cs` | Жёсткие `if` по `UpdateType` — нарушение OCP (новый тип требует изменения класса). | OCP | Средняя |
| B6 | `Common/CommandAnalyzer.cs` | Дублирование проверки `isKicked` для CallbackQuery и Message. | DRY | Низкая |
| B7 | `Commands/ClientCommands/TextCommands/AppTextCommand.cs` | 160 строк, нарушение SRP: валидация канала + отправка заявки + форвард + формирование сообщения менеджеру (с локализацией pets/country/term) + уведомление клиента + очистка сессии. | SRP | Высокая |
| B8 | `Commands/ClientCommands/TextCommands/AppTextCommand.cs` | Локализация `petsDisplay` через тернарный оператор на 3 языка прямо в коде. | OCP | Средняя |
| B9 | `Commands/GeneralCommands/TextCommands/StartTextCommand.cs` | Инжекция трёх конкретных сообщений (`ClientStartMessage`, `AdminStartMessage`, `ManagerStartMessage`) вместо фабрики/стратегии. | DIP | Средняя |
| B10 | `Common/Abstractions/BaseMessage.cs` | SRP: локализация + отправка + редактирование в одном классе; статический вызов `MessageService`. | SRP/DIP | Средняя |
| B11 | `Common/TelegramBot.cs` | SRP: создание клиента + настройка webhook + логирование в одном классе. Жёстко зашитый путь `api/message/update`. | SRP/OCP | Средняя |
| B12 | `Services/ExceptionNotification.cs`, `Services/UserNotifier.cs` | Используют статический `MessageService` — нарушение DIP. | DIP | Средняя |
| B13 | `Common/Abstractions/BaseCallbackCommand.cs` | `Contains()` избыточен: `FirstOrDefault()` + `Contains()` — дублирование логики. | — | Низкая |

### 2.5 Web

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| W1 | `Controllers/TmaController.cs` | Нарушение SRP: бизнес-логика локализации, создание сессии, прямая работа с `IBotDbContext` в контроллере. | SRP/DIP | Высокая |
| W2 | `Controllers/TmaController.cs` | Hardcoded строки локализации (switch на 3 языка). | OCP | Средняя |
| W3 | `Controllers/TmaController.cs` | Возвращает анонимные объекты вместо DTO. | — | Средняя |
| W4 | `Controllers/TmaController.cs` | `GetManager` не валидирует `initData` пользователя. | Безопасность | Средняя |
| W5 | `Controllers/DashboardApiController.cs` | Прямая работа с Telegram API, нет кэширования фото, generic catch без логирования. | SRP | Средняя |
| W6 | `Controllers/AdminAuthController.cs` | Смешение аутентификации и отладочной логики (`auth-debug`). | SRP | Низкая |
| W7 | `Controllers/TelegramBotController.cs` | Silent catch без логирования при ошибке уведомления. | — | Низкая |
| W8 | `Pages/Index.cshtml.cs` | Нарушение SRP: 5 POST-обработчиков + маппинг DTO→ViewModel + дублирование логики «Other» для enum. | SRP | Средняя |
| W9 | `Pages/Index.cshtml.cs` | `GetCurrentAdminChatId` возвращает `0` при ошибке — нет обработки. | — | Низкая |
| W10 | `Models/FlatViewModel.cs` | Мёртвый код — не используется. | YAGNI | Низкая |
| W11 | `Models/TmaSetLanguageRequest.cs` | Нет валидации списка поддерживаемых языков. | — | Низкая |
| W12 | `GlobalSuppressions.cs` | Suppression для несуществующего типа `_FlatRowPartialModel`. | — | Низкая |
| W13 | `Services/BotInitializationService.cs` | Silent catch без логирования при ошибке инициализации. | — | Средняя |
| W14 | `wwwroot/js/dashboard.js` | jQuery + глобальные функции (устаревший стиль). | — | Низкая |
| W15 | `wwwroot/js/common.js`, `login.js` | Глобальные функции вне модулей. | — | Низкая |

---

## 3. Сквозные проблемы

1. **Статические сервисы как глобальное состояние.** `MessageService` и `BotI18n` — статические классы, используемые во всём слое Bot и частично в Web. Это блокирует unit-тестирование, нарушает DIP и делает невозможным декорирование (логирование, кэш, retry).

2. **Пробелы в авторизации.** `ToggleUserKickCommand` и `CloseRequestRequest` не проходят через authorization behaviors. Первый — критичная уязвимость (любой пользователь может кикнуть любого через `MyChatMember` update).

3. **Мёртвый код.** Неиспользуемые behaviors (`ValidationBehavior`, `AuthorizationBehavior`, `SuperAdminAuthorizationBehavior`), `IDateTime` (объявлен, не используется), `FlatViewModel`, пустая миграция, suppression для несуществующего типа.

4. **Дублирование маппингов.** `Flat` → `FlatDto` в трёх handler'ах; формирование полного имени пользователя в нескольких местах; логика «Other» для enum'ов в `Index.cshtml.cs` и `AppTextCommand`.

5. **Локализация разрознена.** Часть — в БД (`Messages`), часть — в статическом `BotI18n`, часть — в JS `tma-i18n.js`, часть — захардкожена в handler'ах и контроллерах. Нет единой точки.

6. **Рассинхронизация версий пакетов.** Application ссылается на `EF Core 7.0.0` и `DI.Abstractions 7.0.0`, тогда как Infrastructure/Web — на 10.0.10.

7. **Nullable-подавления.** `null!` в `ConnectionStringFactory` и `HomeGeBotDbContext` скрывают потенциальные NRE.

---

## 4. План рефакторинга

Рефакторинг разбит на 7 фаз, упорядоченных по убыванию критичности. Каждая фаза самодостаточна и может быть выполнена отдельным PR.

### Фаза 1 — Критичные исправления безопасности и корректности

**Цель:** закрыть уязвимости авторизации и рассинхронизацию пакетов.

| Шаг | Действие | Файлы | Принцип |
|-----|----------|-------|---------|
| 1.1 | `ToggleUserKickCommand`: реализовать `IAdminCommand`, добавить `AdminChatId`, валидатор, вернуть `Result`. В `CommandAnalyzer` передавать реальный `AdminChatId` (или сделать команду внутренней для системы, если кик должен срабатывать только от Telegram-события — тогда вынести в отдельный не-MediatR сервис). | `Application/TlgUsers/Commands/ToggleUserKick/*`, `Bot/Common/CommandAnalyzer.cs` | Безопасность |
| 1.2 | `CloseRequestRequest`: реализовать `IAdminCommand` (уже имеет `AdminChatId`), добавить валидатор `CloseRequestRequestValidator`. | `Application/AdminManagement/Commands/CloseRequest/*` | Безопасность |
| 1.3 | Выровнять версии пакетов: `Application.csproj` → `EF Core 10.0.10`, `DI.Abstractions 10.0.10`. Удалить `EF Core` из Application, если возможно (зависит только от `IBotDbContext`, который в Application). | `Application/Application.csproj` | — |
| 1.4 | `TmaController.GetManager`: валидировать `initData` через `ITmaValidationService` перед возвратом данных супер-админа. | `Web/Controllers/TmaController.cs` | Безопасность |
| 1.5 | Убрать silent catches в `BotInitializationService`, `TelegramBotController`, `DashboardApiController` — добавить `_logger.LogError`. | `Web/Services/BotInitializationService.cs`, `Web/Controllers/*` | — |

**Критерии приёмки:** компиляция без warnings, ручной тест: обычный пользователь не может кикнуть другого; закрытие заявки требует активного админа.

---

### Фаза 2 — Удаление мёртвого кода и дублирования

**Цель:** уменьшить поверхность поддержки.

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 2.1 | Удалить неиспользуемые `ValidationBehavior`, `AuthorizationBehavior`, `SuperAdminAuthorizationBehavior` (или зарегистрировать, если решено использовать исключения — но проект уже выбрал Result-pattern, поэтому удалить). | `Application/Common/Behaviors/*` |
| 2.2 | Удалить `Web/Models/FlatViewModel.cs`. | `Web/Models/` |
| 2.3 | Удалить пустую миграцию `20260730184210_UpdateChatIdToValueObject` (если она уже применена к БД — пометить как no-op, но не удалять из истории; иначе удалить). | `Infrastructure/Migrations/` |
| 2.4 | Удалить `Web/GlobalSuppressions.cs` или убрать suppression для `_FlatRowPartialModel`. | `Web/GlobalSuppressions.cs` |
| 2.5 | Удалить папку `Domain/Exception/` и `<Folder Include="Exception\" />` из `.csproj`. | `Domain/` |
| 2.6 | Удалить переопределение `SaveChangesAsync` в `HomeGeBotDbContext`, если оно только вызывает базовый. | `Infrastructure/Persistence/HomeGeBotDbContext.cs` |
| 2.7 | Вынести маппинг `Flat` → `FlatDto` в `Application/Dashboard/Mappings/FlatMappingExtensions.cs` и использовать в трёх handler'ах. | `Application/Dashboard/` |
| 2.8 | Вынести формирование полного имени (`FirstName + LastName`) в `Application/Common/Extensions/UserExtensions.cs`. | `Application/` |

**Критерии приёмки:** `dotnet build` без новых warnings; `git diff` показывает только удаления/извлечения.

---

### Фаза 3 — Избавление от статических сервисов (DIP)

**Цель:** сделать слой Bot тестируемым и соответствующим DIP.

| Шаг | Действие | Файлы | Принцип |
|-----|----------|-------|---------|
| 3.1 | Создать интерфейс `IMessageService` с методами `SendMessage`, `SendPhoto`, `EditMessage`, `EditMediaMessage`, `DeleteMessage`, `ShowAlert`, `GetMessageText`, `GetMessagePathToPhoto`, `Escape`. Реализовать в `MessageService` как нестатический Scoped-сервис. Зарегистрировать в `Bot/ConfigureServices.cs`. | `Bot/Services/MessageService.cs`, `Bot/Services/IMessageService.cs` | DIP |
| 3.2 | Создать интерфейс `IBotI18n` с методом `T(string key, string lang)`. Реализовать в `BotI18n` как Singleton. Перенести словарь в JSON-ресурсы (`wwwroot/i18n/bot.json`) для OCP. | `Bot/Services/BotI18n.cs`, `Bot/Services/IBotI18n.cs` | DIP/OCP |
| 3.3 | Во всех потребителях (`BaseMessage`, `ExceptionNotification`, `UserNotifier`, команды) заменить `MessageService.X()` на инжекцию `IMessageService`. | `Bot/**` | DIP |
| 3.4 | В `BaseMessage` заменить статический `MessageService` на инжектируемый `IMessageService` через конструктор. | `Bot/Common/Abstractions/BaseMessage.cs` | DIP |
| 3.5 | `ConnectionStringFactory` → `IConnectionStringFactory` + реализация; регистрировать в DI, убрать `null!`. | `Infrastructure/Persistence/ConnectionStringFactory.cs` | DIP |

**Критерии приёмки:** ни одного статического вызова `MessageService.`/`BotI18n.` в коде; можно написать unit-тест для любой команды с mock `IMessageService`.

---

### Фаза 4 — Декомпозиция по SRP

**Цель:** разбить классы-«боги» на мелкие специализированные.

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 4.1 | **`CommandAnalyzer`**: вынести проверку `isKicked` в отдельный `IUserStatusGuard`; вынести «перенаправление в TMA» в `IUnknownCommandHandler`; заменить `if` по `UpdateType` на словарь `IUpdateHandler`-стратегий (`MyChatMemberHandler`, `CallbackQueryHandler`, `MessageHandler`). `CommandAnalyzer` становится тонким диспетчером. | `Bot/Common/CommandAnalyzer.cs`, `Bot/Common/UpdateHandlers/*` |
| 4.2 | **`AppTextCommand`**: вынести формирование сообщения менеджеру в `IManagerNotificationFormatter` (с локализацией через `IMessageService`/`IBotI18n`); вынести форвард поста в `IPostForwarder`; команда остаётся оркестратором. | `Bot/Commands/ClientCommands/TextCommands/AppTextCommand.cs`, `Bot/Services/ManagerNotificationFormatter.cs` |
| 4.3 | **`GetAdminDashboardHandler`**: разбить на `GetApplicationsQuery`, `GetFlatsQuery`, `GetManagersQuery` (для супер-админа) и композитный `GetAdminDashboardQuery`, который агрегирует результаты. | `Application/Dashboard/` |
| 4.4 | **`TmaController`**: вынести локализацию и работу с `IBotDbContext` в MediatR-запросы (`GetTmaProfileQuery`, `GetTmaManagerQuery`, `SetUserLanguageCommand`, `SaveApplicationDraftCommand`). Контроллер становится тонким. | `Web/Controllers/TmaController.cs`, `Application/Tma/` |
| 4.5 | **`Index.cshtml.cs`**: вынести маппинг DTO→ViewModel в `IApplicationMapper`; каждый POST-обработчик уже отправляет MediatR-команду — оставить только это. | `Web/Pages/Index.cshtml.cs`, `Web/Mappings/` |
| 4.6 | **`TelegramBot`**: вынести настройку webhook в `IWebhookSetupService`; класс отвечает только за предоставление `ITelegramBotClient`. | `Bot/Common/TelegramBot.cs`, `Bot/Services/WebhookSetupService.cs` |
| 4.7 | **`AdminAuthController`**: вынести формирование claims в `IAdminClaimsFactory`; отладочный endpoint — в отдельный контроллер `DebugAuthController` под `#if DEBUG`. | `Web/Controllers/AdminAuthController.cs` |

**Критерии приёмки:** ни один класс не превышает ~80–100 строк бизнес-логики; каждый класс имеет одну причину изменения.

---

### Фаза 5 — Локализация и OCP

**Цель:** единая точка локализации, расширяемость без изменения кода.

| Шаг | Действие |
|-----|----------|
| 5.1 | Определить единый интерфейс `ILocalizationService` (Application) поверх `Messages` из БД + fallback на ресурсы. |
| 5.2 | Перенести захардкоженные строки из `SubmitRentalApplicationHandler`, `AppTextCommand`, `TmaController` в `Messages` или ресурсные файлы `.resx`. |
| 5.3 | `BotI18n` (после Фазы 3) загружает переводы из JSON/БД, а не из кода. |
| 5.4 | Добавить кэширование `GetMessageBodyQuery` (`IMemoryCache`, TTL ~5 мин) — сейчас каждый `SendMessage` делает запрос к БД. |
| 5.5 | Унифицировать поддержку языков: константа `SupportedLanguages = ["ru","en","ka"]` в одном месте; валидация в `TmaSetLanguageRequest`. |

**Критерии приёмки:** добавление нового языка не требует изменения C#-кода — только данных.

---

### Фаза 6 — Инфраструктурные улучшения

| Шаг | Действие |
|-----|----------|
| 6.1 | Добавить `IEntityTypeConfiguration<>` для каждой сущности в `Infrastructure/Persistence/Configurations/`. Убрать неявные конвенции. |
| 6.2 | `DateTimeService` → Singleton. Использовать `IDateTime` во всех handler'ах вместо `DateTime.UtcNow` (Фаза 1.3 + здесь). |
| 6.3 | `HomeGeBotDbContext`: убрать пустой конструктор; для миграций использовать `IDesignTimeDbContextFactory`. Убрать `null!`. |
| 6.4 | `AuditableEntitySaveChangesInterceptor`: вынести extension `HasChangedOwnedEntities` в отдельный файл. |
| 6.5 | `GetUserApplicationsHandler`: исправить N+1 через `Join`/`Include` или пакетный запрос имён менеджеров. |
| 6.6 | `GetUserLanguageQuery`: привести `ChatId` к `Domain.Common.ChatId` для согласованности. |
| 6.7 | `Domain.csproj`: убрать зависимость от `Microsoft.Extensions.Configuration` (Domain не должен её требовать). |
| 6.8 | `DashboardApiController`: добавить кэш фото пользователя (`IMemoryCache`, TTL ~1 час); вынести формирование URL в `TelegramPhotoService`. |
| 6.9 | `BaseCallbackCommand.Contains()`: упростить логику. |
| 6.10 | `EnumExtensions.GetDisplayName`: добавить кэш `ConcurrentDictionary<Enum, string>`. |

---

### Фаза 7 — Тестирование

**Цель:** покрыть критичные use cases после рефакторинга.

| Шаг | Действие |
|-----|----------|
| 7.1 | Создать `Tests/Domain.Tests`, `Tests/Application.Tests`, `Tests/Bot.Tests` (xUnit + Moq + `Microsoft.EntityFrameworkCore.InMemory`). |
| 7.2 | Application: unit-тесты на все handler'ы с in-memory `IBotDbContext`. Покрыть `SubmitRentalApplication` (выбор менеджера, лимит заявок), `GrantAdminRights`/`RevokeAdminRights` (перераспределение), `CloseRequest` (авторизация). |
| 7.3 | Bot: unit-тесты на `CommandAnalyzer` (маршрутизация по типу Update), `AppTextCommand` (форвард из неверного канала → ошибка), сессии. |
| 7.4 | Web: integration-тесты `Web.Tests` с `WebApplicationFactory` — `AdminAuthController` (валидная/невалидная `initData`), `TmaController` (авторизация). |
| 7.5 | Покрытие ≥ 70% на Application и Bot. |

---

## 5. Приоритеты и риски

### Приоритеты

| Приоритет | Фазы | Обоснование |
|-----------|------|-------------|
| 🔴 P0 | Фаза 1 | Безопасность (авторизация кика/закрытия) и корректность (версии пакетов). |
| 🟠 P1 | Фаза 2, Фаза 3 | Уменьшение технического долга, подготовка к тестированию. |
| 🟡 P2 | Фаза 4, Фаза 5 | Качество дизайна, расширяемость. |
| 🟢 P3 | Фаза 6, Фаза 7 | Производительность, тестовое покрытие. |

### Риски

1. **Фаза 3 (статические сервисы)** — затрагивает большинство файлов Bot. Выполнять единым PR с полным прогоном. Риск регресса в отправке сообщений.
2. **Фаза 4.1 (CommandAnalyzer)** — изменение центрального диспетчера. Обязательно покрыть integration-тестом перед рефакторингом (сначала Фаза 7.3 на текущем коде — «characterization tests»).
3. **Фаза 1.1 (ToggleUserKick)** — нужно уточнить продуктовое требование: кик должен срабатывать при `MyChatMember` (пользователь покинул бот) или это админ-операция? От этого зависит, вводить ли авторизацию или вынести в отдельный не-MediatR механизм. **Требует решения владельца продукта.**
4. **Фаза 5 (локализация)** — перенос строк в БД/ресурсы может затронуть существующие данные (`Messages`). Согласовать с продакшен-данными.
5. **Фаза 6.3 (IDesignTimeDbContextFactory)** — изменение процесса миграций; обновить README и CI.

### Рекомендации по выполнению

- Каждый шаг — отдельный коммит, каждая фаза — отдельный PR.
- Перед Фазой 4 снять «characterization tests» на текущее поведение.
- После Фазы 1 и Фазы 3 — обязательное ручное тестирование в staging.
- Не смешивать рефакторинг с новыми фичами.
