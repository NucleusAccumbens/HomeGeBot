# Аудит слоя Application

## Дата: 2026-08-02
## Версия: Актуальное состояние после перехода на MediatR

---

## 1. Структура проекта

```
Application/
├── GlobalUsing.cs                    # Глобальные using (3 пространства + EF Core)
├── ConfigureServices.cs              # DI-регистрация MediatR + FluentValidation
├── Application.csproj                # Зависимости
│
├── Common/
│   ├── Authorization/                 # Маркерные интерфейсы
│   │   ├── IAdminCommand.cs           # Контракт AdminChatId
│   │   └── ISuperAdminCommand.cs      # Контракт SuperAdminChatId
│   ├── Behaviors/                     # Pipeline MediatR (зарегистрировано 3 behavior)
│   │   ├── ResultValidationBehavior.cs
│   │   ├── ResultAuthorizationBehavior.cs
│   │   └── ResultSuperAdminAuthorizationBehavior.cs
│   ├── Results/
│   │   └── Result.cs                  # Result Pattern
│   ├── Interfaces/
│   │   ├── IBotDbContext.cs           # Порт для persistence
│   │   ├── IDateTime.cs               # Абстракция системного времени
│   │   └── IUserNotifier.cs           # Порт для уведомлений
│   ├── Extensions/
│   │   └── UserExtensions.cs          # Форматирование ФИО
│   └── Localization/
│       ├── ApplicationMessages.cs     # Жёстко зашитые строки ошибок
│       └── SupportedLanguages.cs      # Справочник поддерживаемых языков
│
├── AdminManagement/                   # Управление администраторами
├── BotStart/                          # Обработка /start
├── Dashboard/                         # Данные админ-панели
├── Messages/                          # Локализованные сообщения из БД
├── RentalApplications/                # Заявки на аренду
├── TlgUsers/                          # Пользователи Telegram
└── Users/                             # Языковые настройки
```

---

## 2. Пофайловый анализ

### 2.1 GlobalUsing.cs
```csharp
global using Application.Common.Authorization;
global using Application.Common.Interfaces;
global using Domain.Entities;
global using Microsoft.EntityFrameworkCore;
```
**Оценка:** ⚠️ `Microsoft.EntityFrameworkCore` в Application нарушает чистую архитектуру. EF Core — инфраструктурная зависимость. Её использование здесь обусловлено только `IQueryable`/`DbSet` в `IBotDbContext` и LINQ-запросах в handler'ах. Рекомендуется перенести EF-зависимость в Infrastructure и в Application работать с repository/спецификациями.

### 2.2 ConfigureServices.cs
- Регистрирует MediatR через `RegisterServicesFromAssembly`.
- Подключает 3 Result-oriented pipeline behaviors.
- Регистрирует FluentValidation-валидаторы из текущей сборки.

**Оценка:** ✅ Корректно. Отсутствуют только non-Result версии behaviors (которые описывались в старых версиях аудита), так как весь слой Application теперь работает через `Result<T>`.

### 2.3 Common/Behaviors

#### ResultValidationBehavior<TRequest, TResult>
- Работает с `IRequest<Result<TResult>>`.
- Собирает все `IValidator<TRequest>` и возвращает `Result<TResult>.Failure` при ошибках.
- Без reflection.

#### ResultAuthorizationBehavior<TRequest, TResult>
- Проверяет `a.ChatId == request.AdminChatId && a.IsActive`.
- Распространяется на всё, что реализует `IAdminCommand`.

#### ResultSuperAdminAuthorizationBehavior<TRequest, TResult>
- Проверяет `admin.Role == AdminRole.SuperAdmin`.

**Оценка:** ✅ Маркерные интерфейсы + Result pattern без reflection. Правильный подход.

### 2.4 Result.cs
- `Result<T>` с nested private `SuccessResult`/`FailureResult`.
- Есть `Match` для функциональной композиции.
- Статическая фабрика `Result` для неявной типизации.

**Оценка:** ✅ Хорошая реализация.

### 2.5 Feature: AdminManagement

| Use case | Authorization | Validator | Примечания |
|----------|-------------|-----------|------------|
| CloseRequest | ✅ IAdminCommand | ✅ | Проверяет `ClientId` + `AdminChatId` |
| GetBotUsers | ✅ ISuperAdminCommand | ✅ | Исключает активных админов из списка |
| GrantAdminRights | ✅ ISuperAdminCommand | ✅ | Использует `DateTime.UtcNow` вместо `IDateTime` |
| ReassignClient | ✅ ISuperAdminCommand | ✅ | Переназначает активные заявки |
| RevokeAdminRights | ✅ ISuperAdminCommand | ✅ | Запрещает само-отзыв и отзыв SuperAdmin; перераспределяет заявки |
| CheckAdminStatus | ❌ нет | ❌ | Query возвращает `bool`, не `Result<bool>` |

