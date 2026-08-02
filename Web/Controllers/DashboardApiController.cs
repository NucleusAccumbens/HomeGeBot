using Bot.Common;
using Bot.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DashboardApiController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardApiController(TelegramBot telegramBot, IOptions<TelegramBotConfiguration> botConfig,
        ILogger<DashboardApiController> logger, IHttpClientFactory httpClientFactory)
    {
        _telegramBot = telegramBot;
        _botConfig = botConfig.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

            if (string.IsNullOrEmpty(file.FilePath))
                return NotFound();

            var stream = new MemoryStream();
            var httpClient = _httpClientFactory.CreateClient();
            var fileUrl = $"https://api.telegram.org/file/bot{_botConfig.Token}/{file.FilePath}";
            var response = await httpClient.GetAsync(fileUrl, HttpContext.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download photo from Telegram for chatId {ChatId}: {StatusCode}", chatId, response.StatusCode);
                return StatusCode(500, "Failed to fetch user photo");
            }

            await response.Content.CopyToAsync(stream);
            stream.Position = 0;

            return File(stream, response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user photo for chatId {ChatId}", chatId);
            return StatusCode(500, "Failed to fetch user photo");
        }
    }
}
