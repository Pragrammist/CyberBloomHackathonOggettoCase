using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;
using static Consts;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;

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
    options.CallbackPath = "/api/signin-google";
});


builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration["CONNECTION_STRING"])));
        //.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection")));

    // Add the processing server as IHostedService
builder.Services.AddHangfireServer();


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
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
      ForwardedHeaders = ForwardedHeaders.XForwardedProto
});



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


app.UseHangfireDashboard("/workers", new DashboardOptions
{
    Authorization = new [] { new AdminAuthorizationFilter() }
});





app.MapControllers();
app.MapHangfireDashboard();
//app.MapRazorPages();

app.Run();


public class AdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;

        if (user.IsInRole("модератор"))
            return true;

        return false;
    }
}