### 2.6 Feature: BotStart

- `StartBotHandler` не требует админских прав.
- Upsert `TlgUser`, синхронизация `IsAdmin` с таблицей `Admins`.
- Несколько вызовов `SaveChangesAsync`.

**Оценка:** ⚠️ Функционально верно, но обновление `TlgUser.IsAdmin` дублирует информацию из `Admins`. Лучше удалить флаг или управлять им через доменный метод.

### 2.7 Feature: Dashboard

- `GetAdminDashboardHandler` — самый сложный handler.
- Хорошо предотвращает N+1 через `Dictionary` для имён пользователей.
- Содержит 3 приватных метода: заявки, квартиры, менеджеры.

**Оценка:** ⚠️ Граничное нарушение SRP. Можно разделить на 3 query/handler'а и собирать в PageModel.

### 2.8 Feature: RentalApplications

- `SubmitRentalApplicationHandler` — выбор менеджера с наименьшей нагрузкой, лимит 5 заявок.
- Сообщения об ошибках (`ApplicationLimitExceeded`, `NoActiveManagers`) зашиты в `ApplicationMessages`.
- `GetUserApplicationsHandler` — **N+1** в подзапросе `ManagerUsername`.

### 2.9 Feature: Messages / Users / TlgUsers

- Query (`GetMessageBody`, `GetMessagePathToPhoto`, `GetUserLanguage`, `CheckUserStatus`) не возвращают `Result<T>`.
- Валидаторы для query отсутствуют.
- `ToggleUserKickCommand` — **не** требует админских прав, так как вызывается из `MyChatMember` update на основе собственного chat id. Это корректное поведение, но в старых версиях аудита ошибочно считалось уязвимостью.

---

## 3. SOLID

### S — Single Responsibility Principle
- ✅ Большинство handler'ов имеют одну ответственность.
- ⚠️ `GetAdminDashboardHandler` собирает слишком много данных.
- ⚠️ `ApplicationMessages` смешивает несколько сообщений и языки.

### O — Open/Closed Principle
- ✅ Pipeline behaviors расширяемы.
- ✅ Новые feature-папки не требуют изменения существующих.
- ❌ `ApplicationMessages`, enum `Other`, `Message.BodyEn/BodyKa` требуют правки кода при добавлении языка/сообщения.

### L — Liskov Substitution Principle
- ✅ Handler'ы взаимозаменяемы для MediatR.
- ✅ Success/Failure результаты взаимозаменяемы.

### I — Interface Segregation Principle
- ✅ `IAdminCommand` / `ISuperAdminCommand` — минимальные маркеры.
- ✅ `IBotDbContext` — узкий контракт.
- ⚠️ `IDateTime` существует, но почти не используется в Application.

### D — Dependency Inversion Principle
- ✅ Handler'ы зависят от `IBotDbContext` и `IUserNotifier`.
- ❌ `GlobalUsing.cs` тянет `Microsoft.EntityFrameworkCore` — прямая зависимость от инфраструктуры.
- ❌ `DateTime.UtcNow` в `GrantAdminRightsHandler`.

---

## 4. Главные проблемы Application

1. **Clean Architecture violation**: `global using Microsoft.EntityFrameworkCore;` в Application.
2. **Непоследовательный Result pattern**: Query возвращают примитивы.
3. **N+1 в `GetUserApplicationsHandler`**.
4. **Мёртвый код в Result-классах**: `ErrorMessage`, `Failure(...)`.
5. **Недостаточная валидация**: нет validators для 6 query.
6. **Дублирование `TlgUser.IsAdmin`** и слабая инкапсуляция Domain.
7. **Локализация зашита в код**: `ApplicationMessages`, `ManagerNotificationFormatter`.

---

## 5. Рекомендации

1. Убрать `global using Microsoft.EntityFrameworkCore;`; вынести EF-зависимость в `IBotDbContext` как `IQueryable<T>` или перейти на repository/specification.
2. Привести все query к `Result<T>`.
3. Добавить validators для всех query.
4. Использовать `IDateTime` во всех handler'ах.
5. Исправить N+1 через `Dictionary` lookup.
6. Вынести строки ошибок и сообщения в ресурсы/БД.
7. Удалить мёртвые `ErrorMessage`/`Failure` из Result-классов.
8. Покрыть Application-тестами все handlers и validators.
