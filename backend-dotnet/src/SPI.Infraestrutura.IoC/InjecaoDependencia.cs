using SPI.Application.Configuration;
using SPI.Application.Interfaces.Email;
using SPI.Application.Interfaces.Seguranca;
using SPI.Application.Interfaces;
using SPI.Application.Services;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using SPI.Infrastructure.Data.Repositories;
using SPI.Infrastructure.Data.Security;
using SPI.Infrastructure.Data.Seed;
using SPI.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SPI.Infrastructure.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<DatabaseInitializationOptions>(configuration.GetSection(DatabaseInitializationOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<SPI.Application.Configuration.PasswordInviteOptions>(
            configuration.GetSection(SPI.Application.Configuration.PasswordInviteOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseConfiguredDatabase(configuration));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<ISpecialistRepository, SpecialistRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<SPI.Domain.Repositories.IOrganizationRepository, SPI.Infrastructure.Data.Repositories.OrganizationRepository>();

        services.AddScoped<PasswordHasherAdapter>();
        services.AddScoped<IPasswordHasher>(provider => provider.GetRequiredService<PasswordHasherAdapter>());
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordInviteTokenService, PasswordInviteTokenService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IGroupsAppService, GroupsAppService>();
        services.AddScoped<IFormsAppService, FormsAppService>();
        services.AddScoped<IDashboardAppService, DashboardAppService>();
        services.AddScoped<IProfileAppService, ProfileAppService>();
        services.AddScoped<IUsersAppService, UsersAppService>();
        services.AddScoped<IPatientsAppService, PatientsAppService>();
        services.AddScoped<ISpecialistsAppService, SpecialistsAppService>();
        services.AddScoped<IEvaluationsAppService, EvaluationsAppService>();

        return services;
    }
}





