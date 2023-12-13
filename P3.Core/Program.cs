using Hangfire;
using Microsoft.EntityFrameworkCore;
using P3.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var connectionString = builder.Configuration.GetConnectionString("HangfireConnection");
builder.Services.AddHangfire(configuration => configuration
.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
.UseSimpleAssemblyNameTypeSerializer()
.UseRecommendedSerializerSettings()
.UseSqlServerStorage(connectionString,new Hangfire.SqlServer.SqlServerStorageOptions
{
    CommandBatchMaxTimeout=TimeSpan.FromMinutes(5),
    SlidingInvisibilityTimeout=TimeSpan.FromMinutes(5),
    QueuePollInterval=TimeSpan.Zero,
    UseRecommendedIsolationLevel=true,
    DisableGlobalLocks=true

}));
builder.Services.AddHangfireServer();
builder.Services.AddHttpClient("baseurl", client =>
{
    client.BaseAddress = new Uri("http://101.2.165.187:8090");
    //client.BaseAddress = new Uri("https://localhost:8090");
});

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

app.UseRouting();

app.UseAuthorization();
app.UseHangfireDashboard();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});
app.Run();
