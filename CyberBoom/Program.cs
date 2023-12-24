using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;
using static Consts;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Google;


TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.SpeackerImage, s => s.SpeackerImage.JoinFileNames()); 
TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.SpeackerImage, s => s.SpeackerImage.JoinFileNames());


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(TOKENS_SEPORATOR, s.Splecializations));

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(TOKENS_SEPORATOR, s.Splecializations));


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());
var dir = Directory.CreateDirectory("cyber-boom-files");
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(builder.Configuration["CONNECTION_STRING"]));

builder.Services.AddIdentity<User, IdentityRole>()
.AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var bearerOptions = new BearerAccessTokenOptions();
    options.RequireHttpsMetadata = bearerOptions.RequiredHttpsMetadata;
    options.TokenValidationParameters = bearerOptions.TokenValidationParameters;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
    options.CallbackPath = "https://cyberbloom.zetcraft.ru/api/users/google-sign-in";
});




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyMethod());

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "cyber-boom-files")),
    RequestPath = "/api/cyber-boom-files"
});

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();



app.UseAuthentication();    // подключение аутентификации
app.UseAuthorization();



app.MapControllers();
//app.MapRazorPages();

app.Run();
