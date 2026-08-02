using Bot.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web.Middleware;

public class ValidateTelegramWebhookMiddleware
{
    public const string WebhookPath = "/api/message/update";
    public const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    private readonly RequestDelegate _next;
    private readonly ILogger<ValidateTelegramWebhookMiddleware> _logger;
    private readonly WebhookConfiguration _webhookConfig;
    private readonly IWebHostEnvironment _environment;

    public ValidateTelegramWebhookMiddleware(
        RequestDelegate next,
        IOptions<WebhookConfiguration> webhookConfig,
        IWebHostEnvironment environment,
        ILogger<ValidateTelegramWebhookMiddleware> logger)
    {
        _next = next;
        _webhookConfig = webhookConfig.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(WebhookPath))
        {
            await _next(context);
            return;
        }

        var webhookSecret = _webhookConfig.SecretToken;

        if (string.IsNullOrEmpty(webhookSecret))
        {
            if (_environment.IsProduction())
            {
                _logger.LogError("Webhook SecretToken is not configured in production. Rejecting request.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            _logger.LogWarning("Webhook SecretToken not configured, skipping validation");
            await _next(context);
            return;
        }

        if (context.Request.Headers.TryGetValue(SecretTokenHeader, out var headerValue) &&
            headerValue == webhookSecret)
        {
            await _next(context);
            return;
        }

        _logger.LogWarning("Unauthorized webhook request rejected");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
}
