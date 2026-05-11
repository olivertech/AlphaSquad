using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Lmt.Application.Http.DependencyInjection;
using AlphaSquad.Web.Dashboard.Metrics;
using AlphaSquad.Web.Navigation;
using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["Apis:AlphaSquad:BaseUrl"]
                 ?? throw new InvalidOperationException("The API base URL was not configured.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // O dashboard guarda o token da API em sessao para que a camada LMT o reutilize nas chamadas HTTP.
    options.Cookie.Name = "alphasquad.web.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // O cookie controla a navegacao do dashboard; a autorizacao da API continua baseada no JWT em sessao.
        options.Cookie.Name = "alphasquad.web.auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DashboardAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(DashboardRoles.Admin, DashboardRoles.Teacher);
    });
});

builder.Services.AddScoped<IAccessTokenAccessor, AccessTokenAccessor>();
builder.Services.AddScoped<IDashboardNavigationService, DashboardNavigationService>();
builder.Services.AddSingleton<IDashboardMetricCatalog, DashboardMetricCatalog>();
builder.Services.AddGeneratedApi(apiBaseUrl);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Dashboard", "DashboardAccess");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
