using AlphaSquad.Backoffice.Navigation;
using AlphaSquad.Backoffice.Security;
using AlphaSquad.Backoffice.Services;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Lmt.Application.Http.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["Apis:AlphaSquad:BaseUrl"]
                 ?? throw new InvalidOperationException("The API base URL was not configured.");

builder.Services.Configure<BackofficeBootstrapOptions>(builder.Configuration.GetSection("BackofficeBootstrap"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // O backoffice usa uma sessao propria para manter seu estado independente do dashboard das academias.
    options.Cookie.Name = "alphasquad.backoffice.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // O cookie do backoffice nao deve colidir com o cookie do dashboard operacional da academia.
        options.Cookie.Name = "alphasquad.backoffice.auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BackofficeAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(BackofficeRoles.Owner);
    });
});

builder.Services.AddScoped<IAccessTokenAccessor, AccessTokenAccessor>();
builder.Services.AddScoped<IBackofficeNavigationService, BackofficeNavigationService>();
builder.Services.AddSingleton<IBackofficeTenantWorkspaceService, BackofficeTenantWorkspaceService>();
builder.Services.AddGeneratedApi(apiBaseUrl);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Backoffice", "BackofficeAccess");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
