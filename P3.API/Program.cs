using Microsoft.EntityFrameworkCore;
using P3.API.Data;
using P3.API.Repository.IRepository;
using P3.API.Repository;

var builder = WebApplication.CreateBuilder(args);

//============================= Register =====================================
// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("HistoryDataCors",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                          
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient("baseurl", client =>
{
    client.BaseAddress = new Uri("https://financialmodelingprep.com/api/v3/");
});

//============================= Middleware =====================================
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("HistoryDataCors");

app.MapControllers();

app.Run();
