using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Bot.Common.Interfaces;
using Bot.Common;
using Logger.Interfaces;

namespace Web.Controllers;

[ApiController]
[Route("api/message/update")]
public class TelegramBotController : ControllerBase
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger;
    private readonly ICommandAnalyzer _commandAnalyzer;
    private readonly TelegramBot _bot;

    public TelegramBotController(ICustomLogger logger, ICommandAnalyzer commandAnalyzer, 
        TelegramBot bot, IExceptionNotification exceptionNotification)
    {
        _logger = logger;
        _commandAnalyzer = commandAnalyzer;
        _bot = bot;
        _exceptionNotification = exceptionNotification;
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Update update)
    {
        try
        {
            _logger.LogAction($"Получен Update.");
            await _commandAnalyzer.AnalyzeCommandsAsync(await _bot.GetBot(),
                update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);

            var client = await _bot.GetBot();
            await _exceptionNotification.SendExceptionNotification(client, ex.Message, 
                444343256, 2030541425);

            return Ok();
        }

        return Ok();
    }
}
