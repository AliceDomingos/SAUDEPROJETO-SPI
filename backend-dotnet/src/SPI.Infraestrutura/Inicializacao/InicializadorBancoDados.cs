using SPI.Domain.Entities;
using SPI.Domain.Enums;
using SPI.Domain.ValueObjects;
using SPI.Infrastructure.Data.Persistence;
using SPI.Infrastructure.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;

namespace SPI.Infrastructure.Data.Seed;

public static class DatabaseInitializer
{
    public static async Task InitialiseAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
        var databaseInitializationOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseInitializationOptions>>().Value;
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasherAdapter>();

        if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await EnsureSqlitePatientColumnsAsync(context, cancellationToken);
            await EnsureSqliteOrganizationColumnsAsync(context, cancellationToken);
            await EnsureSqliteFormQuestionOptionsTableAsync(context, cancellationToken);
            await EnsureSqliteFormClassificationRangesTableAsync(context, cancellationToken);
            await EnsureSqliteSpecialistTableAsync(context, cancellationToken);
            await EnsureSqliteEvaluationReferralTableAsync(context, cancellationToken);
        }
        else if (databaseInitializationOptions.ApplyMigrationsOnStartup)
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        if (!seedOptions.Enabled)
        {
            return;
        }

        var adminEmail = seedOptions.AdminEmail.Trim().ToLowerInvariant();
        var admin = await context.Users.FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);
        if (admin is null)
        {
            admin = new User(
                seedOptions.AdminName,
                new SPI.Domain.ValueObjects.Email(seedOptions.AdminEmail),
                passwordHasher.Hash(seedOptions.AdminPassword),
                UserRole.Admin);

            context.Users.Add(admin);
            await context.SaveChangesAsync(cancellationToken);
        }

        await EnsureSeedAdminOrganizationAsync(context, admin, cancellationToken);
    }

    private static async Task EnsureSqlitePatientColumnsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('patients');";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            existingColumns.Add(reader.GetString(1));
        }

        await reader.CloseAsync();

        var alterCommands = new[]
        {
            ("cpf", "ALTER TABLE patients ADD COLUMN cpf TEXT NOT NULL DEFAULT '';"),
            ("data_nascimento", "ALTER TABLE patients ADD COLUMN data_nascimento TEXT NOT NULL DEFAULT '2000-01-01';"),
            ("sexo", "ALTER TABLE patients ADD COLUMN sexo TEXT NOT NULL DEFAULT 'outro';"),
            ("nome_responsavel", "ALTER TABLE patients ADD COLUMN nome_responsavel TEXT NULL;"),
            ("telefone", "ALTER TABLE patients ADD COLUMN telefone TEXT NULL;"),
            ("email", "ALTER TABLE patients ADD COLUMN email TEXT NULL;"),
            ("endereco", "ALTER TABLE patients ADD COLUMN endereco TEXT NULL;"),
            ("cep", "ALTER TABLE patients ADD COLUMN cep TEXT NULL;"),
            ("estado", "ALTER TABLE patients ADD COLUMN estado TEXT NULL;"),
            ("cidade", "ALTER TABLE patients ADD COLUMN cidade TEXT NULL;"),
            ("bairro", "ALTER TABLE patients ADD COLUMN bairro TEXT NULL;"),
            ("rua", "ALTER TABLE patients ADD COLUMN rua TEXT NULL;"),
            ("numero", "ALTER TABLE patients ADD COLUMN numero TEXT NULL;"),
            ("complemento", "ALTER TABLE patients ADD COLUMN complemento TEXT NULL;"),
            ("observacoes", "ALTER TABLE patients ADD COLUMN observacoes TEXT NULL;"),
        };

        foreach (var (columnName, sql) in alterCommands)
        {
            if (existingColumns.Contains(columnName))
            {
                continue;
            }

            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        var legacyColumnsToDrop = new[]
        {
            "documentos",
            "historico"
        };

        foreach (var columnName in legacyColumnsToDrop)
        {
            if (!existingColumns.Contains(columnName))
            {
                continue;
            }

            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE patients DROP COLUMN {columnName};", cancellationToken);
        }
    }

    private static async Task EnsureSqliteOrganizationColumnsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var tables = new[]
        {
            ("users", "organization_id", "ALTER TABLE users ADD COLUMN organization_id TEXT NULL;"),
            ("groups", "organization_id", "ALTER TABLE groups ADD COLUMN organization_id TEXT NULL;"),
            ("patients", "organization_id", "ALTER TABLE patients ADD COLUMN organization_id TEXT NULL;"),
            ("evaluations", "organization_id", "ALTER TABLE evaluations ADD COLUMN organization_id TEXT NULL;"),
            ("evaluations", "observacoes", "ALTER TABLE evaluations ADD COLUMN observacoes TEXT NULL;"),
            ("form_templates", "organization_id", "ALTER TABLE form_templates ADD COLUMN organization_id TEXT NULL;"),
        };

        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        foreach (var (table, column, sql) in tables)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info('{table}');";
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(1));
            }
            await reader.CloseAsync();

            if (!columns.Contains(column))
            {
                await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
        }
    }

    private static async Task EnsureSqliteFormQuestionOptionsTableAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='form_question_options';";
        var exists = await checkCmd.ExecuteScalarAsync(cancellationToken);

        if (exists is null)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE form_question_options (
                    id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    form_question_id INTEGER NOT NULL,
                    valor            INTEGER NOT NULL,
                    descricao        TEXT    NOT NULL,
                    FOREIGN KEY (form_question_id) REFERENCES form_questions(id) ON DELETE CASCADE
                );
                CREATE INDEX IX_form_question_options_form_question_id
                    ON form_question_options(form_question_id);
                """,
                cancellationToken);
        }
    }

    private static async Task EnsureSqliteFormClassificationRangesTableAsync(AppDbContext context, CancellationToken cancellationToken)
    private static async Task EnsureSqliteEvaluationReferralTableAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS evaluation_referrals (
                id TEXT NOT NULL CONSTRAINT PK_evaluation_referrals PRIMARY KEY,
                evaluation_id TEXT NOT NULL,
                patient_id TEXT NOT NULL,
                encaminhado INTEGER NOT NULL,
                specialist_id TEXT NULL,
                specialist_nome TEXT NULL,
                especialidade TEXT NULL,
                custo_estimado TEXT NOT NULL DEFAULT '0',
                criado_em TEXT NOT NULL,
                criado_por_usuario_id TEXT NOT NULL,
                organization_id TEXT NULL,
                CONSTRAINT FK_evaluation_referrals_evaluations_evaluation_id FOREIGN KEY (evaluation_id) REFERENCES evaluations(id) ON DELETE CASCADE,
                CONSTRAINT FK_evaluation_referrals_patients_patient_id FOREIGN KEY (patient_id) REFERENCES patients(id),
                CONSTRAINT FK_evaluation_referrals_specialists_specialist_id FOREIGN KEY (specialist_id) REFERENCES specialists(id),
                CONSTRAINT FK_evaluation_referrals_users_criado_por_usuario_id FOREIGN KEY (criado_por_usuario_id) REFERENCES users(id),
                CONSTRAINT FK_evaluation_referrals_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES organizations(id)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS IX_evaluation_referrals_evaluation_id ON evaluation_referrals(evaluation_id);
            CREATE INDEX IF NOT EXISTS IX_evaluation_referrals_patient_id ON evaluation_referrals(patient_id);
            CREATE INDEX IF NOT EXISTS IX_evaluation_referrals_specialist_id ON evaluation_referrals(specialist_id);
            CREATE INDEX IF NOT EXISTS IX_evaluation_referrals_especialidade ON evaluation_referrals(especialidade);
            """;

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureSqliteColumnsAsync(
            context,
            "evaluation_referrals",
            [
                ("specialist_id", "ALTER TABLE evaluation_referrals ADD COLUMN specialist_id TEXT NULL;"),
                ("specialist_nome", "ALTER TABLE evaluation_referrals ADD COLUMN specialist_nome TEXT NULL;")
            ],
            cancellationToken);
    }

    private static async Task EnsureSqliteSpecialistTableAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS specialists (
                id TEXT NOT NULL CONSTRAINT PK_specialists PRIMARY KEY,
                nome TEXT NOT NULL,
                especialidade TEXT NOT NULL,
                custo_consulta TEXT NOT NULL,
                ativo INTEGER NOT NULL DEFAULT 1,
                criado_em TEXT NOT NULL,
                organization_id TEXT NULL,
                CONSTRAINT FK_specialists_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES organizations(id)
            );

            CREATE INDEX IF NOT EXISTS IX_specialists_nome ON specialists(nome);
            CREATE INDEX IF NOT EXISTS IX_specialists_especialidade ON specialists(especialidade);
            CREATE INDEX IF NOT EXISTS IX_specialists_organization_id ON specialists(organization_id);
            """;

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static async Task EnsureSqliteColumnsAsync(
        AppDbContext context,
        string table,
        IReadOnlyCollection<(string Column, string Sql)> columns,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='form_classification_ranges';";
        var exists = await checkCmd.ExecuteScalarAsync(cancellationToken);

        if (exists is null)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE form_classification_ranges (
                    id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    form_template_id INTEGER NOT NULL,
                    score_min        REAL NOT NULL,
                    score_max        REAL NOT NULL,
                    rotulo           TEXT NOT NULL,
                    FOREIGN KEY (form_template_id) REFERENCES form_templates(id) ON DELETE CASCADE
                );
                CREATE INDEX IX_form_classification_ranges_form_template_id
                    ON form_classification_ranges(form_template_id);
                """,
                cancellationToken);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info('{table}');";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            existingColumns.Add(reader.GetString(1));
        }
        await reader.CloseAsync();

        foreach (var (column, sql) in columns)
        {
            if (!existingColumns.Contains(column))
            {
                await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
        }
    }

    private static async Task EnsureSeedAdminOrganizationAsync(
        AppDbContext context,
        User admin,
        CancellationToken cancellationToken)
    {
        var org = await context.Organizations
            .FirstOrDefaultAsync(x => x.AdminId == admin.Id, cancellationToken);

        if (org is null)
        {
            org = new Organization("Organizacao Principal", admin.Id);
            context.Organizations.Add(org);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (admin.OrganizationId != org.Id)
        {
            admin.AssignOrganization(org.Id);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}



