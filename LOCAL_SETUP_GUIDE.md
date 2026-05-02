# Локальный запуск и тестирование

## Требования

- PostgreSQL + pgAdmin
- .NET 6.0 SDK
- ngrok

## Быстрый старт

```powershell
# 1. Создать БД в pgAdmin: propertybot_dev

# 2. Скопировать и настроить конфиг
Copy-Item Web\appsettings.example.json Web\appsettings.json
# Отредактировать: пароль БД, токен бота, ngrok URL, ваш ChatId

# 3. Применить миграции
dotnet ef database update --project Infrastructure --startup-project Web

# 4. Запустить ngrok в отдельном окне
cd C:\ngrok
.\ngrok http 5000

# 5. Обновить ngrok URL в appsettings.json

# 6. Запустить
cd Web
dotnet run
```

## Структура appsettings.json

```json
{
  "InDeveloping": true,
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=propertybot_dev;Username=postgres;Password=ВАШ_ПАРОЛЬ;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=True"
  },
  "TelegramBot": {
    "Token": "YOUR_TELEGRAM_BOT_TOKEN"
  },
  "Webhook": {
    "BaseUrl": "https://ВАША_NGROK_URL/",
    "SecretToken": "любая_случайная_строка"
  },
  "AdminNotifications": {
    "ChatIds": [ВАШ_CHAT_ID]
  },
  "Bot": {
    "SourceChannelId": -1001580911411,
    "AdminWhitelist": [ВАШ_CHAT_ID]
  }
}
```

**Параметры:**
- `Token` — от @BotFather
- `BaseUrl` — URL из ngrok
- `ChatIds` — ваш ID от @userinfobot (уведомления об ошибках)
- `SourceChannelId` — ID канала с квартирами (оставить как есть)
- `AdminWhitelist` — список chat ID, которым можно стать администратором (управление через веб-панель)

## Локальное тестирование

### Флоу заявки на аренду

1. Отправить `/start` боту
2. Выбрать страну
3. Ввести профессию
4. Выбрать наличие питомцев
5. Выбрать срок аренды
6. Переслать пост из канала @propertyintbilisi
7. Проверить: менеджер получает уведомление с кнопкой "Написать"

### Добавление администратора

Через веб-панель: `http://localhost:5000` → управление пользователями.

### Проверка данных

```sql
-- В pgAdmin Query Tool
SELECT * FROM "TlgUsers" ORDER BY "CreatedAt" DESC;
SELECT * FROM "Clients" ORDER BY "CreatedAt" DESC;
SELECT * FROM "Admins";
```

### Swagger

http://localhost:5000/swagger — документация API

## Команды

```powershell
# Миграции
dotnet ef migrations add Имя --project Infrastructure --startup-project Web
dotnet ef database update --project Infrastructure --startup-project Web

# Сборка
dotnet build
dotnet run --project Web

# Тесты (если есть)
dotnet test
```

## Типичные проблемы

**"No connection could be made"** — PostgreSQL не запущен или неверный пароль.

**"relation does not exist"** — не применены миграции: `dotnet ef database update`.

**Бот не отвечает** — проверить ngrok URL в конфиге.

**"Bot token is invalid"** — скопировать токен заново от @BotFather.

## Чеклист тестирования

- [ ] `/start` — бот отвечает
- [ ] Выбор страны — работает
- [ ] Ввод профессии — сохраняется
- [ ] Выбор питомцев — работает
- [ ] Срок аренды — сохраняется
- [ ] Пересылка поста — заявка создаётся
- [ ] Админ получает уведомление
- [ ] Данные видны в pgAdmin
