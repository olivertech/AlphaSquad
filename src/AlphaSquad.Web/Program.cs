using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Lmt.Application.Http.DependencyInjection;
using AlphaSquad.Web.Security;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["Apis:AlphaSquad:BaseUrl"]
                 ?? throw new InvalidOperationException("The API base URL was not configured.");

builder.Services.AddScoped<IAccessTokenAccessor, AccessTokenAccessor>();
builder.Services.AddGeneratedApi(apiBaseUrl);

// Add services to the container.
builder.Services.AddRazorPages();

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
