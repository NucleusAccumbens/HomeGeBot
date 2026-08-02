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
    private readonly ITelegramBotClientProvider _bot;
    private readonly AdminNotificationConfiguration _adminNotifications;

    public TelegramBotController(
        ILogger<TelegramBotController> logger,
        ICommandAnalyzer commandAnalyzer,
        ITelegramBotClientProvider bot,
        IExceptionNotification exceptionNotification,
        IOptions<AdminNotificationConfiguration> adminNotifications)
    {
        _logger = logger;
        _commandAnalyzer = commandAnalyzer;
        _bot = bot;
        _exceptionNotification = exceptionNotification;
        _adminNotifications = adminNotifications.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Update update)
    {
        try
        {
            _logger.LogInformation("Получен Update.");
            await _commandAnalyzer.AnalyzeCommandsAsync(await _bot.GetBot(), update);
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
                    _adminNotifications.ChatIds.Select(Domain.Common.ChatId.FromLong).ToArray());
            }
            catch (Exception notifyEx)
            {
                _logger.LogError(notifyEx, "Failed to send exception notification to admins");
            }

            return Ok();
        }

        return Ok();
    }
}
