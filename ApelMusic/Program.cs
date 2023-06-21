using System.Text;
using ApelMusic.Database.Migrations;
using ApelMusic.Database.Repositories;
using ApelMusic.Database.Seeds;
using ApelMusic.Email;
using ApelMusic.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region AddScoped Migration dan seeder
builder.Services.AddScoped<MainMigrations>();
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<CourseSeeder>();
builder.Services.AddScoped<UserSeeder>();
#endregion

#region AddScoped Repostories
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<CourseScheduleRepository>();
builder.Services.AddScoped<PaymentMethodRepository>();
builder.Services.AddScoped<ShoppingCartRepository>();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<UsersCoursesRepository>();
#endregion

#region AddScoped Services
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ImageServices>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<CourseScheduleService>();
builder.Services.AddScoped<PaymentMethodService>();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddScoped<PurchaseService>();
#endregion

#region Email Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.AddScoped<EmailService>();
#endregion

#region Konfigurasi JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    // AddPolicy(Nama Policy, requireRole(Role dari jwtnya))
    options.AddPolicy("ADMIN", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("USER", policy => policy.RequireRole("USER"));
});
#endregion

#region CORs settings
string allowedOrigin = builder.Configuration.GetValue<string>("CORs:AllowedOrigin");

builder.Services.AddCors(options => options.AddPolicy(name: "AllowedOrigins",
    builder => builder.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod()
));
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowedOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
