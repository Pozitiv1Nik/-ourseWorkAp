using UI.Components;
using BLL.Interfaces;
using BLL.Services;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ������ �������� ���������� ��� Web API
builder.Services.AddControllers();

// ������ HttpClient ��� Blazor ����������
builder.Services.AddHttpClient();

// ������������ JWT Authentication
var jwtKey = "your_super_secret_key_123456"; // ��� ����� ����, �� � � AuthController
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ������ Authorization
builder.Services.AddAuthorization();

// Entity Framework
builder.Services.AddDbContext<ApDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(BLL.MappingProfile));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
builder.Services.AddScoped<IResumeVacancyLinkRepository, ResumeVacancyLinkRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IVacancyService, VacancyService>();
builder.Services.AddScoped<IResumeVacancyLinkService, ResumeVacancyLinkService>();

// ������ CORS ��� Web API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ������ CORS
app.UseCors("AllowBlazor");

// ������ Authentication �� Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// ������ ���������� ��� Web API
app.MapControllers();

// ������ Blazor ����������
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();