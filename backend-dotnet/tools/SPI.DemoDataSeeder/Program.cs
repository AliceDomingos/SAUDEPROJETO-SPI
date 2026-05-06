using System.Globalization;
using System.Reflection;
using System.Text;
using SPI.Domain.Entities;
using SPI.Domain.Enums;
using SPI.Domain.ValueObjects;
using SPI.Infrastructure.Data.Persistence;
using SPI.Infrastructure.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var options = SeederOptions.Parse(args);
var configuration = LoadConfiguration(options.ApiPath, options.Environment);

await using var context = new AppDbContext(
    BuildDbContextOptions(configuration));

if (context.Database.IsSqlite())
{
    await context.Database.EnsureCreatedAsync();
}
else
{
    await context.Database.MigrateAsync();
}

var rows = CsvReader.Read(options.CsvPath);
if (rows.Count == 0)
{
    Console.WriteLine("Nenhuma linha encontrada no CSV.");
    return;
}

var hasher = new PasswordHasherAdapter();
var admin = await EnsureAdminAsync(context, configuration, hasher);
var organization = await EnsureOrganizationAsync(context, admin);
var manager = await EnsureUserAsync(context, "Gestor Demo", "gestor.demo@spi.com", UserRole.Manager, organization.Id, hasher);
var agent = await EnsureUserAsync(context, "Agente Demo", "agente.demo@spi.com", UserRole.HealthAgent, organization.Id, hasher);

await RemoveExistingDemoPatientsAsync(context);

var groups = await EnsureGroupsAsync(context, rows, manager.Id, agent.Id, organization.Id);
var specialists = await EnsureSpecialistsAsync(context, rows, organization.Id);

var patientCount = 0;
var evaluationCount = 0;
var referralCount = 0;

foreach (var row in rows)
{
    var group = groups[NormalizeKey(row.UbsOrigem)];
    var patient = new Patient(
        row.NomeCrianca,
        DemoCpf(patientCount + 1),
        row.DataNascimento ?? DateTime.UtcNow.Date.AddYears(-Math.Max(row.IdadeAnos ?? 6, 1)),
        NormalizeSex(row.Sexo),
        row.Telefone,
        null,
        row.Responsavel,
        null,
        "GO",
        "Goiania",
        row.Bairro,
        null,
        null,
        null,
        row.Observacoes,
        agent.Id,
        group.Id);

    patient.AssignOrganization(organization.Id);
    context.Patients.Add(patient);
    patientCount++;

    var evaluation = new Evaluation(
        patient.Id,
        agent.Id,
        group.Id,
        BuildAnswers(row.ScoreTriagem ?? 0),
        row.Observacoes);

    evaluation.AssignOrganization(organization.Id);
    SetPrivateProperty(evaluation, nameof(Evaluation.DataAvaliacao), row.DataTriagem ?? DateTime.UtcNow);
    SetPrivateProperty(evaluation, nameof(Evaluation.ScoreTotal), row.ScoreTriagem ?? 0m);
    SetPrivateProperty(evaluation, nameof(Evaluation.Classificacao), ClassificationFromRisk(row.NivelRisco, row.ScoreTriagem ?? 0m));
    context.Evaluations.Add(evaluation);
    evaluationCount++;

    if (IsYes(row.Encaminhado) && !string.IsNullOrWhiteSpace(row.EspecialistaDestino))
    {
        var specialist = specialists[NormalizeKey(row.EspecialistaDestino)];
        var referral = new EvaluationReferral(
            evaluation.Id,
            patient.Id,
            agent.Id,
            true,
            specialist.Id,
            specialist.Nome,
            specialist.Especialidade,
            specialist.CustoConsulta);

        referral.AssignOrganization(organization.Id);
        SetPrivateProperty(referral, nameof(EvaluationReferral.CriadoEm), row.DataEncaminhamento ?? row.DataTriagem ?? DateTime.UtcNow);
        context.EvaluationReferrals.Add(referral);
        referralCount++;
    }
}

await context.SaveChangesAsync();

Console.WriteLine("Dados demo criados com sucesso.");
Console.WriteLine($"Organizacao: {organization.Nome}");
Console.WriteLine($"Grupos: {groups.Count}");
Console.WriteLine($"Especialistas: {specialists.Count}");
Console.WriteLine($"Pacientes: {patientCount}");
Console.WriteLine($"Avaliacoes: {evaluationCount}");
Console.WriteLine($"Encaminhamentos: {referralCount}");

static IConfigurationRoot LoadConfiguration(string apiPath, string environment) =>
    new ConfigurationBuilder()
        .SetBasePath(apiPath)
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{environment}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

static DbContextOptions<AppDbContext> BuildDbContextOptions(IConfiguration configuration)
{
    var builder = new DbContextOptionsBuilder<AppDbContext>();
    builder.UseConfiguredDatabase(configuration);
    return builder.Options;
}

