using SPI.Application.DTOs.Dashboard;
using SPI.Application.DTOs.Groups;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;
using SPI.Domain.ReadModels;
using System.Globalization;

namespace SPI.Application.Services;

public sealed class DashboardAppService : IDashboardAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IFormRepository _formRepository;
    private readonly IGroupRepository _groupRepository;

    public DashboardAppService(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IEvaluationRepository evaluationRepository,
        IFormRepository formRepository,
        IGroupRepository groupRepository)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _evaluationRepository = evaluationRepository;
        _formRepository = formRepository;
        _groupRepository = groupRepository;
    }

    public async Task<DashboardResponseDto> GetAsync(
        Guid actorUserId,
        string? risco = null,
        string? especialista = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        Guid? grupoId = null,
        CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanViewDashboard())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para visualizar dashboard.");
        }

        if (actor.Role == UserRole.Analyst)
        {
            var users = await _userRepository.ListAsync(cancellationToken);
            var patients = await _patientRepository.ListAsync(cancellationToken);
            var evaluations = await _evaluationRepository.ListDetailedAsync(cancellationToken);
            var filteredEvaluations = ApplyFilters(evaluations, risco, especialista, dataInicio, dataFim, grupoId);
            var forms = await _formRepository.ListAsync(cancellationToken);
            var groups = await _groupRepository.ListAsync(cancellationToken);

            return new DashboardResponseDto
            {
                TotalUsuarios = users.Count,
                TotalPacientes = patients.Count,
                TotalAvaliacoes = filteredEvaluations.Count,
                TotalFormularios = forms.Count,
                TotalGrupos = groups.Count,
                Triagens = BuildTriageSummary(filteredEvaluations, CountPatientsForSummary(filteredEvaluations, patients.Count)),
                UltimasAvaliacoes = []
            };
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        List<SPI.Domain.Entities.User> scopedUsers;
        List<SPI.Domain.Entities.Patient> scopedPatients;
        List<SPI.Domain.ReadModels.EvaluationDetails> scopedEvaluations;
        List<SPI.Domain.Entities.FormTemplate> scopedForms;
        List<SPI.Domain.Entities.Group> scopedGroups;

        if (accessScope.IsAdmin && accessScope.OrganizationId.HasValue)
        {
            var orgId = accessScope.OrganizationId.Value;
            scopedUsers = await _userRepository.ListByOrganizationIdAsync(orgId, cancellationToken);
            scopedPatients = await _patientRepository.ListByOrganizationIdAsync(orgId, cancellationToken);
            scopedEvaluations = await _evaluationRepository.ListDetailedByOrganizationIdAsync(orgId, cancellationToken);
            scopedForms = await _formRepository.ListByOrganizationIdAsync(orgId, cancellationToken);
            scopedGroups = await _groupRepository.ListByOrganizationIdAsync(orgId, cancellationToken);
        }
        else
        {
            scopedUsers = await _userRepository.ListByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
            scopedPatients = await _patientRepository.ListByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
            scopedEvaluations = await _evaluationRepository.ListDetailedByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
            scopedForms = await _formRepository.ListByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
            scopedGroups = await _groupRepository.ListByIdsAsync(
                actor.Role.HasManagerPrivileges() ? accessScope.ManagedGroupIds : accessScope.OperationalGroupIds,
                cancellationToken);
        }

        var filteredScopedEvaluations = ApplyFilters(scopedEvaluations, risco, especialista, dataInicio, dataFim, grupoId);

        return new DashboardResponseDto
        {
            TotalUsuarios = scopedUsers.Count,
            TotalPacientes = scopedPatients.Count,
            TotalAvaliacoes = filteredScopedEvaluations.Count,
            TotalFormularios = scopedForms.Count,
            TotalGrupos = scopedGroups.Count,
            Triagens = BuildTriageSummary(filteredScopedEvaluations, CountPatientsForSummary(filteredScopedEvaluations, scopedPatients.Count)),
            UltimasAvaliacoes = filteredScopedEvaluations.Take(5).Select(x => x.ToDto()).ToArray()
        };
    }

    public async Task<IReadOnlyCollection<GroupResponseDto>> ListFilterGroupsAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (actor.Role != UserRole.Analyst)
        {
            return [];
        }

        var groups = actor.OrganizationId.HasValue
            ? await _groupRepository.ListByOrganizationIdAsync(actor.OrganizationId.Value, cancellationToken)
            : await _groupRepository.ListAsync(cancellationToken);

        return groups
            .Where(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => x.ToDto())
            .ToArray();
    }

    private static IReadOnlyCollection<EvaluationDetails> ApplyFilters(
        IEnumerable<EvaluationDetails> evaluations,
        string? risco,
        string? especialista,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? grupoId)
    {
        var query = evaluations;

        if (grupoId.HasValue && grupoId.Value != Guid.Empty)
        {
            query = query.Where(x => x.GroupId == grupoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(risco))
        {
            query = query.Where(x => string.Equals(RiskLabel(x), risco.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(especialista))
        {
            query = query.Where(x => x.Referral?.Encaminhado == true && string.Equals(x.Referral.Especialidade, especialista.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(x => x.DataAvaliacao.Date >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(x => x.DataAvaliacao.Date <= dataFim.Value.Date);
        }

        return query.ToArray();
    }

    private static int CountPatientsForSummary(IReadOnlyCollection<EvaluationDetails> evaluations, int fallbackTotal) =>
        evaluations.Count == 0 ? fallbackTotal : evaluations.Select(x => x.PatientId).Distinct().Count();

    private static DashboardTriageSummaryDto BuildTriageSummary(IReadOnlyCollection<EvaluationDetails> evaluations, int totalPatients)
    {
        var scores = evaluations.Select(x => x.ScoreTotal).ToArray();
        var encaminhados = evaluations.Count(x => x.Referral?.Encaminhado == true);
        var semEncaminhamento = evaluations.Count(x => x.Referral is not null && !x.Referral.Encaminhado);
        var custoTotalEncaminhamentos = evaluations
            .Where(x => x.Referral?.Encaminhado == true)
            .Sum(x => x.Referral?.CustoEstimado ?? 0m);

        return new DashboardTriageSummaryDto
        {
            TotalPacientes = totalPatients,
            TotalTriagens = evaluations.Count,
            ScoreMedio = scores.Length == 0 ? 0 : Math.Round(scores.Average(), 2),
            MenorScore = scores.Length == 0 ? 0 : scores.Min(),
            MaiorScore = scores.Length == 0 ? 0 : scores.Max(),
            Encaminhados = encaminhados,
            ConsultasEvitadas = semEncaminhamento,
            EconomiaFinanceiraEstimada = 0,
            CustoTotalEncaminhamentos = custoTotalEncaminhamentos,
            CasosSeveros = evaluations.Count(x => RiskLabel(x) == "Severo"),
            TaxaEncaminhamento = evaluations.Count == 0 ? 0 : Math.Round(encaminhados * 100m / evaluations.Count, 1),
            DistribuicaoTriagensMensais = MonthlyDistribution(evaluations),
            DistribuicaoRisco = Distribution(evaluations, RiskLabel, ["Severo", "Moderado", "Leve", "Sem Sinais"]),
            DistribuicaoEspecialista = Distribution(
                    evaluations.Where(x => x.Referral?.Encaminhado == true && !string.IsNullOrWhiteSpace(x.Referral.Especialidade)),
                    x => x.Referral?.Especialidade ?? string.Empty)
                .Take(6)
                .ToArray()
        };
    }

    private static IReadOnlyCollection<DashboardDistributionItemDto> MonthlyDistribution(IEnumerable<EvaluationDetails> evaluations)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        return evaluations
            .GroupBy(x => new DateTime(x.DataAvaliacao.Year, x.DataAvaliacao.Month, 1))
            .OrderBy(x => x.Key)
            .Select(group => new DashboardDistributionItemDto
            {
                Label = culture.TextInfo.ToTitleCase(group.Key.ToString("MMM/yy", culture).Replace(".", string.Empty)),
                Value = group.Count()
            })
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardDistributionItemDto> Distribution(
        IEnumerable<EvaluationDetails> evaluations,
        Func<EvaluationDetails, string> selector,
        IReadOnlyCollection<string>? preferredOrder = null)
    {
        var items = evaluations
            .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .Select(group => new DashboardDistributionItemDto { Label = group.Key, Value = group.Count() })
            .ToArray();

        if (preferredOrder is null)
        {
            return items.OrderByDescending(x => x.Value).ThenBy(x => x.Label).ToArray();
        }

        return items
            .OrderBy(x =>
            {
                var index = preferredOrder
                    .Select((label, order) => new { label, order })
                    .FirstOrDefault(item => string.Equals(item.label, x.Label, StringComparison.OrdinalIgnoreCase))
                    ?.order;
                return index ?? int.MaxValue;
            })
            .ThenByDescending(x => x.Value)
            .ToArray();
    }

    private static string RiskLabel(EvaluationDetails evaluation)
    {
        var classification = evaluation.Classificacao.ToLowerInvariant();
        if (classification.Contains("grave"))
        {
            return "Severo";
        }

        if (classification.Contains("moderado"))
        {
            return "Moderado";
        }

        if (classification.Contains("leve"))
        {
            return "Leve";
        }

        if (classification.Contains("sem"))
        {
            return "Sem Sinais";
        }

        if (evaluation.ScoreTotal >= 37)
        {
            return "Severo";
        }

        return evaluation.ScoreTotal > 29.5m ? "Moderado" : "Sem Sinais";
    }
}
