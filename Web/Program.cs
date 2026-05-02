using Bot;
using Bot.Configuration;
using Logger;
using Logger.Interfaces;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<TelegramBotConfiguration>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.Configure<WebhookConfiguration>(
    builder.Configuration.GetSection("Webhook"));
builder.Services.Configure<AdminNotificationConfiguration>(
    builder.Configuration.GetSection("AdminNotifications"));
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("Bot"));

builder.Services.AddTelegramBotServices();
builder.Services.AddSingleton<ICustomLogger, CustomLogger>();
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
builder.Services.AddHostedService<BotInitializationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();

app.Run();