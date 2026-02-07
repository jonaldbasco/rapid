using rapid.core.app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using rapid.core.app.Plugin;


var builder = WebApplication.CreateBuilder(args);

//Ignore SSL certificate validation for development purposes
builder.Services.AddHttpClient("IgnoreSSL")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
    });

// -----------------------------
// Configure Services
// -----------------------------

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

//var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
//builder.Services.AddDbContext<AicoDBContext>(options =>
//    options.UseSqlite(connectionString));


// -----------------------------
// Register Kernel
// -----------------------------
builder.Services.AddSingleton<Kernel>(sp =>
{
    var config = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
    var emailPlugin = sp.GetRequiredService<EmailPlugin>();

    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.AddOpenAIChatCompletion(
        modelId: config.Model,
        apiKey: config.ApiKey
    );

    //kernelBuilder.Plugins.AddFromPromptDirectory("Plugin", "HealthSummarizer");
    kernelBuilder.Plugins.AddFromObject(emailPlugin, "email");

    var kernel = kernelBuilder.Build();

    // Memory is available via DI, not attached directly to kernel
    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAPID API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
