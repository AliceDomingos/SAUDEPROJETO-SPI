using SPI.Application.DTOs.Evaluations;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Entities;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

namespace SPI.Application.Services;

public sealed class EvaluationsAppService : IEvaluationsAppService
{
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EvaluationsAppService(
        IEvaluationRepository evaluationRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository,
        IFormRepository formRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<EvaluationResponseDto>> ListAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanViewEvaluations())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar avaliacoes.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        List<SPI.Domain.ReadModels.EvaluationDetails> evaluations;
        if (accessScope.IsAdmin && accessScope.OrganizationId.HasValue)
        {
            evaluations = await _evaluationRepository.ListDetailedByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken);
        }
        else
        {
            evaluations = await _evaluationRepository.ListDetailedByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
        }

        return evaluations.Select(x => x.ToDto()).ToList();
    }

    public async Task<EvaluationResponseDto?> GetByIdAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanViewEvaluations())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar avaliacoes.");
        }

        var evaluation = await _evaluationRepository.GetDetailedByIdAsync(id, cancellationToken);
        if (evaluation is null)
        {
            return null;
        }

        EnsureCanAccessGroup(actor, evaluation.GroupId, allowManagedOnly: false);
        return evaluation.ToDto();
    }

    public async Task<EvaluationResponseDto> CreateAsync(CreateEvaluationRequestDto request, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanEvaluate())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para avaliar.");
        }

        var patient = await _patientRepository.GetByIdAsync(request.ResolvePatientId(), cancellationToken)
            ?? throw new KeyNotFoundException("Paciente nao encontrado.");

        EnsureCanAccessGroup(actor, patient.GroupId, allowManagedOnly: false);

        Evaluation evaluation;
        if (request.FormId.HasValue && request.FormId.Value > 0)
        {
            var form = await _formRepository.GetDetailedByIdAsync(request.FormId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Formulario nao encontrado.");

            if (form.GroupId.HasValue)
            {
                EnsureCanAccessGroup(actor, form.GroupId.Value, allowManagedOnly: false);
            }

            evaluation = new Evaluation(
                patient.Id,
                actorUserId,
                patient.GroupId,
                form.Id,
                request.Respostas,
                form.Questions.ToArray());
        }
        else
        {
            evaluation = new Evaluation(patient.Id, actorUserId, patient.GroupId, request.Respostas);
        }

        if (actor.Role == UserRole.Admin && actor.OrganizationId.HasValue)
        {
            evaluation.AssignOrganization(actor.OrganizationId.Value);
        }

        await _evaluationRepository.AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _evaluationRepository.GetDetailedByIdAsync(evaluation.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel recuperar a avaliacao apos salvar.");

        return created.ToDto();
    }

    public async Task DeleteAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (actor.Role != UserRole.Admin && !actor.Role.HasManagerPrivileges())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para excluir avaliacoes.");
        }

        var evaluation = await _evaluationRepository.GetByIdWithRelationsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Avaliacao nao encontrada.");

        EnsureCanAccessGroup(actor, evaluation.GroupId, allowManagedOnly: actor.Role.HasManagerPrivileges());

        _evaluationRepository.Remove(evaluation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<EvaluationStatsResponseDto> GetStatsAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        var evaluations = await ListAsync(actorUserId, cancellationToken);
        var list = evaluations.OrderByDescending(x => x.DataAvaliacao).ToList();

        return new EvaluationStatsResponseDto
        {
            Total = list.Count,
            AverageScore = list.Count == 0 ? 0 : Math.Round(list.Average(x => x.ScoreTotal), 2),
            LastMonth = list.Count(x => x.DataAvaliacao >= DateTime.UtcNow.AddMonths(-1)),
            ClassificationDistribution = new ClassificationDistributionDto
            {
                SemIndicativo = list.Count(x => x.Classificacao == "Sem indicativo"),
                TeaLeveModerado = list.Count(x => x.Classificacao == "TEA leve/moderado"),
                TeaGrave = list.Count(x => x.Classificacao == "TEA grave" || x.Classificacao == "formulario")
            },
            RecentEvaluations = list.Take(5).ToList()
        };
    }

    public async Task<ExportFileResultDto> ExportExcelAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
    {
        var evaluation = await GetRequiredEvaluationAsync(id, actorUserId, cancellationToken);
        return EvaluationExportBuilder.BuildCsvFile(evaluation);
    }

    public async Task<ExportFileResultDto> ExportPdfAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
    {
        var evaluation = await GetRequiredEvaluationAsync(id, actorUserId, cancellationToken);
        return EvaluationExportBuilder.BuildPdfFile(evaluation);
    }

    private async Task<User> GetActorAsync(int actorUserId, CancellationToken cancellationToken)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Ativo)
        {
            throw new UnauthorizedAccessException("Usuario desativado.");
        }

        return actor;
    }

    private async Task<EvaluationResponseDto> GetRequiredEvaluationAsync(int id, int actorUserId, CancellationToken cancellationToken)
    {
        var evaluation = await GetByIdAsync(id, actorUserId, cancellationToken);
        return evaluation ?? throw new KeyNotFoundException("Avaliacao nao encontrada.");
    }

    private static void EnsureCanAccessGroup(User actor, int groupId, bool allowManagedOnly)
    {
        var accessScope = AccessScopeResolver.Resolve(actor);
        if (accessScope.IsAdmin)
        {
            return;
        }

        var allowedGroupIds = allowManagedOnly
            ? accessScope.ManagedGroupIds
            : accessScope.OperationalGroupIds;

        if (!allowedGroupIds.Contains(groupId))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar este grupo.");
        }
    }
}



