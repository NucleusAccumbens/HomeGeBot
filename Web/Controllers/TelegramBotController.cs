using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Bot.Common.Interfaces;
using Bot.Common;
using Bot.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web.Controllers;

[ApiController]
[Route("api/message/update")]
public class TelegramBotController : ControllerBase
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ILogger<TelegramBotController> _logger;
    private readonly ICommandAnalyzer _commandAnalyzer;
    private readonly TelegramBot _bot;
    private readonly AdminNotificationConfiguration _adminNotifications;
    private readonly WebhookConfiguration _webhookConfig;
    private readonly IWebHostEnvironment _environment;

    public TelegramBotController(ILogger<TelegramBotController> logger, ICommandAnalyzer commandAnalyzer,
        TelegramBot bot, IExceptionNotification exceptionNotification,
        IOptions<AdminNotificationConfiguration> adminNotifications,
        IOptions<WebhookConfiguration> webhookConfig,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _commandAnalyzer = commandAnalyzer;
        _bot = bot;
        _exceptionNotification = exceptionNotification;
        _adminNotifications = adminNotifications.Value;
        _webhookConfig = webhookConfig.Value;
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Update update)
    {
        if (!ValidateWebhookRequest())
        {
            _logger.LogWarning("Unauthorized webhook request rejected");
            return Unauthorized();
        }

        try
        {
            _logger.LogInformation("Получен Update.");
            await _commandAnalyzer.AnalyzeCommandsAsync(await _bot.GetBot(),
                update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram update");

            try
            {
                var client = await _bot.GetBot();
                await _exceptionNotification.SendExceptionNotification(
                    client,
                    $"[{ex.GetType().Name}] {ex.Message}",
                    _adminNotifications.ChatIds);
            }
            catch (Exception notifyEx)
            {
                _logger.LogError(notifyEx, "Failed to send exception notification to admins");
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
            if (_environment.IsProduction())
            {
                _logger.LogError("Webhook SecretToken is not configured in production. Rejecting request.");
                return false;
            }

            _logger.LogWarning("Webhook SecretToken not configured, skipping validation");
            return true;
        }

        if (Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerValue))
        {
            return headerValue == webhookSecret;
        }

        return false;
    }
}
