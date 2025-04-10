using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Profile;
using UCMS.Repositories.UserRepository;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Services.AuthService;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.EmailService;
using UCMS.Services.EmailService.Abstraction;

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
builder.Services.AddHttpContextAccessor();


// builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseRouting();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



