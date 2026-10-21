using StockDrawdownAnalyzer.Components;
using StockDrawdownAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddSingleton<StockData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/api/stocks", (StockData data) => data.GetSummaries());
app.MapGet("/api/stocks/{symbol}", (string symbol, StockData data) =>
    data.GetDetail(symbol) is { } detail ? Results.Ok(detail) : Results.NotFound());

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(StockDrawdownAnalyzer.Client._Imports).Assembly);

app.Run();
