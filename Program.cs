using ECommerceApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<AppDbContext>( options => options.UseSqlite("Data Source = ecommerce.db"));

builder.Services.AddOpenApi();

var app = builder.Build();



app.Run();


