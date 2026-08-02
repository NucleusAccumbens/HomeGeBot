# План рефакторинга и отчёт код-ревью

Дата ревью: 2026-08-02
Объект: `Property In Tbilisi bot` (.NET 10.0, Clean Architecture + CQRS + Telegram Bot + Telegram Mini App)
Критерии: принципы ООП и SOLID.
Состояние сборки: `dotnet build` — 0 ошибок, 0 предупреждений. `dotnet test` — 42/42 тестов пройдено (33 в `Application.Tests`, 9 в `Bot.Tests`).

Пофайловый разбор каждого слоя — в [`Application/ARCHITECTURE_AUDIT.md`](./Application/ARCHITECTURE_AUDIT.md). Этот документ описывает только **текущее** состояние и план дальнейших действий.

---

## 1. Сводная оценка по принципам

| Принцип | Соблюдение | Комментарий |
|---------|-----------|-------------|
| **Инкапсуляция** | ⚠️ Частично | Сущности (`Admin`, `Client`, `Flat`, `TlgUser`) имеют доменные методы (`ChangeManager`, `Complete`, `UpdateProfile` и т.д.), но `BaseEntity.Id`, `BaseAuditableEntity.CreatedAt/LastModified`, `Admin.Role` — с публичными сеттерами. `Session/BotSession.cs`, `RentalApplicationDraft.cs` — полностью открытые POCO без валидации. |
| **Наследование** | ✅ Хорошо | `BaseEntity → BaseAuditableEntity → {Admin, Client, Flat, Message, TlgUser}` — согласованная иерархия без нарушений. |
| **Полиморфизм** | ✅ Хорошо | `IUpdateHandler`-стратегии (`MessageUpdateHandler`, `CallbackQueryUpdateHandler`, `MyChatMemberUpdateHandler`), MediatR pipeline behaviors, `BaseTextCommand`/`BaseCallbackCommand`. |
| **Абстракция** | ✅ Хорошо | `ITelegramBotClientProvider`, `IUpdateHandler`, `ITextCommandRouter`/`ICallbackCommandRouter`, `IRentalApplicationForwardProcessor`, `ITmaLabelProvider`, `IConnectionStringProvider`, `IBotDbContext`, `ITmaInitDataParser`/`ITmaInitDataValidator`. |
| **SRP** | ⚠️ Частично | `GetAdminDashboardHandler` (~175 строк), `Pages/Index.cshtml.cs` (дашборд с 4 POST-обработчиками), `Bot/Common/TelegramBot.cs` (клиент + webhook-сервис в одном файле), `RentalApplicationForwardProcessor` — каждый совмещает несколько обязанностей. |
| **OCP** | ⚠️ Частично | Pipeline behaviors и `IUpdateHandler` расширяемы. Локализация (`BotI18n`, `ApplicationMessages`, `TmaLabelProvider`, `Message.BodyEn/BodyKa`) и `Other` в enum'ах `Country`/`Term` требуют правки кода при расширении. |
| **LSP** | ✅ Хорошо | Нарушений не обнаружено. |
| **ISP** | ✅ Хорошо | Интерфейсы в проекте узкие и сфокусированные (`IDateTime`, `IUserNotifier`, `IAdminClaimsFactory` и т.д.). |
| **DIP** | ⚠️ Частично | Web и Bot в целом зависят от абстракций Application. Исключения: `Infrastructure/ConfigureServices.cs` создаёт `ConnectionStringProvider` через `new` вместо резолва из контейнера; `TmaController.SubmitApplication` формирует `BotSession`/`RentalApplicationDraft` и напрямую вызывает `IUserNotifier`, то есть часть бизнес-логики находится в Web, а не в Application. |

**Общий вердикт**: проект собирается и проходит все существующие тесты, архитектура в целом соответствует заявленной Clean Architecture (обращение к БД из Web полностью отсутствует, кроме одного места с session/notifier логикой в `TmaController`). Основной технический долг — расширяемость локализации, минимальные EF Core конфигурации, несколько «разросшихся» классов и низкое тестовое покрытие вне Application/Bot.

---

## 2. Обнаруженные проблемы по слоям (приоритизировано)

### 2.1 Domain

