using ControleFinanceiroDesktop.Services.Header;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Services.CategoryServices;
using ControleFinanceiroWeb.Services.Helpers;
using ControleFinanceiroWeb.Services.StatementServices;
using ControleFinanceiroWeb.Services.StatementTypeServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseFirebird(@"User=SYSDBA;Password=masterkey;Database=C:\Users\LeonardoL\Documents\BackupBancoDados\DATABASE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=ISO8859_1;"));

builder.Services.AddScoped<MainViewServices>();
builder.Services.AddScoped<MainViewModelBuilder>();
builder.Services.AddScoped<StatementTypeSaveServices>();
builder.Services.AddScoped<StatementTypeDeleteServices>();
builder.Services.AddScoped<CategoryListServices>();
builder.Services.AddScoped<SatatementSaveServices>();
builder.Services.AddScoped<StatementDeleteServices>();
builder.Services.AddScoped<StatementExcelImportService>();
builder.Services.AddScoped<CategoryReidentificationServices>();
builder.Services.AddScoped<CategoryListServices>();
builder.Services.AddScoped<CategoryDeleteServices>();
builder.Services.AddScoped<CategoryGetServices>();
builder.Services.AddScoped<CategorySaveServices>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Transactions/Index/0");
        return;
    }

    await next();
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transactions}/{action=Index}/{id=0}");



app.Run();
