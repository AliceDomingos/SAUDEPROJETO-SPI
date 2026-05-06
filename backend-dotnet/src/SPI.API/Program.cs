using System.Text;
using SPI.Infrastructure.Data.Seed;
using SPI.Infrastructure.Data.Security;
using SPI.Infrastructure.IoC;
using SPI.Api.Extensions;
using SPI.Api.Middlewares;
using SPI.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var developmentUrlOptions = builder.ConfigureDevelopmentUrls();

builder.Services.AddSingleton(developmentUrlOptions);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<SpiMockSusDashboardService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Default camelCase policy for frontend compatibility
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactLocal", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
            (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("DashboardAccess", policy => policy.RequireRole("admin", "analista", "gestor"));
    options.AddPolicy("UserManagement", policy => policy.RequireRole("admin", "gestor"));
    options.AddPolicy("PatientAccess", policy => policy.RequireRole("admin", "agente_saude", "gestor"));
    options.AddPolicy("EvaluationAccess", policy => policy.RequireRole("admin", "agente_saude", "gestor"));
    options.AddPolicy("EvaluationManagement", policy => policy.RequireRole("admin", "gestor"));
    options.AddPolicy("FormAccess", policy => policy.RequireRole("admin", "analista", "agente_saude", "gestor"));
    options.AddPolicy("FormManagement", policy => policy.RequireRole("admin", "agente_saude", "gestor"));
    options.AddPolicy("SpecialistAccess", policy => policy.RequireRole("admin", "agente_saude", "gestor"));
    options.AddPolicy("SpecialistManagement", policy => policy.RequireRole("admin", "gestor"));
    options.AddPolicy("GroupAccess", policy => policy.RequireRole("admin", "agente_saude", "gestor"));
    options.AddPolicy("GroupManagement", policy => policy.RequireRole("admin", "gestor"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.TryStartFrontendDevServer();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("ReactLocal");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DatabaseInitializer.InitialiseAsync(app.Services);

app.Run();