| ID | Проблема | Приоритет |
|----|----------|-----------|
| D1 | `BaseEntity.Id`, `BaseAuditableEntity.CreatedAt/LastModified` имеют публичные сеттеры — инвариант «Id/аудит не меняются извне» ничем не защищён. | Средний |
| D2 | `Admin.Role` имеет публичный сеттер вместо метода `ChangeRole(...)`, как у остальных изменяемых полей `Admin`. | Низкий |
| D3 | `Message.BodyEn/BodyKa` — фиксированный набор языков в сущности; добавление нового языка требует новых полей + миграции (OCP). | Средний |
| D4 | `Country`/`Term` содержат значение `Other`, которое является UI-концерном (нужно для варианта «свой вариант»), а не доменным понятием. | Низкий |
| D5 | Нет валидации на уровне сущности/валидатора «`CountryOther`/`TermOther` обязательны, если выбрано `Other`». | Средний |

### 2.2 Application

| ID | Проблема | Приоритет |
|----|----------|-----------|
| A1 | `GetAdminDashboardHandler` — самый большой handler в проекте, совмещает авторизацию, 3 выборки данных и дублирующийся маппинг. | Высокий |
| A2 | `ResultAuthorizationBehavior` и `ResultSuperAdminAuthorizationBehavior` дублируют структуру — стоит объединить в один behavior с параметром требуемой роли. | Средний |
| A3 | Часть query возвращает примитивы (`bool`, `string`) вместо `Result<T>`: `CheckAdminStatusHandler`, `CheckUserStatusHandler`, `GetUserLanguageHandler`. Нарушает единообразие CQRS-паттерна, принятого в проекте. | Средний |
| A4 | Нет FluentValidation-валидаторов для части query (`GetMessageBodyQuery`, `GetUserLanguageQuery`, `CheckAdminStatusQuery`). | Низкий |
| A5 | Смешение локализованных (`ApplicationMessages`) и захардкоженных русскоязычных сообщений об ошибках в handler'ах/валидаторах (`DeleteFlatHandler`, `ReassignClientHandler`, `SetUserLanguageHandler`, `RevokeAdminRightsHandler` и др.). | Средний |
| A6 | Логика «выбрать активного менеджера с наименьшей загрузкой» дублируется между `SubmitRentalApplicationHandler` и перераспределением в `RevokeAdminRightsHandler`. | Средний |
| A7 | `TmaLabelProvider`/`ApplicationMessages`/`BotI18n` (Bot) — три независимых, дублирующих подход механизма локализации, требующих правки кода для нового языка (OCP). | Средний |
| A8 | Мелкая уборка: неиспользуемое свойство `CloseRequestRequest.ClientChatId`, всегда-`false` `BotUserDto.IsAdmin`, несогласованное именование `ToggleUserKickCommand`/`...RequestValidator`. | Низкий |

### 2.3 Infrastructure

| ID | Проблема | Приоритет |
|----|----------|-----------|
| I1 | `IEntityTypeConfiguration<>` для `Admin`, `Client`, `Flat`, `Message`, `TlgUser` содержат только `ToTable`/`HasKey` — нет индексов (`ChatId`, `AdminId`, `Message.Name`), ограничений длины строк, явной конфигурации связей и cascade-поведения. | Высокий |
| I2 | `ConnectionStringProvider` возвращает `string.Empty` при отсутствии и `DATABASE_URL`, и `DefaultConnection` вместо явного исключения — ошибка конфигурации проявится поздно и неинформативно. | Средний |
| I3 | `ConnectionStringFactory`: широкий `catch (Exception)`, отсутствие проверки длины `UserInfo.Split(':')`, захардкоженные SSL-параметры. | Средний |
| I4 | `Infrastructure/ConfigureServices.cs` создаёт `ConnectionStringProvider` через `new` внутри метода регистрации DI вместо резолва зарегистрированного сервиса. | Низкий |
| I5 | Пустая миграция `20260730184210_UpdateChatIdToValueObject.cs` — не выполняет никаких изменений схемы, засоряет историю миграций. | Низкий |

### 2.4 Bot