static async Task<User> EnsureAdminAsync(AppDbContext context, IConfiguration configuration, PasswordHasherAdapter hasher)
{
    var email = configuration["Seed:AdminEmail"] ?? "admin@spi.com";
    var admin = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (admin is not null)
    {
        return admin;
    }

    admin = new User(
        configuration["Seed:AdminName"] ?? "Administrador",
        new Email(email),
        hasher.Hash(configuration["Seed:AdminPassword"] ?? "admin123"),
        UserRole.Admin);

    context.Users.Add(admin);
    await context.SaveChangesAsync();
    return admin;
}

static async Task<Organization> EnsureOrganizationAsync(AppDbContext context, User admin)
{
    var organization = await context.Organizations.FirstOrDefaultAsync(x => x.AdminId == admin.Id);
    if (organization is null)
    {
        organization = new Organization("Organizacao Principal", admin.Id);
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();
    }

    if (admin.OrganizationId != organization.Id)
    {
        admin.AssignOrganization(organization.Id);
        await context.SaveChangesAsync();
    }

    return organization;
}

static async Task<User> EnsureUserAsync(
    AppDbContext context,
    string name,
    string email,
    UserRole role,
    Guid organizationId,
    PasswordHasherAdapter hasher)
{
    var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (user is null)
    {
        user = new User(name, new Email(email), hasher.Hash("demo123"), role);
        context.Users.Add(user);
    }

    if (user.OrganizationId != organizationId)
    {
        user.AssignOrganization(organizationId);
    }

    await context.SaveChangesAsync();
    return user;
}

static async Task RemoveExistingDemoPatientsAsync(AppDbContext context)
{
    var demoPatientIds = await context.Patients
        .Where(x => x.Cpf.StartsWith("900"))
        .Select(x => x.Id)
        .ToArrayAsync();

    if (demoPatientIds.Length == 0)
    {
        return;
    }

    var demoEvaluationIds = await context.Evaluations
        .Where(x => demoPatientIds.Contains(x.PatientId))
        .Select(x => x.Id)
        .ToArrayAsync();

    context.EvaluationReferrals.RemoveRange(context.EvaluationReferrals.Where(x => demoEvaluationIds.Contains(x.EvaluationId)));
    context.Evaluations.RemoveRange(context.Evaluations.Where(x => demoEvaluationIds.Contains(x.Id)));
    context.Patients.RemoveRange(context.Patients.Where(x => demoPatientIds.Contains(x.Id)));
    await context.SaveChangesAsync();
}

static async Task<Dictionary<string, Group>> EnsureGroupsAsync(
    AppDbContext context,
    IReadOnlyCollection<DemoRow> rows,
    Guid managerId,
    Guid agentId,
    Guid organizationId)
{
    var groupNames = rows
        .Select(x => x.UbsOrigem)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToArray();

    var groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
    foreach (var groupName in groupNames)
    {
        var group = await context.Groups.FirstOrDefaultAsync(x => x.Nome == groupName);
        if (group is null)
        {
            group = new Group(groupName, managerId);
            context.Groups.Add(group);
        }
        else
        {
            group.Update(groupName, managerId);
        }

        group.AssignOrganization(organizationId);
        groups[NormalizeKey(groupName)] = group;
    }

    await context.SaveChangesAsync();

    foreach (var group in groups.Values)
    {
        var hasMembership = await context.UserGroupMemberships.AnyAsync(x => x.UserId == agentId && x.GroupId == group.Id);
        if (!hasMembership)
        {
            context.UserGroupMemberships.Add(new UserGroupMembership(agentId, group.Id));
        }
    }

    await context.SaveChangesAsync();
    return groups;
}

static async Task<Dictionary<string, Specialist>> EnsureSpecialistsAsync(AppDbContext context, IReadOnlyCollection<DemoRow> rows, Guid organizationId)
{
    var costs = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Terapeuta Ocupacional"] = 280m,
        ["Fonoaudiologo"] = 220m,
        ["Fonoaudiólogo"] = 220m,
        ["Psiquiatra Infantil"] = 650m,
        ["Psicologo Infantil"] = 240m,
        ["Psicólogo Infantil"] = 240m,
        ["Neuropediatra"] = 750m
    };

    var specialties = rows
        .Where(x => IsYes(x.Encaminhado) && !string.IsNullOrWhiteSpace(x.EspecialistaDestino))
        .Select(x => x.EspecialistaDestino)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToArray();

    var specialists = new Dictionary<string, Specialist>(StringComparer.OrdinalIgnoreCase);
    foreach (var specialty in specialties)
    {
        var name = $"Equipe de {specialty}";
        var specialist = await context.Specialists.FirstOrDefaultAsync(x => x.Nome == name);
        var cost = costs.GetValueOrDefault(specialty, 300m);
        if (specialist is null)
        {
            specialist = new Specialist(name, specialty, cost);
            context.Specialists.Add(specialist);
        }
        else
        {
            specialist.Update(name, specialty, cost);
            specialist.Activate();
        }

        specialist.AssignOrganization(organizationId);
        specialists[NormalizeKey(specialty)] = specialist;
    }

    await context.SaveChangesAsync();
    return specialists;
}

