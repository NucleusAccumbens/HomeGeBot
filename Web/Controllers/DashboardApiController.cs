using Bot.Common;
using Bot.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Web.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(AuthenticationSchemes = "AdminAuth")]
public class DashboardApiController : ControllerBase
{
    private readonly TelegramBot _telegramBot;
    private readonly TelegramBotConfiguration _botConfig;

    public DashboardApiController(TelegramBot telegramBot, IOptions<TelegramBotConfiguration> botConfig)
    {
        _telegramBot = telegramBot;
        _botConfig = botConfig.Value;
    }

    [HttpGet("user-photo")]
    public async Task<IActionResult> GetUserPhoto([FromQuery] long chatId)
    {
        if (chatId <= 0)
            return BadRequest("Invalid chatId");

        try
        {
            var client = await _telegramBot.GetBot();
            var photos = await client.GetUserProfilePhotosAsync(chatId, limit: 1);

            if (photos.Photos.Length == 0 || photos.Photos[0].Length == 0)
                return NotFound();

            var fileId = photos.Photos[0][0].FileId;
            var file = await client.GetFileAsync(fileId);

            var fileUrl = $"https://api.telegram.org/file/bot{_botConfig.Token}/{file.FilePath}";
            return Ok(new { photoUrl = fileUrl });
        }
        catch
        {
            return StatusCode(500, "Failed to fetch user photo");
        }
    }
}
