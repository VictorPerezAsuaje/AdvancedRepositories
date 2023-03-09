using AdvancedRepositories.Core;
using WebClient.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleTypedRepository, ArticleTypedRepository>();
builder.Services.AddRepositoriesConfiguration(c =>
{
    c.Server = "(localdb)\\MSSQLLocalDB";
    c.DbName = "RepositoriesTestDb";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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