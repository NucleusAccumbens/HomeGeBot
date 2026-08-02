# Архитектурный аудит проекта Property In Tbilisi Bot

Дата аудита: 2026-08-02
Целевая платформа: .NET 10.0
Тип приложения: ASP.NET Core веб-приложение + Telegram-бот (webhook) с админ-панелью и Telegram Mini App (TMA).
Состояние сборки: `dotnet build` — 0 ошибок, 0 предупреждений. `dotnet test` — 42/42 тестов пройдено (33 `Application.Tests`, 9 `Bot.Tests`).

Документ описывает **текущее** состояние кодовой базы по файлам и слоям. Приоритизированный план действий — в [`REFACTORING_PLAN.md`](../REFACTORING_PLAN.md).

---

## 1. Стек и архитектура

| Слой / аспект | Технологии |
|---------------|------------|
| Платформа | .NET 10.0, ASP.NET Core, Razor Pages, MVC-контроллеры |
| Архитектура | Clean Architecture, CQRS (MediatR 12.0), Result Pattern |
| Валидация | FluentValidation 11.5 |
| БД | PostgreSQL, EF Core 10.0, Npgsql 10.0 |
| Бот | Telegram.Bot 18.0 (webhook) |
| Веб-UI | Razor Pages, Telegram WebApp SDK, JavaScript |
| Сериализация | Newtonsoft.Json |
| API-документация | Swashbuckle Swagger (Development) |
| Кэш / сессии | `IDistributedCache` (сессии бота, TTL 1 день), `IMemoryCache` (кэш локализованных сообщений) |

