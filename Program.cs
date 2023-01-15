using GymManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GymManagerApi.Data;
using Microsoft.AspNetCore.Identity;
using GymManagerApi.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using GymManagerApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GymManagerApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GymManagerContext") ?? throw new InvalidOperationException("Connection string 'GymManagerApiContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GymManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GymManagerContext") ?? throw new InvalidOperationException("Connection string 'GymManagerContext' not found.")));

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GymManagerApiContext>();

builder.Services.AddScoped<ApiKeyService>();


builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });


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
