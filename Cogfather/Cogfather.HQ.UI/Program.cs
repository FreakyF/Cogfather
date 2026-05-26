using Cogfather.HQ.Application.Extensions;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Infrastructure;
using Cogfather.HQ.Infrastructure.Data;
using Cogfather.HQ.Infrastructure.Identity;
using Cogfather.HQ.Infrastructure.Services;
using Cogfather.HQ.UI.Api;
using Cogfather.HQ.UI.Components;
using Cogfather.HQ.UI.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Node", "HQ")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Node}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddSingleton<PreauthTokenService>();
builder.Services.AddSingleton<ConsensusEventService>();
builder.Services.AddSingleton<IConsensusNotifier, UiConsensusNotifier>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CogfatherAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var hqDb = scope.ServiceProvider.GetRequiredService<HqDbContext>();
    await hqDb.Database.EnsureCreatedAsync();

    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await authDb.Database.EnsureCreatedAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline'; connect-src 'self' ws: wss:";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHub<ConsensusHub>("/hubs/consensus");

app.MapHealthChecks("/api/v1/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            components = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString())
        });
        await ctx.Response.WriteAsync(result);
    }
}).AllowAnonymous();

app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInMgr) =>
{
    await signInMgr.SignOutAsync();
    return Results.Redirect("/account/login");
}).RequireAuthorization();

app.MapGet("/account/do-login", async (
    string token,
    string? returnUrl,
    PreauthTokenService tokens,
    UserManager<ApplicationUser> userMgr,
    SignInManager<ApplicationUser> signInMgr) =>
{
    var userId = tokens.ConsumeToken(token);
    if (userId is null)
        return Results.Redirect("/account/login?error=expired");

    var user = await userMgr.FindByIdAsync(userId);
    if (user is null)
        return Results.Redirect("/account/login?error=notfound");

    await signInMgr.SignInAsync(user, isPersistent: false);
    return Results.Redirect(returnUrl ?? "/");
}).AllowAnonymous();

app.MapOrdersEndpoints();
app.MapNodesEndpoints();
app.MapInventoryEndpoints();
app.MapRecipesEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