| ID | Проблема | Приоритет |
|----|----------|-----------|
| B1 | `MemoryBotSessionStore` реализует `IBotSessionStore`, но нигде не регистрируется в DI (используется только `DistributedBotSessionStore`) — мёртвый код. | Низкий |
| B2 | `BotI18n` — переводы UI-текстов бота захардкожены в статическом `Dictionary`; добавление языка требует изменения кода. | Средний |
| B3 | `TelegramBot.cs` объявляет `ITelegramBotClientProvider`/`TelegramBot` и `IWebhookSetupService`/`WebhookSetupService` в одном файле — нарушает принятое в проекте соглашение «интерфейс в `Common/Interfaces`, реализация отдельно». | Низкий |
| B4 | `TelegramBot` не освобождает `SemaphoreSlim`/`TelegramBotClient` (класс не реализует `IDisposable`). | Низкий |
| B5 | `BaseMessage` запрашивает язык пользователя (`GetUserLanguageQuery`) отдельно в каждом методе вместо переиспользования уже полученного значения при последовательных вызовах. | Низкий |
| B6 | `RentalApplicationForwardProcessor` совмещает обработку форварда, формирование уведомления менеджеру, создание клавиатуры и очистку сессии — кандидат на декомпозицию. | Средний |
| B7 | Исключения (`SessionExpiredException`) и уведомления об ошибках (`ExceptionNotification`) содержат захардкоженный русский текст в обход `IBotI18n`. | Низкий |

### 2.5 Web

| ID | Проблема | Приоритет |
|----|----------|-----------|
| W1 | `TmaController.SubmitApplication` формирует `BotSession`/`RentalApplicationDraft` и вызывает `IUserNotifier` напрямую — часть бизнес-логики подачи заявки находится в Web-контроллере, а не в Application/Bot use case. | Высокий |
| W2 | `Pages/Index.cshtml.cs` — PageModel дашборда с `OnGet` + 4 POST-обработчиками (`GrantAdmin`, `RevokeAdmin`, `Reassign`, `CloseRequest`); стоит разделить по вкладкам/партиалам или вынести обработчики в отдельные small page handlers. | Средний |
| W3 | `TmaInitDataValidator` не проверяет `auth_date` из `initData` — нет защиты от повторного использования устаревшего (скомпрометированного) `initData` (replay). | Средний |
| W4 | `AdminAuthController` помечен `[IgnoreAntiforgeryToken]` — CSRF-защита для admin-auth эндпоинта отключена (частично компенсируется HMAC-проверкой `initData`, но не полностью, т.к. `auth-debug` в Development её не выполняет). | Низкий |
| W5 | `ValidateTmaInitDataAttribute` достаёт `InitData` из DTO через рефлексию по имени свойства — хрупко к рефакторингу DTO без ошибки компиляции. | Низкий |
| W6 | `DashboardApiController` не кэширует фото пользователей, каждый показ дашборда — новый запрос к Telegram API. | Низкий |

### 2.6 Тесты

