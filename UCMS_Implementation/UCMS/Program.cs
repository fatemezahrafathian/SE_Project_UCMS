using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UCMS.Data;
using UCMS.Middleware;
using UCMS.Profile;
using UCMS.Repositories.ClassRepository;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.InstructorRepository;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Repositories.ProjectRepository;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Repositories.RoleRepository;
using UCMS.Repositories.RoleRepository.Abstraction;
using UCMS.Repositories.StudentRepository;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.TeamRepository;
using UCMS.Repositories.TeamRepository.Abstraction;
using UCMS.Repositories.UserRepository;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Services.AuthService;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.ClassService;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.CookieService;
using UCMS.Services.CookieService.Abstraction;
using UCMS.Services.EmailService;
using UCMS.Services.EmailService.Abstraction;
using UCMS.Services.FileService;
using UCMS.Services.ImageService;
using UCMS.Services.PasswordService;
using UCMS.Services.PasswordService.Abstraction;
using UCMS.Services.InstructorService;
using UCMS.Services.InstructorService.Abstraction;
using UCMS.Services.ProjectService;
using UCMS.Services.RoleService;
using UCMS.Services.RoleService.Abstraction;
using UCMS.Services.StudentService;
using UCMS.Services.StudentService.Abstraction;
using UCMS.Services.TeamService;
using UCMS.Services.TeamService.Abstraction;
using UCMS.Services.TokenService;
using UCMS.Services.TokenService.Abstraction;
using UCMS.Services.UserService;

var builder = WebApplication.CreateBuilder(args);


var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<ImageUploadSettings>(
    builder.Configuration.GetSection("ImageUploadSettings"));

builder.Services.Configure<TeamTemplateSettings>(
    builder.Configuration.GetSection("TeamTemplateSettings"));

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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ICookieService,CookieService>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IOneTimeCodeService, OneTimeCodeService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IStudentClassService, StudentClassService>();

builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();


builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateClassDtoValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
// ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;


// builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    //db.Database.Migrate(); // Ensures schema is created before anything else

    var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
    await SeedData.Initialize(scope.ServiceProvider, roleService); // Seed roles etc.
}

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
app.UseCors("AllowLocalhost5173");
app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();



