using ApelMusic.Database.Migrations;
using ApelMusic.Database.Repositories;
using ApelMusic.Database.Seeds;
using ApelMusic.Email;
using ApelMusic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region AddScoped Migration dan seeder
builder.Services.AddScoped<MainMigrations>();
builder.Services.AddScoped<RoleSeeder>();
#endregion

#region AddScoped Repostories
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UserRepository>();
#endregion

#region AddScoped Services
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AuthService>();
#endregion

#region Email Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.AddScoped<EmailService>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
