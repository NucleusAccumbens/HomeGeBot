using Bot;
using Bot.Common;
using Logger;
using Logger.Interfaces;
using Web.BackgroundTasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddTelegramBotServices();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSingleton<ICustomLogger, CustomLogger>();
builder.Services.AddHostedService<ConsumeScopedHostedService>();
builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.Services.GetRequiredService<TelegramBot>().GetBot().Wait();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorPages();

app.MapControllers();

app.Run();