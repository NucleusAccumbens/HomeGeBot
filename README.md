# Property In Tbilisi Bot & Management System

Комплексное решение для приёма заявок на аренду недвижимости в Тбилиси: Telegram-бот с интеграцией Telegram Mini App (TMA) для ввода данных и защищённая админ-панель на Razor Pages.

---

## 📑 Содержание
- [Возможности](#-возможности)
- [Технологический стек](#-технологический-стек)
- [Архитектура](#-архитектура)
- [Функциональность](#-функциональность)
- [Структура проекта](#-структура-проекта)
- [Конфигурация](#-конфигурация-appsettingsjson)
- [Запуск в production](#-запуск-в-production)
- [Управление и администрирование](#-управление-и-администрирование)
- [Документация](#-документация)

---

## 🚀 Возможности

### 📱 Пользовательский опыт (Telegram Bot + Mini App)
- **Гибридный поток**: пользователь заполняет анкету в современном Telegram Mini App (TMA), а бот отвечает за коммуникацию и форвард постов объектов.
- **Мастер заявки**: 4-шаговая адаптивная веб-форма внутри Telegram:
  1. Страна происхождения
  2. Профессия / род деятельности
  3. Наличие домашних животных
  4. Планируемый срок аренды
- **Привязка объектов**: пользователь пересылает посты прямо из исходного Telegram-канала, чтобы привязать их к своей заявке.
- **Автоматическое назначение менеджера**: заявка закрепляется за активным менеджером с наименьшей текущей нагрузкой.
- **Мультиязычность**: интерфейс TMA и сообщения бота поддерживают `ru`, `en`, `ka`.

### 🔐 Админ-панель
- **Безопасный доступ**: панель открывается как Telegram Mini App (кнопка «Перейти в админку» в боте) и авторизуется через подписанные `initData` Telegram (HMAC-SHA256). Пароль не требуется.
- **Управление заявками**: просмотр всех входящих заявок и закреплённых менеджеров, закрытие заявок, переназначение клиентов.
- **Управление объявлениями**: добавление комментариев к объектам и удаление устаревших записей.
- **Обзор менеджеров**: мониторинг загрузки менеджеров (количество закреплённых клиентов).
- **Управление пользователями**: повышение пользователей бота до администраторов и отзыв прав.
- **Профиль менеджера**: просмотр собственного профиля и фото.

### 🛠 Технические характеристики
- **Clean Architecture**: слои `Domain`, `Application`, `Infrastructure`, `Bot`, `Web` с явным направлением зависимостей.
- **CQRS**: MediatR разделяет команды и запросы; pipeline behaviors обеспечивают валидацию и авторизацию.
- **Типобезопасность**: `ChatId` как `record struct` (Value Object) с неявными преобразованиями к `long`.
- **Безопасность**: криптографическая валидация `initData` Telegram, HTML-санитизация пользовательского ввода, cookie-аутентификация с `HttpOnly` и `Secure`.
- **Локализация**: тексты сообщений хранятся в БД (`Messages` с полями `Body`, `BodyEn`, `BodyKa`); переводы UI — в JS-модуле `tma-i18n.js`.
- **Сессии**: `IDistributedCache`/`IMemoryCache` хранят состояние многошаговой формы (TTL = 1 день).

---

## 🛠 Технологический стек

| Слой | Технологии |
|------|-----------|
| **Платформа** | .NET 10.0, ASP.NET Core |
| **Архитектура** | Clean Architecture, CQRS (MediatR 12.0) |
| **Валидация** | FluentValidation 11.5 |
| **База данных** | PostgreSQL + EF Core 10.0 (Npgsql 10.0) |
| **Бот** | Telegram.Bot 18.0 (Webhook mode) |
| **Веб-UI** | Razor Pages, JavaScript (Telegram WebApp SDK) |
| **Сериализация** | Newtonsoft.Json |
| **Документация API** | Swashbuckle Swagger |
| **Кэш** | `IDistributedCache` (in-memory), `IMemoryCache` |

---

## 🏗 Архитектура

### Слои и направление зависимостей

```
┌─────────────────────────────────────────────────────────┐
│  Web  (Razor Pages, API Controllers, Middleware, TMA)   │
└───────────────┬───────────────────────────┬─────────────┘
                │                           │
        ┌───────▼───────┐           ┌────────▼────────┐
        │  Bot          │           │  Infrastructure │
        │  (Telegram)   │           │  (EF Core, PG)  │
        └───────┬───────┘           └────────┬────────┘
                │                            │
                └────────────┬───────────────┘
                             │
                     ┌───────▼───────┐
                     │  Application  │
                     │  (MediatR,    │
                     │   CQRS,       │
                     │   Validators) │
                     └───────┬───────┘
                             │
                     ┌───────▼───────┐
                     │   Domain      │
                     │ (Entities,    │
                     │  Value Objects│
                     │  Enums)       │
                     └───────────────┘
```

- **Domain** — чистые сущности, value objects, enum'ы. Нет внешних зависимостей (только `Microsoft.Extensions.Configuration`).
- **Application** — use cases как MediatR `IRequest`/`IRequestHandler`, FluentValidation-валидаторы, pipeline behaviors (`ResultValidationBehavior`, `ResultAuthorizationBehavior`, `ResultSuperAdminAuthorizationBehavior`), Result-pattern, интерфейсы `IBotDbContext`, `IDateTime`, `IUserNotifier`.
- **Infrastructure** — `HomeGeBotDbContext` (PostgreSQL), `AuditableEntitySaveChangesInterceptor`, `ConnectionStringFactory` (поддержка `DATABASE_URL`), миграции EF Core, `DateTimeService`.
- **Bot** — `TelegramBot` (webhook), `CommandAnalyzer` (диспетчер Update), `BaseTextCommand`/`BaseCallbackCommand`, сообщения (`BaseMessage`), сессии (`IBotSessionStore`), статические сервисы `MessageService`/`BotI18n`.
- **Web** — `Program.cs` (DI, middleware), контроллеры (`TelegramBotController`, `AdminAuthController`, `TmaController`, `DashboardApiController`), Razor Pages (`Index`, `Login`, `Tma`), `TmaValidationService`, `GlobalExceptionMiddleware`, `BotInitializationService` (IHostedService).

### Паттерны
- **CQRS + MediatR**: каждый use case — отдельный `IRequest` с собственным handler'ом и опциональным validator'ом.
- **Result-pattern**: `Result<T>` с `Success`/`Failure` для предсказуемой обработки ошибок без исключений.
- **Pipeline behaviors**: валидация и авторизация выполняются централизованно до handler'а.
- **Marker interfaces**: `IAdminCommand` / `ISuperAdminCommand` включают соответствующий authorization behavior.
- **Value Object**: `ChatId` (record struct) защищает от путаницы между ID чатов и другими long-значениями.

### Поток обработки Telegram Update
1. Telegram отправляет POST на `/api/message/update` → `TelegramBotController`.
2. Контроллер (опционально) проверяет `Webhook.SecretToken` и делегирует в `ICommandAnalyzer.AnalyzeCommandsAsync`.
3. `CommandAnalyzer` определяет тип Update (`MyChatMember`, `CallbackQuery`, `Message`), проверяет статус пользователя (`CheckUserStatusQuery`), маршрутизирует в зарегистрированные `BaseTextCommand`/`BaseCallbackCommand` по имени или шагу сессии.
4. Команды отправляют MediatR-запросы в Application, где срабатывают validation/authorization behaviors, затем handler выполняет бизнес-логику через `IBotDbContext`.

### Поток авторизации админ-панели
1. Админ открывает панель через WebApp-кнопку бота → `Login.cshtml` читает `Telegram.WebApp.initData`.
2. JS отправляет `POST /api/admin/auth` с `initData` → `AdminAuthController`.
3. `TmaValidationService` проверяет HMAC-SHA256 подпись по спецификации Telegram.
4. Через MediatR `CheckAdminStatusQuery` проверяется, что chat id — активная запись в `Admins`.
5. При успехе выдаётся cookie `AdminAuth` с claim `ChatId`; при открытии вне Telegram показывается сообщение об ошибке.

---

## 📋 Функциональность

### Команды бота
| Команда | Тип | Назначение |
|---------|-----|-----------|
| `/start` | Text | Регистрирует/обновляет пользователя, отправляет стартовое сообщение по роли (Client/Manager/Admin/SuperAdmin) |
| `app` (форвард поста) | Text + Session step `WaitForFlatForward` | Принимает заявку, назначает менеджера, форвардит пост менеджеру, уведомляет клиента |
| `dCancel` | Callback | Отмена текущей заявки (очистка сессии) |

### Use cases (Application)
- **BotStart**: `StartBot` — регистрация пользователя при `/start`.
- **RentalApplications**: `SubmitRentalApplication` (с лимитом активных заявок и авто-выбором менеджера), `GetUserApplications`.
- **AdminManagement**: `CheckAdminStatus`, `GetBotUsers`, `GrantAdminRights`, `RevokeAdminRights`, `ReassignClient`, `CloseRequest`.
- **Dashboard**: `GetAdminDashboard`, `DeleteFlat`, `UpdateFlatComment`.
- **TlgUsers**: `ToggleUserKick` (блокировка при покидании бота), `CheckUserStatus`.
- **Users**: `GetUserLanguage`.
- **Messages**: `GetMessageBody`, `GetMessagePathToPhoto` (локализация из БД).

### TMA API (`/api/tma/*`)
- `POST /submit-application` — сохранение черновика заявки в сессию.
- `GET /applications` — список заявок пользователя.
- `GET /manager` — контакт супер-админа.
- `GET /profile` — профиль текущего пользователя.
- `POST /set-language` — смена языка интерфейса.

### Dashboard API
- `GET /api/dashboard/user-photo` — прокси фото пользователя из Telegram API.
- `POST /api/admin/auth` — авторизация по `initData`.
- `GET /api/admin/auth-debug` — отладочная авторизация (только Development).

---

## 📁 Структура проекта

```
Property In Tbilisi bot/
├── Domain/                      # Сущности, value objects, enum'ы
│   ├── Common/                  # BaseEntity, BaseAuditableEntity, ChatId, EnumExtensions
│   ├── Entities/                # Admin, Client, Flat, Message, TlgUser
│   ├── Enums/                   # AdminRole, Country, Term
│   └── Exception/               # (зарезервировано, пусто)
├── Application/                 # CQRS use cases
│   ├── Common/                  # Interfaces, Results, Authorization, Behaviors
│   ├── AdminManagement/         # Commands + Queries для админ-панели
│   ├── BotStart/                # Команда /start
│   ├── Dashboard/               # Данные дашборда
│   ├── Messages/                # Локализация сообщений
│   ├── RentalApplications/      # Подача и просмотр заявок
│   ├── TlgUsers/                # Управление пользователями
│   └── Users/                   # Языковые настройки
├── Infrastructure/              # EF Core, PostgreSQL, миграции
│   ├── Persistence/             # DbContext, ConnectionStringFactory, Interceptors
│   ├── Services/                # DateTimeService
│   └── Migrations/              # 11 миграций + snapshot
├── Bot/                         # Логика Telegram-бота
│   ├── Common/                  # TelegramBot, CommandAnalyzer, Abstractions
│   ├── Commands/                # General/Client text & callback commands
│   ├── Configuration/           # TelegramBot/Webhook/Bot/AdminNotification configs
│   ├── Messages/                # Client/General сообщения (BaseMessage)
│   ├── Services/                # MessageService, BotI18n, UserNotifier, ExceptionNotification
│   ├── Session/                 # BotSession, IBotSessionStore + реализации
│   └── Exceptions/              # SessionExpired, Token, Url
├── Web/                         # Веб-слой
│   ├── Controllers/             # TelegramBot, AdminAuth, Tma, DashboardApi
│   ├── Middleware/              # GlobalExceptionMiddleware
│   ├── Models/                  # ViewModels + Validators
│   ├── Pages/                   # Razor Pages: Index, Login, Tma + Shared partials
│   ├── Services/                # TmaValidationService, BotInitializationService
│   └── wwwroot/                 # js (tma-*, dashboard, login, common), css, img
├── HomeGeBot.sln
└── README.md
```

---

## ⚙️ Конфигурация (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN"
  },
  "Webhook": {
    "BaseUrl": "https://your-domain.com/",
    "SecretToken": "RANDOM_SECURE_STRING"
  },
  "AdminNotifications": {
    "ChatIds": [12345678]
  },
  "Bot": {
    "SourceChannelId": -100...,
    "AdminPanelUrl": "https://your-domain.com/",
    "WebAppUrl": "https://your-domain.com/tma"
  }
}
```

- **`Webhook.SecretToken`**: обязателен в production. Гарантирует, что обновления приходят только от Telegram.
- **`AdminNotifications.ChatIds`**: список Telegram ID, получающих системные уведомления об ошибках.
- **`Bot.SourceChannelId`**: ID канала, из которого пользователи должны пересылать посты.
- **`DATABASE_URL`**: поддерживается для облачных провайдеров (Heroku/Railway) — парсится в строку подключения Npgsql.

---

## 🚢 Запуск в production

### 1. База данных
Установите PostgreSQL. Приложение поддерживает как стандартную строку подключения (`ConnectionStrings:DefaultConnection`), так и переменную окружения `DATABASE_URL`.

### 2. Публикация
```bash
dotnet publish Web/Web.csproj -c Release -o ./publish
```

### 3. Миграции
```bash
dotnet ef database update --project Infrastructure --startup-project Web
```

### 4. Webhook
При старте `BotInitializationService` автоматически устанавливает webhook на `{Webhook.BaseUrl}api/message/update`. Домен должен иметь валидный SSL-сертификат (Telegram требует HTTPS).

### 5. Локальная разработка
```bash
dotnet run --project Web
```
В Development-режиме доступны Swagger (`/swagger`) и отладочная авторизация `/api/admin/auth-debug?chatId=...`.

---

## 👥 Управление и администрирование

### Авторизация
Админ-панель открывается как Telegram Mini App (кнопка `AdminPanelUrl`/WebApp, отправляемая ботом админам). При загрузке `Web/Pages/Login.cshtml` читает `Telegram.WebApp.initData`, отправляет его на `POST /api/admin/auth`, где проверяется HMAC-подпись Telegram и наличие chat id в таблице `Admins` с `IsActive = true`. При успехе выдаётся cookie `AdminAuth` с claim реального `ChatId`. Открытие панели вне Telegram покажет «Откройте панель через кнопку в Telegram-боте».

### Назначение первого админа
Система безопасна по умолчанию — первого админа нужно повысить вручную через SQL:
1. Узнайте свой `ChatId` (например, через @userinfobot).
2. Запустите бота один раз (`/start`), чтобы создать запись в `TlgUsers`.
3. Выполните SQL:
   ```sql
   UPDATE "TlgUsers" SET "IsAdmin" = true WHERE "ChatId" = YOUR_ID;
   INSERT INTO "Admins" ("ChatId", "IsActive", "CreatedAt", "Role")
   VALUES (YOUR_ID, true, NOW(), 1); -- Role 1 = SuperAdmin
   ```

### Управление менеджерами
После назначения первого **SuperAdmin**:
1. Войдите в Web-дашборд.
2. Откройте вкладку **«Users»**.
3. Найдите пользователя → **«Сделать админом»** (повышение) или **«Отозвать права»** (понижение).
4. При отзыве прав активные заявки менеджера автоматически перераспределяются.

### Поток уведомлений
При подаче заявки:
1. Система выбирает активного менеджера с наименьшим числом закреплённых клиентов.
2. Менеджер получает Telegram-сообщение с деталями клиента и кнопкой «Написать» для начала диалога.
3. Клиенту приходит подтверждение с кнопкой WebApp для просмотра статуса.

---

## 🩺 Актуальный аудит и план рефакторинга (2026-08-02)

Последнее комплексное ревью показало, что проект успешно собирается (`dotnet build`) и все существующие тесты проходят (`dotnet test` — 40 тестов). Архитектура в целом здорова, однако выявлен ряд проблем, которые отражены в [`REFACTORING_PLAN.md`](./REFACTORING_PLAN.md) и [`Application/ARCHITECTURE_AUDIT.md`](./Application/ARCHITECTURE_AUDIT.md):

- **Безопасность**: `TmaController` и `Login.cshtml.cs` отключают antiforgery; `DashboardApiController` возвращает URL с токеном бота; webhook пропускает запросы без `SecretToken`.
- **Архитектура**: `TmaController` напрямую использует `IBotDbContext`, обходя слой Application.
- **OOP/SOLID**: сущности Domain не инкапсулируют инварианты; `ChatId` имеет неявные преобразования, сводящие на нет типобезопасность; `CommandAnalyzer` и `AppTextCommand` берут на себя слишком много ответственности.
- **Производительность**: N+1-запрос в `GetUserApplicationsHandler`.
- **Тесты**: покрытие низкое (~5%), присутствуют пустые `UnitTest1.cs`.

Ключевые рекомендации: убрать прямую работу Web с `IBotDbContext`, добавить Application use-cases для TMA, закрыть CSRF-уязвимости, вынести локализацию из контроллеров/команд, усилить инкапсуляцию сущностей и расширить тестовое покрытие.

---

## 📚 Документация

- [`REFACTORING_PLAN.md`](./REFACTORING_PLAN.md) — актуальный отчёт код-ревью на соответствие OOP/SOLID и пошаговый план рефакторинга.
- [`Application/ARCHITECTURE_AUDIT.md`](./Application/ARCHITECTURE_AUDIT.md) — проектный архитектурный аудит и план рефакторинга (Domain, Infrastructure, Application, Bot, Web, Tests, OOP/SOLID).