| ID | Проблема | Приоритет |
|----|----------|-----------|
| T1 | Нет тестов для `Domain` (инварианты сущностей, `ChatId`, enum'ы). | Высокий |
| T2 | Нет тестов для `Infrastructure` (`ConnectionStringFactory`, `AuditableEntitySaveChangesInterceptor`). | Средний |
| T3 | Нет тестов для `Web` (`TmaInitDataValidator`, middleware, контроллеры) — особенно критично для HMAC-валидации и webhook-мидлвара. | Высокий |
| T4 | Покрытие `Bot` ограничено `BotI18n`; нет тестов для `CommandAnalyzer`, роутеров, `RentalApplicationForwardProcessor`. | Средний |
| T5 | Покрытие `Application` частичное — нет тестов для большинства pipeline behaviors и половины handler'ов. | Средний |

---

## 3. Рекомендуемый план работ

### Итерация 1 — Безопасность и границы слоёв (высокий приоритет)
1. Вынести логику `TmaController.SubmitApplication` (W1) в Application use case (например, `SubmitRentalApplicationDraftCommand`), Web-контроллер должен только вызывать MediatR.
2. Добавить проверку `auth_date` в `TmaInitDataValidator` (W3) с настраиваемым окном (например, 24 часа).
3. Написать тесты на `TmaInitDataValidator` и `ValidateTelegramWebhookMiddleware` (T3).

### Итерация 2 — EF Core конфигурации (высокий приоритет)
1. Наполнить `AdminConfiguration`, `ClientConfiguration`, `FlatConfiguration`, `MessageConfiguration`, `TlgUserConfiguration` (I1): индексы на `ChatId`/`AdminId`/`Message.Name`, ограничения длины строк, явное описание связей и cascade-поведения (перенести туда то, что сейчас задано только миграцией `UnifyClientAdminRelationship`).
2. Заменить `string.Empty`-фолбэк в `ConnectionStringProvider`/`DesignTimeDbContextFactory` на явное исключение с понятным сообщением (I2).
3. Удалить пустую миграцию `UpdateChatIdToValueObject` или зафиксировать причину её пустоты в комментарии.

### Итерация 3 — Декомпозиция крупных классов (средний приоритет)
1. Разбить `GetAdminDashboardHandler` (A1) на отдельные query по разделам дашборда (заявки/менеджеры/объекты).
2. Разделить `Pages/Index.cshtml.cs` (W2) на отдельные partial-handler'ы или страницы по вкладкам дашборда.
3. Вынести логику подбора менеджера с наименьшей загрузкой (A6) в общий сервис (`IManagerAssignmentService`), используемый и `SubmitRentalApplicationHandler`, и `RevokeAdminRightsHandler`.
4. Декомпозировать `RentalApplicationForwardProcessor` (B6): вынести формирование уведомления и клавиатуры в отдельные сервисы.

### Итерация 4 — Расширяемая локализация (средний приоритет)
1. Выбрать единый подход к локализации сообщений (например, ресурсные файлы `.resx` или таблица переводов в БД) и постепенно перевести на него `ApplicationMessages`, `TmaLabelProvider`, `BotI18n`, `Message.BodyEn/BodyKa`.
2. Объединить `ResultAuthorizationBehavior`/`ResultSuperAdminAuthorizationBehavior` (A2) в один параметризуемый behavior.
3. Привести query к единому паттерну возврата `Result<T>` (A3): `CheckAdminStatusHandler`, `CheckUserStatusHandler`, `GetUserLanguageHandler`.

### Итерация 5 — Расширение тестов (высокий приоритет, идёт параллельно)
1. `Domain.Tests`: `ChatId`, инварианты `Client`/`Admin`/`Flat`, enum'ы (T1).
2. `Infrastructure.Tests`: `ConnectionStringFactory`, `AuditableEntitySaveChangesInterceptor` (T2).
3. `Web.Tests` через `WebApplicationFactory`: TMA-эндпоинты, webhook, авторизация admin-панели (T3).
4. `Bot.Tests`: `CommandAnalyzer`, `TextCommandRouter`/`CallbackCommandRouter`, `RentalApplicationForwardProcessor` (T4).

### Итерация 6 — Мелкая уборка (низкий приоритет)
1. Убрать публичные сеттеры `BaseEntity.Id`, `BaseAuditableEntity.CreatedAt/LastModified`, `Admin.Role` (D1, D2).
2. Удалить неиспользуемый `MemoryBotSessionStore` либо задокументировать его назначение как альтернативы для локальной разработки без `IDistributedCache` (B1).
3. Убрать неиспользуемое свойство `CloseRequestRequest.ClientChatId`, поле `BotUserDto.IsAdmin` (A8).
4. Переименовать `ToggleUserKickCommand` → `ToggleUserKickRequest` для единообразия (A8).
5. Реализовать `IDisposable` в `TelegramBot` (B4); разнести интерфейсы/реализации `TelegramBot.cs` по файлам согласно принятому в проекте соглашению (B3).
6. Добавить валидацию `CountryOther`/`TermOther` при `Country`/`Term == Other` на уровне валидатора `SubmitRentalApplicationRequestValidator` и/или сущности `Client` (D5).

---

## 4. Как применять миграции

```bash
dotnet ef database update --project Infrastructure --startup-project Web
```

Перед применением рекомендуется сделать резервную копию БД, так как часть миграций выполняет необратимые изменения данных (например, удаление колонок).
