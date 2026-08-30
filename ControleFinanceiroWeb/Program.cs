using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Services;
using ControleFinanceiroWeb.Services.Transactions;
using ControleFinanceiroWeb.Services.Categories;
using ControleFinanceiroWeb.Services.StatementType;
using ControleFinanceiroWeb.Services.Summary;
using ControleFinanceiroWeb.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// The connection string lives in appsettings.Development.json for local runs.
// Outside development it must be supplied by the environment, so that no
// credential is ever committed to the repository:
//   ConnectionStrings__DefaultConnection="User=...;Password=...;Database=..."
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found. Set it in " +
        "appsettings.Development.json for local development, or through the " +
        "ConnectionStrings__DefaultConnection environment variable. " +
        "See database/README.md for the database setup.");
}

// A relative database name is resolved against the content root, so the app
// runs from a clone without anyone editing an absolute path.
if (connectionString.Contains("Database=DATABASE.FDB"))
{
    var absoluteDbPath = Path.Combine(builder.Environment.ContentRootPath, "DATABASE.FDB");
    connectionString = connectionString.Replace("Database=DATABASE.FDB", $"Database={absoluteDbPath}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseFirebird(connectionString));

builder.Services.AddScoped<IStatementTypeService, StatementTypeService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryIdentificationService, CategoryIdentificationService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddSingleton<IPinLockout, PinLockout>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ControleFinanceiro.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute
    {
        NoStore = true,
        Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None
    });

    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Summary}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
