using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Mailer;
using AspnetCoreMvcFull.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);


// Connect to the database
builder.Services.AddDbContext<AspnetCoreMvcFullContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AspnetCoreMvcFullContext") ?? throw new InvalidOperationException("Connection string 'AspnetCoreMvcFullContext' not found.")));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();

// Bind de perfiles SMTP (ya lo tenías bien)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSignus"));
builder.Services.Configure<SmtpSettings>("Signus", builder.Configuration.GetSection("SmtpSignus"));
builder.Services.Configure<SmtpSettings>("Diverscan", builder.Configuration.GetSection("SmtpDiverscan"));
builder.Services.Configure<SmtpSettings>("Smartcosta", builder.Configuration.GetSection("SmtpSmartcosta"));

// Selector
builder.Services.AddSingleton<ISmtpSelector, SmtpSelector>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

builder.Services.AddMemoryCache();

// Create a service scope to get an AspnetCoreMvcFullContext instance using DI and seed the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsePathBase("/SIGNUS_ID");
app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
   name: "default",
pattern: "{controller=Auth}/{action=Login}/{id?}");//pattern: "{controller=Auth}/{action=Login}/{id?}");// pattern: "{controller=Dashboards}/{action=Index}/{id?}"); //

app.Run();
