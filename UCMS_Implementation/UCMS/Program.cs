using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UCMS.Data;
using UCMS.Middleware;
using UCMS.Profile;
using UCMS.Repositories.RoleRepository;
using UCMS.Repositories.RoleRepository.Abstraction;
using UCMS.Repositories.UserRepository;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Services.AuthService;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.CookieService;
using UCMS.Services.CookieService.Abstraction;
using UCMS.Services.EmailService;
using UCMS.Services.EmailService.Abstraction;
using UCMS.Services.RoleService;
using UCMS.Services.RoleService.Abstraction;
using UCMS.Services.TokenService;
using UCMS.Services.TokenService.Abstraction;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
// var mapperConfig = new MapperConfiguration(cfg =>
// {
//     cfg.AddProfile(new AutoMapperProfile());
// });
//
// // ساخت Mapper و اضافه کردن به DI
// IMapper mapper = mapperConfig.CreateMapper();
// builder.Services.AddSingleton(mapper);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICookieService,CookieService>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IOneTimeCodeService, OneTimeCodeService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.HttpContext.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


// builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// Seed data
var scope = app.Services.CreateScope();
var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
await SeedData.Initialize(scope.ServiceProvider, roleService);

    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate(); // This applies all migrations

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();



