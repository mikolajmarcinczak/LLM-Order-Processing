using Microsoft.EntityFrameworkCore;
using OrderProcessor.Application.Interfaces;
using OrderProcessor.Application.Services;
using OrderProcessor.Domain.Interfaces;
using OrderProcessor.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

#region Services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
#endregion

#region EF - DbContext with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderProcessorDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
#endregion

#region IMAP Email Service
var imapServer = builder.Configuration["IMAP_SERVER"];
var imapPortString = builder.Configuration["IMAP_PORT"];
var imapPort = int.Parse(imapPortString ?? "993");
var imapUsername = builder.Configuration["IMAP_USERNAME"];
var imapPassword = builder.Configuration["IMAP_PASSWORD"];
builder.Services.AddSingleton(new IMAPEmailService(imapServer, imapPort, imapUsername, imapPassword));
#endregion

#region LLM Services
builder.Services.AddHttpClient();
var openaiApiKey = builder.Configuration["OPENAI_API_KEY"] ?? throw new InvalidOperationException("OPENAI_API_KEY is not configured.");
builder.Services.AddSingleton(new LanguageModelService(openaiApiKey));

var ollamaApiUrl = builder.Configuration["OLLAMA_API_URL"] ?? "http://ollama:11434/api/generate";
builder.Services.AddSingleton<FallbackLanguageModelService>(serviceProvider => 
    new FallbackLanguageModelService(serviceProvider.GetRequiredService<HttpClient>(), ollamaApiUrl));

#region LLM - Composite for both models
builder.Services.AddSingleton<ILanguageModelService>(serviceProvider => 
    new CompositeLanguageModelService(
        serviceProvider.GetRequiredService<LanguageModelService>(),
        serviceProvider.GetRequiredService<FallbackLanguageModelService>()
    ));
#endregion
#endregion

builder.Services.AddScoped<OrderProcessingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run(); 
