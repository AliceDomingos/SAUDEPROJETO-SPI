using SPI.Application.Interfaces;
using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroupMembership> UserGroupMemberships => Set<UserGroupMembership>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormQuestion> FormQuestions => Set<FormQuestion>();
    public DbSet<FormQuestionOption> FormQuestionOptions => Set<FormQuestionOption>();
    public DbSet<FormClassificationRange> FormClassificationRanges => Set<FormClassificationRange>();
    public DbSet<Specialist> Specialists => Set<Specialist>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<EvaluationReferral> EvaluationReferrals => Set<EvaluationReferral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}


