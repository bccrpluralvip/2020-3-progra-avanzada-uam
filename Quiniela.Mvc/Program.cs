using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quiniela.Model;
using Quiniela.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

var matchesApiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (!Uri.TryCreate(matchesApiBaseUrl, UriKind.Absolute, out var matchesApiUri))
{
    throw new InvalidOperationException("Configuration value 'Api:BaseUrl' must be an absolute URI.");
}

builder.Services.AddHttpClient<MatchesApiClient>(client =>
{
    client.BaseAddress = matchesApiUri;
});

var connectionString = builder.Configuration.GetConnectionString("MySecretPostgresConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'MySecretPostgresConnection' is not configured.");
}

builder.Services.AddDbContext<QuinielaDbContext>(
    options => options.UseQuinielaPostgres(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Quiniela}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
