using Bot;
using Bot.Configuration;
using Web.Middleware;
using Web.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<TelegramBotConfiguration>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.Configure<WebhookConfiguration>(
    builder.Configuration.GetSection("Webhook"));
builder.Services.Configure<AdminNotificationConfiguration>(
    builder.Configuration.GetSection("AdminNotifications"));
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("Bot"));

builder.Services.AddTelegramBotServices();
builder.Services.AddScoped<ITmaValidationService, TmaValidationService>();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "XSRF-TOKEN";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
        : CookieSecurePolicy.Always;
});
builder.Services.AddAuthentication("AdminAuth")
    .AddCookie("AdminAuth", options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.None 
            : CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddHostedService<BotInitializationService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();