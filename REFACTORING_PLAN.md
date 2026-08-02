# План рефакторинга и отчёт код-ревью

Дата ревью: 2026-08-02  
Объект: `Property In Tbilisi bot` (.NET 10.0, Clean Architecture + CQRS + Telegram Bot)  
Критерии: принципы ООП (инкапсуляция, наследование, полиморфизм, абстракция) и SOLID (SRP, OCP, LSP, ISP, DIP).  
Состояние сборки: `dotnet build` — успешно; `dotnet test` — 40 тестов пройдены.

---

## 📑 Содержание
- [1. Сводная оценка](#1-сводная-оценка)
- [2. Нарушения по слоям](#2-нарушения-по-слоям)
  - [2.1 Domain](#21-domain)
  - [2.2 Application](#22-application)
  - [2.3 Bot](#23-bot)
  - [2.4 Web](#24-web)
  - [2.5 Infrastructure](#25-infrastructure)
  - [2.6 Tests](#26-tests)
- [3. Сквозные проблемы](#3-сквозные-проблемы)
- [4. План рефакторинга](#4-план-рефакторинга)
  - [Фаза 1 — Безопасность и архитектурные границы](#фаза-1--безопасность-и-архитектурные-границы)
  - [Фаза 2 — Устранение дублирования и мёртвого кода](#фаза-2--устранение-дублирования-и-мёртвого-кода)
  - [Фаза 3 — Инкапсуляция Domain и единство модели](#фаза-3--инкапсуляция-domain-и-единство-модели)
  - [Фаза 4 — Декомпозиция по SRP](#фаза-4--декомпозиция-по-srp)
  - [Фаза 5 — Локализация и OCP](#фаза-5--локализация-и-ocp)
  - [Фаза 6 — Производительность и качество данных](#фаза-6--производительность-и-качество-данных)
  - [Фаза 7 — Тестирование](#фаза-7--тестирование)
- [5. Приоритеты и риски](#5-приоритеты-и-риски)

---

## 1. Сводная оценка

| Принцип | Соблюдение | Комментарий |
|---------|-----------|-------------|
| **Инкапсуляция** | ⚠️ Частично | Сущности Domain — «data bags» с публичными `set`; нет инвариантов и защиты коллекций. |
| **Наследование** | ✅ Хорошо | `BaseEntity` → `BaseAuditableEntity`; `BaseTextCommand`/`BaseCallbackCommand`/`BaseMessage` — уместные абстракции. |
| **Полиморфизм** | ✅ Хорошо | MediatR handlers, pipeline behaviors, marker-интерфейсы используют полиморфизм корректно. |
| **Абстракция** | ⚠️ Частично | Хорошие порты: `IBotDbContext`, `ICommandAnalyzer`, `IBotSessionStore`, `ITmaValidationService`. Плохо: `TelegramBot` и `ConnectionStringFactory` — конкретные классы без интерфейсов. |
| **SRP** | ⚠️ Частично | `CommandAnalyzer`, `AppTextCommand`, `TmaController`, `Index.cshtml.cs` делают слишком много. |
| **OCP** | ⚠️ Частично | Pipeline behaviors и Result<T> расширяемы; локализация, типы Update и конфигурация требуют правки существующего кода. |
| **LSP** | ✅ Хорошо | Нарушений не обнаружено. |
| **ISP** | ✅ Хорошо | Интерфейсы узкие и предметные. |
| **DIP** | ⚠️ Частично | Большинство handler'ов зависят от `IBotDbContext`, но Web и Bot слой в ряде мест ссылаются на конкретные реализации (`TelegramBot`, `ClientStartMessage`). |

**Общий вердикт**: проект имеет здравую архитектуру для своего масштаба, чёткое разделение слоёв и хороший DI. Основные риски сосредоточены в **безопасности Web-слоя** (CSRF, утечка токена, webhook), **нарушении Clean Architecture в `TmaController`**, **слабой инкапсуляции Domain** и **низком тестовом покрытии**.

---

## 2. Нарушения по слоям

### 2.1 Domain

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| D1 | `Entities/Admin.cs`, `Client.cs`, `Flat.cs`, `Message.cs`, `TlgUser.cs` | Все свойства — публичные auto-properties с `set`; нет инвариантов; любой код может перевести объект в невалидное состояние. | Инкапсуляция | Средняя |
| D2 | `Entities/Admin.cs` | `public List<Client> Clients { get; set; } = new();` — публичная изменяемая коллекция, доступная извне. | Инкапсуляция | Средняя |
| D3 | `Entities/Client.cs` | `AdminChatId` — value object, а не навигационное свойство к `Admin`; связь односторонняя и не консистентна с `Admin.Clients`. | Согласованность | Средняя |
| D4 | `Entities/TlgUser.cs` | `IsAdmin` дублирует наличие записи в `Admins`; возможна рассинхронизация. | SRP/DIP | Средняя |
| D5 | `Entities/Message.cs` | Языки зашиты в свойствах `BodyEn`, `BodyKa`; добавление языка требует правки сущности. | OCP | Средняя |
| D6 | `Common/ChatId.cs` | Неявные `implicit operator` к/из `long` сводят на нет типобезопасность value object. | Абстракция | Низкая |
| D7 | `Enums/Country.cs`, `Term.cs` | Значение `Other` — UI-концерн, смешанный с доменным enum. | SRP | Низкая |

### 2.2 Application

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| A1 | `GlobalUsing.cs` | `global using Microsoft.EntityFrameworkCore;` в Application нарушает Clean Architecture. | DIP | Средняя |
| A2 | `Common/Interfaces/IDateTime.cs` | Интерфейс объявлен, реализован в Infrastructure и используется только в interceptor'е; handler'ы всё ещё берут `DateTime.UtcNow` напрямую. | DIP | Средняя |
| A3 | `RentalApplications/Queries/GetUserApplications/GetUserApplicationsHandler.cs` | N+1: подзапрос `ManagerUsername` внутри `Select` выполняется для каждой заявки. | Производительность | Средняя |
| A4 | `Dashboard/Commands/GetAdminDashboard/GetAdminDashboardHandler.cs` | Handler собирает заявки, квартиры, менеджеров, имя текущего админа и флаг супер-админа. Большой класс. | SRP | Средняя |
| A5 | `Dashboard/Mappings/FlatMappingExtensions.cs` | `ToShortDateString()` без явного формата; форматирование — UI-ответственность. | SRP | Низкая |
| A6 | `AdminManagement/Commands/GrantAdminRights/GrantAdminRightsHandler.cs` | `CreatedAt = DateTime.UtcNow` вместо `_dateTime.Now`. | DIP | Низкая |
| A7 | `Dashboard/Commands/DeleteFlat/DeleteFlatHandler.cs`, `UpdateFlatComment/UpdateFlatCommentHandler.cs`, `GetAdminDashboard/GetAdminDashboardHandler.cs` | Дублирование маппинга `Flat` → `FlatDto` (есть `FlatMappingExtensions`, но используется только в UpdateFlatComment). | DRY | Низкая |
| A8 | `Messages/Queries/GetMessageBody/GetMessageBodyHandler.cs`, `GetMessagePathToPhoto` и др. | Отсутствуют FluentValidation-валидаторы для query. | Безопасность/корректность | Средняя |
| A9 | `RentalApplications/Commands/SubmitRentalApplication/SubmitRentalApplicationHandler.cs` | Лимит `MaxApplicationsPerUser = 5` и сообщения об ошибках зашиты в код; не конфигурируются. | OCP/SRP | Низкая |
| A10 | `Common/Results/Result.cs` + Result-классы | Во всех `*Result` есть неиспользуемые `ErrorMessage` и `Failure(...)`, так как ошибки хранятся в `Result<T>`. | YAGNI | Низкая |
| A11 | `AdminManagement/Queries/CheckAdminStatus/CheckAdminStatusQuery.cs`, `TlgUsers/Queries/CheckUserStatus`, `Users/Queries/GetUserLanguage` | Query возвращают `bool`/`string` вместо `Result<T>`; несогласованно с остальным слоем. | Согласованность | Низкая |

### 2.3 Bot

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| B1 | `Common/CommandAnalyzer.cs` | Нарушение SRP: маршрутизация, проверка `IsUserKicked`, логирование, обработка ошибок, fallback к `ClientStartMessage` в одном классе. | SRP | Высокая |
| B2 | `Common/CommandAnalyzer.cs` | Жёсткий `switch` по `UpdateType` и зависимость от конкретного `ClientStartMessage`. | OCP/DIP | Средняя |
| B3 | `Commands/ClientCommands/TextCommands/AppTextCommand.cs` | 156 строк: валидация канала, отправка заявки, форвард, формирование сообщения менеджеру, уведомление клиента, очистка сессии. | SRP | Высокая |
| B4 | `Commands/GeneralCommands/TextCommands/StartTextCommand.cs` | Инжекция трёх конкретных сообщений вместо фабрики/стратегии. | DIP | Средняя |
| B5 | `Services/BotI18n.cs` | Хотя сервис зарегистрирован в DI, переводы зашиты в статический словарь; добавление языка требует правки кода. | OCP | Средняя |
| B6 | `Services/ManagerNotificationFormatter.cs` | `petsDisplay` реализован тернарным оператором на 3 языка прямо в коде. | OCP/SRP | Средняя |
| B7 | `Common/Abstractions/BaseMessage.cs` | Локализация + отправка + редактирование в одном классе; дублирует вызовы `GetUserLanguageQuery`. | SRP | Средняя |
| B8 | `Common/TelegramBot.cs` | Синглтон без интерфейса; смешивает создание клиента, ленивую инициализацию и вызов webhook setup. | SRP/DIP | Средняя |
| B9 | `Services/UserNotifier.cs` | Зависит от конкретного `TelegramBot` вместо интерфейса. | DIP | Средняя |
| B10 | `Session/BotSession.cs`, `RentalApplicationDraft.cs` | Публичные `set`; нет валидации обязательных полей. | Инкапсуляция | Низкая |

### 2.4 Web

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| W1 | `Controllers/TmaController.cs` | Прямое использование `IBotDbContext` (`_dbContext`) вместо Application use cases; бизнес-логика локализации и обновления языка в контроллере. | Clean Architecture/SRP/DIP | Высокая |
| W2 | `Controllers/TmaController.cs`, `Login.cshtml.cs`, `AdminAuthController.cs` | `[IgnoreAntiforgeryToken]` отключает CSRF-защиту на критических эндпоинтах. | Безопасность | Высокая |
| W3 | `Controllers/TmaController.cs` | Возвращает анонимные объекты и содержит switch с захардкоженными строками локализации. | OCP/Согласованность | Средняя |
| W4 | `Controllers/DashboardApiController.cs` | Возвращает URL вида `https://api.telegram.org/file/bot{Token}/...`, раскрывая токен бота клиенту. | Безопасность | Высокая |
| W5 | `Controllers/TelegramBotController.cs` | Если `Webhook.SecretToken` не задан, валидация webhook пропускается (в том числе в production). | Безопасность | Высокая |
| W6 | `Controllers/DashboardApiController.cs` | Зависит от конкретного `TelegramBot`; прямые вызовы Telegram API без retry/circuit breaker. | DIP/Надёжность | Средняя |
| W7 | `Pages/Index.cshtml.cs` | 5 POST-обработчиков + ручной маппинг DTO→ViewModel + логика «Other» для enum; перегружен. | SRP | Средняя |
| W8 | `Pages/Index.cshtml.cs` | `GetCurrentAdminChatId` возвращает `0` при отсутствии claim, что может привести к неочевидным ошибкам. | Корректность | Средняя |
| W9 | `Pages/Index.cshtml.cs`, `Login.cshtml` | `@Html.Raw(JsonSerializer.Serialize(...))` с пользовательскими данными потенциально позволяет XSS. | Безопасность | Средняя |
| W10 | `Services/IAdminClaimsFactory.cs` | Роль захардкожена строкой `"Admin"` вместо использования `AdminRole`. | Согласованность | Низкая |

### 2.5 Infrastructure

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| I1 | `Persistence/ConnectionStringFactory.cs` | Статический класс без абстракции; нельзя подменить в тестах. | DIP | Средняя |
| I2 | `Persistence/ConnectionStringFactory.cs` | `Trust Server Certificate=true` и `SSL Mode=Require` захардкожены. | Безопасность/Конфигурируемость | Средняя |
| I3 | `Persistence/HomeGeBotDbContext.cs` | `OnConfiguring` вызывает `ConnectionStringFactory.GetConnectionString(null!)` для design-time; используется `null!`. | Корректность | Низкая |
| I4 | `Persistence/HomeGeBotDbContext.cs` | Нет классов `IEntityTypeConfiguration<>`; конфигурация сущностей неявная. | Читаемость/поддержка | Низкая |
| I5 | `Migrations/20260730184210_UpdateChatIdToValueObject.cs` | Пустая миграция (пустые `Up`/`Down`). | Корректность | Низкая |
| I6 | `Migrations/20260502000000_SeedMessages.cs` | Seeding текстов в миграции; сложно поддерживать и переводить. | Поддержка | Низкая |
| I7 | `ConfigureServices.cs` | Namespace `Microsoft.Extensions.DependencyInjection` «захватывает» чужое пространство имён. | Стиль | Низкая |

### 2.6 Tests

| # | Файл | Проблема | Принцип | Серьёзность |
|---|------|----------|---------|-------------|
| T1 | `Application.Tests/UnitTest1.cs`, `Bot.Tests/UnitTest1.cs` | Пустые placeholder-тесты. | YAGNI | Низкая |
| T2 | Весь test suite | Покрытие ~5%: нет тестов на Web, Infrastructure, Domain, validators, behaviors, большинство handlers и Bot commands. | Качество | Высокая |
| T3 | `Application.Tests/RentalApplications/SubmitRentalApplicationHandlerTests.cs` | `TestDbContext` определён inline; дублируется потенциально. | DRY | Низкая |

---

## 3. Сквозные проблемы

1. **Web-слой обходит Application**. `TmaController` напрямую читает и пишет через `IBotDbContext`, смешивая бизнес-логику (локализация, валидация) с HTTP-инфраструктурой. Это главное архитектурное нарушение Clean Architecture.

2. **Безопасность Web API**. Отключён antiforgery на аутентификации и TMA-эндпоинтах; webhook не требует `SecretToken` в production; endpoint фото пользователя возвращает URL с токеном бота.

3. **Слабая инкапсуляция Domain**. Сущности — наборы публичных свойств без инвариантов, защиты коллекций и единой модели связей. Это перекладывает ответственность за целостность данных на Application и Infrastructure.

4. **Локализация размазана по слоям**. Часть строк в JS (`tma-i18n.js`), часть в БД (`Messages`), часть в коде C# (`ApplicationMessages`, `ManagerNotificationFormatter`, `TmaController`). Добавление языка требует правок во многих местах.

5. **Низкое тестовое покрытие**. Покрыты только небольшие утилиты и один handler. Критическая бизнес-логика (администрирование, заявки, webhook, TMA) не покрыта.

6. **Неконсистентные API handler'ов**. Часть query возвращает `Result<T>`, часть — примитивы (`bool`, `string`). Result-классы содержат мёртвые члены `ErrorMessage`/`Failure`, так как ошибки обрабатываются в `Result<T>`.

---

## 4. План рефакторинга

### Фаза 1 — Безопасность и архитектурные границы

1. **Убрать прямой доступ `TmaController` к `IBotDbContext`**.
   - Создать use cases в Application: `SetUserLanguage`, `GetManagerContact`, `GetUserApplications`, `SubmitTmaApplication` (или переиспользовать существующие).
   - `TmaController` должен вызывать только `_mediator.Send(...)`.

2. **Закрыть CSRF-уязвимости**.
   - Для TMA-эндпоинтов antiforgery неприменим напрямую (WebView не несёт cookie-токена), поэтому заменить `[IgnoreAntiforgeryToken]` на проверку `initData` Telegram + подписи на каждом изменяющем запросе. Уже есть `ITmaValidationService` — нужно убедиться, что он вызывается до любой записи.
   - Для Razor Pages-аутентификации реализовать защиту через anti-forgery token, если это технически возможно, или вынести login-flow в API с initData.

3. **Webhook**.
   - В `TelegramBotController.ValidateWebhookRequest` при отсутствии `SecretToken` возвращать `false` в production; пропускать только в Development.

4. **Убрать утечку токена бота**.
   - В `DashboardApiController.GetUserPhoto` не возвращать URL с токеном; либо проксировать файл через сервер (`Stream`), либо генерировать временный подписанный URL.

5. **XSS в Razor**.
   - Заменить `@Html.Raw(JsonSerializer.Serialize(...))` на `Json.Serialize` с JavaScript-экранированием или сериализацию в `data-*` атрибуты через `@Json.Serialize(...)` (Razor экранирует по умолчанию).

### Фаза 2 — Устранение дублирования и мёртвого кода

1. **Удалить пустые `UnitTest1.cs`**.
2. **Убрать неиспользуемые `ErrorMessage` и `Failure(...)` из Result-классов** (оставить только фабрики Success).
3. **Унифицировать возвращаемые типы**: все query должны возвращать `Result<T>`.
4. **Вынести `TestDbContext`** в общую test-infrastructure.
5. **Удалить `null!` и параметрless конструктор в `HomeGeBotDbContext`** или пометить его `[Obsolete("EF Core design-time")]`.

### Фаза 3 — Инкапсуляция Domain и единство модели

1. **Добавить инварианты и защиту полей**.
   - Сделать сеттеры приватными, добавить конструкторы и методы изменения состояния (`MarkAsCompleted`, `AssignManager`, `Deactivate` и т.п.).
   - Защитить `Admin.Clients` через `IReadOnlyCollection` и методы `AssignClient`/`RemoveClient`.

2. **Унифицить связь Admin–Client**.
   - В `Client` добавить навигационное свойство `Admin? Admin` и убрать дублирование через `AdminChatId`, либо оставить `AdminChatId` как foreign key, но явно сконфигурировать.

3. **Убрать дублирование `TlgUser.IsAdmin` / `Admins`**.
   - Единственный источник прав — таблица `Admins` (`IsActive` + `Role`). Флаг `IsAdmin` в `TlgUser` можно удалить или синхронизировать через методы домена.

4. **Вынести `Other` из enum**.
   - Добавить boolean-флаг или отдельный value object (`CountryInput` / `TermInput`), чтобы enum оставался чистым доменным понятием.

5. **Усилить `ChatId`**.
   - Убрать неявные преобразования, заменить на явные методы `FromLong`/`ToLong`.

### Фаза 4 — Декомпозиция по SRP

1. **Рефакторинг `CommandAnalyzer`**.
   - Вынести стратегии обработки UpdateType в `IUpdateHandler` / `IUpdateHandler<T>`.
   - Вынести проверку `IsUserKicked` в отдельный behavior/pipeline или сервис.
   - Fallback к TMA сделать через `IStartMessageResolver`, а не хардкод `ClientStartMessage`.

2. **Рефакторинг `AppTextCommand`**.
   - Разделить на: валидатор форварда, сервис извлечения данных из сообщения, сервис подачи заявки, сервис уведомления менеджера, сервис уведомления клиента.

3. **Рефакторинг `BaseMessage`**.
   - Разделить получение языка/текста (с кэшированием) и отправку сообщения.
   - Убрать дублирование вызовов `GetUserLanguageQuery`.

4. **Рефакторинг `StartTextCommand`**.
   - Внедрить `IStartMessageFactory`, которая по роли возвращает нужный `BaseMessage`.

5. **Разделить `TelegramBot` и `WebhookSetupService` на интерфейсы**.
   - `ITelegramBotClientProvider` / `IBotClientFactory` + `IWebhookSetupService`.

### Фаза 5 — Локализация и OCP

1. **Единый источник переводов**.
   - Вынести все TMA-переводы в JSON-ресурсы (ASP.NET Core IStringLocalizer / PO-файлы).
   - В Bot-слое Button-тексты (`BotI18n`) тоже загружать из ресурсов/БД.
   - `ApplicationMessages` заменить на lookup по ключу с fallback на русский.

2. **Шаблоны сообщений менеджера**.
   - Заменить `Replace("{country}", ...)` на шаблонизатор (например, `string.Format` с именованными параметрами или Scriban), чтобы формат не был в коде.

3. **Расширяемый выбор DisplayName**.
   - Вынести отображение `Country.Other`/`Term.Other` в сервис форматирования, а не в enum/entity.

### Фаза 6 — Производительность и качество данных

1. **Исправить N+1 в `GetUserApplicationsHandler`**.
   - Загрузить маппинг `AdminChatId → Username` в `Dictionary` одним запросом, как сделано в `GetAdminDashboardHandler`.

2. **Оптимизировать `SubmitRentalApplicationHandler`**.
   - Запросы username клиента и менеджера можно выполнить одним `Where(...Contains(...)).ToDictionaryAsync`.

3. **Использовать `IDateTime` везде**.
   - Заменить `DateTime.UtcNow` в handler'ах на `_dateTime.Now`.
   - В UI-форматировании передавать `DateTimeOffset` или форматировать в Presenter/ViewModel.

4. **Добавить `IEntityTypeConfiguration<>`**.
   - Перенести ограничения БД (индексы, required-поля, точность) из конвенций и миграций в явные конфигурации Infrastructure.

5. **Починить пустую миграцию `UpdateChatIdToValueObject`**.
   - Проверить, что value object корректно маппится; при необходимости сгенерировать новую миграцию.

### Фаза 7 — Тестирование

1. **Удалить placeholder'ы**.
2. **Добавить `Domain.Tests`** — проверка инвариантов, `ChatId`, enum formatting.
3. **Добавить `Infrastructure.Tests`** — `ConnectionStringFactory`, `DateTimeService`, interceptor аудита.
4. **Расширить `Application.Tests`**:
   - Тесты для всех validators.
   - Тесты для pipeline behaviors (validation, admin auth, super-admin auth).
   - Тесты для всех handlers, особенно администрирования и dashboard.
5. **Добавить `Web.Tests`** с `WebApplicationFactory`:
   - Интеграционные тесты TMA-эндпоинтов (initData validation).
   - Тесты webhook endpoint и авторизации.
6. **Добавить тесты Bot-слоя**:
   - `CommandAnalyzer` с моками команд.
   - `AppTextCommand`, `StartTextCommand` — проверка отправки нужных сообщений.

---

## 5. Приоритеты и риски

### 🔴 Критично (нужно сделать в первую очередь)
- **W4** — утечка токена бота в URL фото.
- **W2** — отключён antiforgery на аутентификации/TMA.
- **W5** — webhook пропускает запросы без `SecretToken`.
- **W1** — `TmaController` работает напрямую с `IBotDbContext`.
- **T2** — критически низкое тестовое покрытие.

### 🟠 Высокий приоритет
- **B1/B3** — декомпозиция `CommandAnalyzer` и `AppTextCommand`.
- **D1–D4** — инкапсуляция сущностей и устранение дублирования `IsAdmin`.
- **A3** — N+1 в `GetUserApplicationsHandler`.
- **I2** — захардкоженные SSL-настройки БД.

### 🟡 Средний приоритет
- **A2** — использование `IDateTime` в handler'ах.
- **B4/B5/B6** — фабрика сообщений, внешняя локализация, OCP локализации.
- **A6, A9, A10** — мёртвый код и константы в коде.
- **W7, W8** — упрощение `Index.cshtml.cs` и обработка ошибок claims.

### 🟢 Низкий приоритет
- **A1, A11, I5, I7** — стилевые и мелкие архитектурные шероховатости.
- **T1, T3** — удаление placeholder'ов и вынесение `TestDbContext`.

### ⚠️ Риски
- **Переписывание Domain** может потребовать новой миграции и синхронизации данных.
- **Ужесточение webhook** может сломать локальную разработку, если не предусмотреть Development-режим.
- **Перенос логики из `TmaController` в Application** затронет контракты frontend'а; нужны regression-тесты.

---

*Документ составлен на основе актуального состояния репозитория и заменяет предыдущие устаревшие замечания (в частности, `MessageService` и `BotI18n` уже переведены в DI, а `CloseRequestRequest` корректно реализует `IAdminCommand`).*