Направление зависимостей: `Web` → `Bot`/`Infrastructure` → `Application` → `Domain`. Domain не имеет внешних зависимостей, кроме `System.ComponentModel.DataAnnotations` (атрибут `Display` для локализации enum'ов).

---

## 2. Пофайловый обзор

### 2.1 Domain

| Файл | Назначение | Замечания |
|------|------------|-----------|
| `Common/BaseEntity.cs` | Базовый класс с `Id` | `Id` имеет `public set` — сущность может быть переприсвоена извне после создания. |
| `Common/BaseAuditableEntity.cs` | `CreatedAt`/`LastModified` | Публичные сеттеры; фактически заполняются только через `AuditableEntitySaveChangesInterceptor`, но ничего не мешает произвольному коду в Application изменить их напрямую. |
| `Common/ChatId.cs` | Value Object для Telegram chat id (`record struct`) | Неявные преобразования в/из `long`, `IComparable`. Обеспечивает базовую типобезопасность в сигнатурах, но неявные операторы позволяют случайно передать произвольный `long` без валидации. |
| `Common/EnumExtensions.cs` | `DisplayAttribute → localized name` через рефлексию с кэшированием | Приемлемо, но привязывает Domain к `System.ComponentModel.DataAnnotations`. |
| `Entities/Admin.cs` | Администратор/менеджер | Методы `Activate/Deactivate/AssignClient/RemoveClient` инкапсулируют переходы состояния. `Role` имеет `public set` — можно обойти доменную логику. |
| `Entities/Client.cs` | Заявка клиента | Методы `ChangeManager/Complete/UpdateDetails`. Конструктор принимает 8 параметров. `UpdateDetails` обновляет не все изменяемые поля (частичное покрытие). Нет валидации «`CountryOther` обязателен, если `Country == Other`» на уровне сущности. |
| `Entities/Flat.cs` | Объект недвижимости | `UpdateComment`/`UpdateDetails`; все поля nullable без валидации формата (`Link`, `OwnerNumber`). |
| `Entities/Message.cs` | Локализуемый шаблон сообщения | Поля `Body`/`BodyEn`/`BodyKa` — жёстко заданный набор языков в самой сущности (см. D5 в плане рефакторинга). |
| `Entities/TlgUser.cs` | Пользователь Telegram | `UpdateProfile/SetLanguage/SetKicked`; `Language` по умолчанию `"ru"`, входные данные не валидируются (например, произвольный код языка можно установить). |
| `Enums/AdminRole.cs` | `Admin`, `SuperAdmin` | Без замечаний. |
| `Enums/Country.cs`, `Enums/Term.cs` | Enum'ы с `DisplayAttribute` | Значение `Other` в доменном enum — UI-концерн (нужен для варианта «свой вариант» в форме), формально смешивает домен и представление. |

### 2.2 Application

Слой построен на MediatR (команды/запросы в отдельных папках-фичах: `AdminManagement`, `BotStart`, `Dashboard`, `Messages`, `RentalApplications`, `TlgUsers`, `Users`), каждый use case — `Request/Command` + `Handler` + (опционально) `Validator` + `Result`.

| Компонент | Замечания |
|-----------|-----------|
| `Common/Behaviors/ResultValidationBehavior.cs` | Централизованная FluentValidation-валидация в pipeline. Работает только для запросов, для которых зарегистрирован `IValidator<T>` — часть query (`GetMessageBody`, `GetUserLanguage`, `CheckAdminStatus`) валидаторов не имеет. |
| `Common/Behaviors/ResultAuthorizationBehavior.cs` / `ResultSuperAdminAuthorizationBehavior.cs` | Дублирующие друг друга по структуре behavior'ы для `IAdminCommand`/`ISuperAdminCommand`. Разумно объединить через общий базовый класс/стратегию с параметром роли. |
| `Common/Interfaces/IDateTime.cs` | Используется **только** в `AuditableEntitySaveChangesInterceptor` (Infrastructure). Ни один Application-хендлер не обращается к `IDateTime` напрямую — там, где нужна текущая дата (например, фильтрация по времени), это пока не требовалось, но абстракция уже готова для будущего использования. |
| `Common/Localization/TmaLabelProvider.cs`, `ApplicationMessages.cs` | Переводы захардкожены как `switch`/словари внутри классов — добавление языка требует правки кода (OCP). В `TmaLabelProvider` для русского языка используются значения по умолчанию из `TmaSubmitApplicationLabels`, а не отдельная ветка словаря. |
| `Dashboard/Commands/GetAdminDashboard/GetAdminDashboardHandler.cs` | Самый крупный handler в проекте (~175 строк): в одном методе выполняется авторизация, выборка Clients/Flats/Admins и маппинг в 2 похожих DTO-блока. Кандидат на декомпозицию по CQRS (`GetApplicationsQuery`, `GetManagersQuery` и т.д.). |
| `AdminManagement/Commands/RevokeAdminRights/RevokeAdminRightsHandler.cs` | Помимо отзыва прав, содержит логику перераспределения клиентов уволенного менеджера между оставшимися активными менеджерами — стоит вынести в отдельный доменный/прикладной сервис, т.к. похожая логика выбора «менее загруженного менеджера» дублируется в `SubmitRentalApplicationHandler`. |
| `AdminManagement/Queries/CheckAdminStatus/CheckAdminStatusHandler.cs`, `TlgUsers/Queries/CheckUserStatus/CheckUserStatusHandler.cs`, `Users/Queries/GetUserLanguage/GetUserLanguageHandler.cs` | Возвращают примитивы (`bool`/`string`) вместо `Result<T>` — единообразие CQRS-паттерна в проекте нарушено (часть query следует Result Pattern, часть — нет). |
| Локализация сообщений в handler'ах | Часть сообщений об ошибках берётся из `ApplicationMessages` (локализовано), часть — захардкожена на русском прямо в handler'ах/валидаторах (`DeleteFlatHandler`, `ReassignClientHandler`, `SetUserLanguageHandler` и др.). Непоследовательно. |
| `RentalApplications/Queries/GetUserApplications`, `Users/Queries/GetManagerContact`, `Users/Queries/GetUserLanguage` | Запросы не помечены `IAdminCommand`/`ISuperAdminCommand` — доступ к данным ограничивается только тем, что вызывающий код (TMA-контроллеры) всегда подставляет `ChatId` из проверенного `initData` текущего пользователя. Сам use case не имеет встроенной защиты «пользователь может запросить только свои данные», это ответственность вызывающей стороны — стоит явно задокументировать или добавить проверку на уровне handler'а. |
| `AdminManagement/Dtos/BotUserDto.cs` | Свойство `IsAdmin` в `GetBotUsersHandler` всегда устанавливается в `false` — фактически неиспользуемое/вводящее в заблуждение поле. |
| `AdminManagement/Commands/CloseRequest/CloseRequestRequest.cs` | Свойство `ClientChatId` объявлено, но не используется в `CloseRequestHandler`. |
| Именование | `ToggleUserKickCommand`/`ToggleUserKickRequestValidator` — единственное место, где команда называется `...Command`, а не `...Request`, при этом валидатор всё равно `...RequestValidator`. |

### 2.3 Infrastructure

| Файл | Назначение | Замечания |
|------|------------|-----------|
| `ConfigureServices.cs` | Регистрация DI | Строка подключения получается через прямое `new ConnectionStringProvider(configuration)` внутри метода регистрации сервисов, а не через DI-контейнер — рабочий, но нечистый паттерн (смешивает конфигурацию контейнера и runtime-логику). Также сам класс называется `ConfigureService` (в единственном числе), что расходится с остальными проектами (`Application.ConfigureServices`, `Bot.ConfigureServices` — тоже в ед. числе, то есть на самом деле единообразно в рамках проекта, но не по общепринятой .NET-конвенции). |
| `Persistence/HomeGeBotDbContext.cs` | EF Core `DbContext` | Реализует `IBotDbContext`; приватный `ChatIdConverter` для `ChatId`; `AuditableEntitySaveChangesInterceptor` подключается опционально через конструктор. Пустой конструктор для design-time не документирован. |
| `Persistence/ConnectionStringProvider.cs` | Возвращает строку подключения (приоритет `DATABASE_URL`, иначе `appsettings`) | При отсутствии обоих источников возвращает `string.Empty` вместо явного исключения — ошибка конфигурации будет обнаружена только при первом обращении к БД с непонятным сообщением от Npgsql. |
| `Persistence/ConnectionStringFactory.cs` | Парсинг `DATABASE_URL` (Heroku/Railway-формат) в Npgsql connection string | `catch (Exception)` слишком широкий; `uri.UserInfo.Split(':')` не проверяется на длину (упадёт с `IndexOutOfRangeException`, если пароль отсутствует); SSL-параметры (`SSL Mode=Require;Trust Server Certificate=true`) захардкожены. |
| `Persistence/DesignTimeDbContextFactory.cs` | Фабрика для `dotnet ef` | Дублирует логику получения `DATABASE_URL` из `ConnectionStringProvider`; при отсутствии — пустая строка, миграции упадут с неинформативной ошибкой. |
| `Persistence/Configurations/*Configuration.cs` (Admin, Client, Flat, Message, TlgUser) | `IEntityTypeConfiguration<>` | **Все 5 файлов** содержат только `ToTable(...)` и `HasKey(...)`. Отсутствуют: индексы (`ChatId`, `AdminId`, `Message.Name`), ограничения длины строк, явные конфигурации связей и cascade-поведения (сейчас cascade delete для `Client → Admin` задан миграцией `UnifyClientAdminRelationship`, но не отражён в `IEntityTypeConfiguration`, то есть источник истины для схемы — миграция, а не конфигурация, что противоречит EF Core best practices). |
| `Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs` | Автозаполнение `CreatedAt`/`LastModified` через `IDateTime` | Реализован корректно (sync/async), использует `EntityEntryExtensions` для owned-типов. |
| `Services/DateTimeService.cs` | Реализация `IDateTime` через `DateTime.UtcNow` | Без замечаний. |
| `Migrations/` (14 файлов + snapshot) | История схемы | `20260730184210_UpdateChatIdToValueObject.cs` — **пустая миграция** (`Up`/`Down` без тела), оставшийся артефакт после рефакторинга `ChatId`; безопасна для применения, но не несёт смысла и создаёт путаницу в истории. `20260501180626_Init.cs` создаёт таблицу `HasPets`, немедленно удаляемую следующей миграцией — признак незафиксированной на момент разработки модели, актуальному состоянию БД не мешает. |

### 2.4 Bot

| Файл / область | Замечания |
|------|-----------|
| `Common/TelegramBot.cs` | В одном файле объявлены `ITelegramBotClientProvider`/`TelegramBot`, а также `IWebhookSetupService`/`WebhookSetupService` — нарушение принятого в проекте соглашения «один интерфейс/класс — один файл» (см. `Common/Interfaces/*`). `SemaphoreSlim` в `TelegramBot` не освобождается (класс не реализует `IDisposable`). |
| `Common/CommandAnalyzer.cs` | Диспетчер обновлений: определяет тип Update, проверяет статус пользователя, маршрутизирует в `IUpdateHandler`. Ловит исключения на верхнем уровне, логирует и уведомляет админов через `IExceptionNotification` — неплохой единый error boundary, но совмещает в одном классе роутинг + обработку ошибок. |
| `Common/Abstractions/BaseMessage.cs` | Методы `SendMessage`, `SendMessageWithPhoto` и т.п. каждый раз independently запрашивают язык пользователя через `IMediator.Send(GetUserLanguageQuery)` — если метод вызывает несколько операций подряд, язык запрашивается повторно вместо переиспользования результата. |
| `Services/BotI18n.cs` | Переводы UI-текстов бота захардкожены в статическом `Dictionary` внутри класса — добавление языка требует изменения кода и пересборки; потенциальный конкурентный доступ к статическому полю не защищён (на практике `Dictionary` только читается после инициализации, поэтому в реальности не проблема, но неявно). |
| `Services/RentalApplicationForwardProcessor.cs` | Основной сценарий «клиент переслал пост → создание заявки»: 6 внедрённых зависимостей, обработка форварда, формирование уведомления менеджеру, создание клавиатуры, очистка сессии — несколько ответственностей в одном сервисе. |
| `Session/BotSession.cs`, `RentalApplicationDraft.cs` | POCO с полностью открытыми `public` сеттерами и без валидации — состояние многошаговой формы можно перевести в противоречивое состояние из любого места кода, имеющего ссылку на объект. |
| `Session/MemoryBotSessionStore.cs` | Реализует `IBotSessionStore`, но **не зарегистрирован в DI** (`Bot/ConfigureServices.cs` регистрирует только `DistributedBotSessionStore`) — на практике мёртвый код, оставшийся, вероятно, как альтернативная реализация для локальной разработки без Redis/`IDistributedCache`. |
| `ConfigureServices.cs` | Метод `AddCallbackCommands` не регистрирует ни одной реализации `BaseCallbackCommand` — на данный момент в проекте нет callback-команд (только `dCancel`, обрабатываемый иначе), поэтому `CallbackCommandRouter` существует, но не имеет зарегистрированных обработчиков. Это не баг (пока нет callback-команд), но стоит иметь в виду при добавлении новых inline-кнопок. |
| Локализация ошибок | `SessionExpiredException`, `ExceptionNotification` содержат захардкоженные русскоязычные строки — не проходят через `IBotI18n`. |

### 2.5 Web

| Файл / область | Замечания |
|------|-----------|
| `Program.cs` | DI, middleware pipeline, cookie-аутентификация (`HttpOnly`, `SecurePolicy = Always` в Production, `SameSite = Lax`). Собран по стандартным для ASP.NET Core 10 конвенциям, замечаний нет. |
| `Controllers/TelegramBotController.cs` | Webhook-эндпоинт делегирует в `ICommandAnalyzer` — не обращается к БД напрямую. Хорошее соответствие Clean Architecture. |
| `Controllers/TmaController.cs` | Все методы защищены `[ValidateTmaInitData]`. `SubmitApplication` формирует `BotSession`/`RentalApplicationDraft` и сохраняет их напрямую через `IBotSessionStore`, а также сам вызывает `IUserNotifier.SendNotificationAsync` — часть бизнес-логики (какой менеджер уведомляется, в каком формате) фактически находится в Web-контроллере, а не в Application/Bot. К БД напрямую не обращается (использует MediatR для остальных операций). |
| `Controllers/AdminAuthController.cs` | Эндпоинт `auth-debug` доступен только при `IsDevelopment()`, но не выполняет проверку `initData` — предназначен исключительно для локальной отладки без Telegram-клиента; важно, чтобы `ASPNETCORE_ENVIRONMENT` в production никогда не был `Development`. Контроллер помечен `[IgnoreAntiforgeryToken]` — CSRF-защита для admin-auth не задействована (частично компенсируется тем, что аутентификация опирается на HMAC-подпись `initData`, а не на cookie/форму). |
| `Services/TmaInitDataValidator.cs` | Реализация HMAC-SHA256 проверки `initData` соответствует официальной спецификации Telegram (secret_key = HMAC-SHA256("WebAppData", bot_token); hash = HMAC-SHA256(secret_key, data_check_string)) — корректна. Проверка `auth_date` (защита от replay-атак повторного использования старого `initData`) отсутствует. |
| `Middleware/ValidateTelegramWebhookMiddleware.cs` | В Production при отсутствии настроенного `SecretToken` запрос отклоняется (401) — корректно. В Development при отсутствии `SecretToken` валидация пропускается с предупреждением в лог — осознанное упрощение для локальной разработки без публичного HTTPS-домена. |
| `Pages/Index.cshtml.cs` | PageModel дашборда админки (~150 строк): `OnGet` + 4 POST-обработчика (`GrantAdmin`, `RevokeAdmin`, `Reassign`, `CloseRequest`). Каждый обработчик тонкий (делегирует в MediatR), но сама модель страницы отвечает за все административные действия дашборда — кандидат на разделение по вкладкам/партиалам с собственными handler'ами. |
| `Filters/ValidateTmaInitDataAttribute.cs` | Достаёт свойство `InitData` из тела запроса через рефлексию по имени свойства — работает, но неявно требует от каждого DTO конкретного имени свойства; при рефакторинге DTO легко сломать без ошибки компиляции. |
| XSS/HTML | `@Html.Raw` с пользовательскими данными не встречается ни в одном `.cshtml`; клиентский код (`tma-utils.js`) последовательно применяет `escapeHtml` перед вставкой пользовательских строк в DOM. |
| `wwwroot/appsettings.example.json` | Содержит только плейсхолдеры (`YOUR_BOT_TOKEN`, `RANDOM_SECURE_STRING`) — реальных секретов в репозитории нет. |

### 2.6 Tests

| Проект | Покрытие |
|--------|----------|
| `Tests/Application.Tests` | 33 теста: `UserExtensions`, `ApplicationMessages`, `SupportedLanguages`, `FlatMappingExtensions`, `SubmitRentalApplicationHandler`, `SetUserLanguageHandler`, `GetManagerContactHandler`. Использует `TestDbContext` (in-memory) из `Tests/Application.Tests/Common`. |
| `Tests/Bot.Tests` | 9 тестов: `BotI18n`. |
| Не покрыты тестами | `Domain` (инварианты сущностей, `ChatId`), `Infrastructure` (`ConnectionStringFactory`, `AuditableEntitySaveChangesInterceptor`), большая часть `Application` (behaviors, остальные handler'ы), весь `Bot` кроме `BotI18n` (`CommandAnalyzer`, роутеры, `RentalApplicationForwardProcessor`), весь `Web` (контроллеры, `TmaInitDataValidator`, middleware). |

---

## 3. Итоговая оценка по слоям

| Слой | Состояние |
|------|-----------|
| Domain | Инкапсуляция от умеренной до хорошей (методы для переходов состояния есть), но базовые классы (`BaseEntity`, `BaseAuditableEntity`) и часть свойств (`Admin.Role`) оставляют публичные сеттеры. Нет валидации доменных инвариантов (Country/Term "Other"). |
| Application | Чистая CQRS-структура, Result Pattern, pipeline behaviors для валидации/авторизации. Технический долг: непоследовательные возвращаемые типы у query, дублирование поведений авторизации, смешение локализованных и захардкоженных сообщений об ошибках, один разросшийся handler (`GetAdminDashboardHandler`). |
| Infrastructure | DbContext и interceptor реализованы аккуратно. Главный пробел — минимальные `IEntityTypeConfiguration<>` (нет индексов/ограничений) и хрупкая обработка отсутствующей строки подключения. |
| Bot | Хорошее разделение на роутеры/хендлеры/сообщения через интерфейсы. Есть смешение интерфейс+реализация в одном файле, нехватка диспозинга `SemaphoreSlim`/`TelegramBotClient`, захардкоженная статическая локализация, неиспользуемый `MemoryBotSessionStore`. |
| Web | Хорошая изоляция от БД (весь доступ к данным — через MediatR), корректная HMAC-проверка `initData`, отсутствие `@Html.Raw`-уязвимостей. Технический долг сосредоточен в `Index.cshtml.cs` (слишком много обязанностей) и частичной бизнес-логике в `TmaController.SubmitApplication`. |
| Tests | Низкое покрытие вне `Application.Tests`/`Bot.Tests`; отсутствуют тесты для Domain, Infrastructure, Web. |

Конкретные рекомендации и приоритеты — см. [`REFACTORING_PLAN.md`](../REFACTORING_PLAN.md).