static Dictionary<string, int> BuildAnswers(decimal targetScore)
{
    var answers = Enumerable.Range(1, 14).ToDictionary(x => x.ToString(CultureInfo.InvariantCulture), _ => 1);
    var remaining = Math.Clamp((int)Math.Round(targetScore, MidpointRounding.AwayFromZero) - 14, 0, 42);
    for (var question = 1; question <= 14 && remaining > 0; question++)
    {
        var add = Math.Min(3, remaining);
        answers[question.ToString(CultureInfo.InvariantCulture)] += add;
        remaining -= add;
    }

    return answers;
}

static string DemoCpf(int index) => $"900{index:00000000}";

static string NormalizeSex(string value) =>
    value.Trim().ToLowerInvariant() switch
    {
        "feminino" => "feminino",
        "masculino" => "masculino",
        _ => "outro"
    };

static string ClassificationFromRisk(string risk, decimal score)
{
    if (risk.Contains("severo", StringComparison.OrdinalIgnoreCase))
    {
        return "TEA Grave";
    }

    if (risk.Contains("leve", StringComparison.OrdinalIgnoreCase) ||
        risk.Contains("moderado", StringComparison.OrdinalIgnoreCase))
    {
        return "TEA Leve a Moderado";
    }

    return score >= 37m
        ? "TEA Grave"
        : score > 29.5m
            ? "TEA Leve a Moderado"
            : "Sem indicativo de TEA";
}

static bool IsYes(string value) =>
    value.Equals("sim", StringComparison.OrdinalIgnoreCase) ||
    value.Equals("s", StringComparison.OrdinalIgnoreCase) ||
    value.Equals("yes", StringComparison.OrdinalIgnoreCase);

static string NormalizeKey(string value) => value.Trim().ToUpperInvariant();

static void SetPrivateProperty<T>(object instance, string propertyName, T value)
{
    var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Propriedade '{propertyName}' nao encontrada em {instance.GetType().Name}.");
    property.SetValue(instance, value);
}

internal sealed record SeederOptions(string CsvPath, string ApiPath, string Environment)
{
    public static SeederOptions Parse(string[] args)
    {
        string? csvPath = null;
        string? apiPath = null;
        var environment = "Development";

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--csv":
                    csvPath = args[++index];
                    break;
                case "--api":
                    apiPath = args[++index];
                    break;
                case "--environment":
                    environment = args[++index];
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        {
            throw new FileNotFoundException("CSV de dados demo nao encontrado.", csvPath);
        }

        if (string.IsNullOrWhiteSpace(apiPath) || !Directory.Exists(apiPath))
        {
            throw new DirectoryNotFoundException("Pasta da API nao encontrada.");
        }

        return new SeederOptions(Path.GetFullPath(csvPath), Path.GetFullPath(apiPath), environment);
    }
}

internal sealed record DemoRow(
    string IdPaciente,
    string NomeCrianca,
    string Sexo,
    DateTime? DataNascimento,
    int? IdadeAnos,
    string Bairro,
    string UbsOrigem,
    string Responsavel,
    string Telefone,
    DateTime? DataTriagem,
    decimal? ScoreTriagem,
    string NivelRisco,
    string Encaminhado,
    string EspecialistaDestino,
    DateTime? DataEncaminhamento,
    string Observacoes);

internal static class CsvReader
{
    public static IReadOnlyCollection<DemoRow> Read(string csvPath)
    {
        using var reader = new StreamReader(csvPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [];
        }

        var headers = ParseCsvLine(headerLine).Select(x => x.Trim()).ToArray();
        var rows = new List<DemoRow>();
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Length; index++)
            {
                record[headers[index]] = index < values.Count ? values[index].Trim() : string.Empty;
            }

            rows.Add(new DemoRow(
                Get(record, "id_paciente"),
                Get(record, "nome_crianca"),
                Get(record, "sexo"),
                GetDate(record, "data_nascimento"),
                GetInt(record, "idade_anos"),
                Get(record, "bairro"),
                Get(record, "ubs_origem"),
                Get(record, "responsavel"),
                Get(record, "telefone"),
                GetDate(record, "data_triagem"),
                GetDecimal(record, "score_triagem"),
                Get(record, "nivel_risco"),
                Get(record, "encaminhado"),
                Get(record, "especialista_destino"),
                GetDate(record, "data_encaminhamento"),
                Get(record, "observacoes")));
        }

        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];
            if (current == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(current);
        }

        values.Add(value.ToString());
        return values;
    }

    private static string Get(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value.Trim() : string.Empty;

    private static int? GetInt(IReadOnlyDictionary<string, string> values, string key) =>
        int.TryParse(Get(values, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static decimal? GetDecimal(IReadOnlyDictionary<string, string> values, string key) =>
        decimal.TryParse(Get(values, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DateTime? GetDate(IReadOnlyDictionary<string, string> values, string key)
    {
        var raw = Get(values, key);
        return DateTime.TryParseExact(raw, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out var value)
            ? value
            : null;
    }
}
