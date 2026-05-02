using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Bot.Common.Interfaces;
using Bot.Common;
using Bot.Configuration;
using Logger.Interfaces;
using Microsoft.Extensions.Options;

namespace Web.Controllers;

[ApiController]
[Route("api/message/update")]
public class TelegramBotController : ControllerBase
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger;
    private readonly ICommandAnalyzer _commandAnalyzer;
    private readonly TelegramBot _bot;
    private readonly AdminNotificationConfiguration _adminNotifications;
    private readonly WebhookConfiguration _webhookConfig;

    public TelegramBotController(ICustomLogger logger, ICommandAnalyzer commandAnalyzer,
        TelegramBot bot, IExceptionNotification exceptionNotification,
        IOptions<AdminNotificationConfiguration> adminNotifications,
        IOptions<WebhookConfiguration> webhookConfig)
    {
        _logger = logger;
        _commandAnalyzer = commandAnalyzer;
        _bot = bot;
        _exceptionNotification = exceptionNotification;
        _adminNotifications = adminNotifications.Value;
        _webhookConfig = webhookConfig.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Update update)
    {
        if (!ValidateWebhookRequest())
        {
            _logger.LogAction("Unauthorized webhook request rejected");
            return Unauthorized();
        }

        try
        {
            _logger.LogAction($"Получен Update.");
            await _commandAnalyzer.AnalyzeCommandsAsync(await _bot.GetBot(),
                update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);

            try
            {
                var client = await _bot.GetBot();
                await _exceptionNotification.SendExceptionNotification(
                    client,
                    $"[{ex.GetType().Name}] {ex.Message}",
                    _adminNotifications.ChatIds);
            }
            catch
            {
                // не даём упасть, если уведомление недоступно
            }

            return Ok();
        }

        return Ok();
    }

    private bool ValidateWebhookRequest()
    {
        var webhookSecret = _webhookConfig.SecretToken;

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogAction("Warning: Webhook SecretToken not configured, skipping validation");
            return true;
        }

        if (Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerValue))
        {
            return headerValue == webhookSecret;
        }

        return false;
    }
}
