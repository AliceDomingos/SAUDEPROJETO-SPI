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
    private readonly IGroupRepository _groupRepository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EvaluationsAppService(
        IEvaluationRepository evaluationRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository,
        IFormRepository formRepository,
        IGroupRepository groupRepository,
        ISpecialistRepository specialistRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _formRepository = formRepository;
        _groupRepository = groupRepository;
        _specialistRepository = specialistRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<EvaluationResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default)
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

    public async Task<EvaluationResponseDto?> GetByIdAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
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

    public async Task<EvaluationResponseDto> CreateAsync(CreateEvaluationRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanEvaluate())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para avaliar.");
        }

        var patient = await _patientRepository.GetByIdAsync(request.ResolvePatientId(), cancellationToken)
            ?? throw new KeyNotFoundException("Paciente nao encontrado.");

        EnsureCanUsePatient(actor, patient);

        Evaluation evaluation;
        FormTemplate? form = null;
        if (request.FormId.HasValue && request.FormId.Value != Guid.Empty)
        {
            form = await _formRepository.GetDetailedByIdAsync(request.FormId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Formulario nao encontrado.");

            if (form.GroupId.HasValue)
            {
                EnsureCanAccessGroup(actor, form.GroupId.Value, allowManagedOnly: false);
            }
        }

        var evaluationGroupId = await ResolveEvaluationGroupIdAsync(actor, request.GroupId, form?.GroupId, patient.GroupId, cancellationToken);

        if (form is not null)
        {
            evaluation = new Evaluation(
                patient.Id,
                actorUserId,
                evaluationGroupId,
                form.Id,
                request.Respostas,
                form.Questions.ToArray(),
                form.ClassificationRanges.ToArray());
                request.Observacoes);
        }
        else
        {
            evaluation = new Evaluation(patient.Id, actorUserId, evaluationGroupId, request.Respostas, request.Observacoes);
        }

        if (actor.OrganizationId.HasValue)
        {
            evaluation.AssignOrganization(actor.OrganizationId.Value);
        }
        else if (patient.OrganizationId.HasValue)
        {
            evaluation.AssignOrganization(patient.OrganizationId.Value);
        }

        await _evaluationRepository.AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _evaluationRepository.GetDetailedByIdAsync(evaluation.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel recuperar a avaliacao apos salvar.");

        return created.ToDto();
    }

    public async Task<EvaluationReferralResponseDto> SaveReferralAsync(
        Guid id,
        SaveEvaluationReferralRequestDto request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanEvaluate())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para registrar encaminhamento.");
        }

        var evaluation = await _evaluationRepository.GetByIdWithReferralAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Avaliacao nao encontrada.");

        EnsureCanAccessGroup(actor, evaluation.GroupId, allowManagedOnly: false);

        Specialist? specialist = null;
        if (request.Encaminhado)
        {
            if (!request.SpecialistId.HasValue || request.SpecialistId.Value == Guid.Empty)
            {
                throw new InvalidOperationException("Selecione o especialista do encaminhamento.");
            }

            specialist = await _specialistRepository.GetByIdAsync(request.SpecialistId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Especialista nao encontrado.");

            if (!specialist.Ativo)
            {
                throw new InvalidOperationException("Especialista inativo nao pode receber encaminhamento.");
            }

            if (actor.OrganizationId.HasValue && specialist.OrganizationId.HasValue && actor.OrganizationId.Value != specialist.OrganizationId.Value)
            {
                throw new UnauthorizedAccessException("Especialista pertence a outra organizacao.");
            }
        }

        EvaluationReferral referral;
        if (evaluation.Referral is null)
        {
            referral = new EvaluationReferral(
                evaluation.Id,
                evaluation.PatientId,
                actorUserId,
                request.Encaminhado,
                specialist?.Id,
                specialist?.Nome,
                specialist?.Especialidade,
                specialist?.CustoConsulta ?? 0);

            if (evaluation.OrganizationId.HasValue)
            {
                referral.AssignOrganization(evaluation.OrganizationId.Value);
            }

            await _evaluationRepository.AddReferralAsync(referral, cancellationToken);
        }
        else
        {
            referral = evaluation.Referral;
            referral.UpdateDecision(
                request.Encaminhado,
                specialist?.Id,
                specialist?.Nome,
                specialist?.Especialidade,
                specialist?.CustoConsulta ?? 0);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return referral.ToDto();
    }

    public async Task DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
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

    public async Task<EvaluationStatsResponseDto> GetStatsAsync(Guid actorUserId, CancellationToken cancellationToken = default)
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
                SemIndicativo = list.Count(x => x.Classificacao == "Sem indicativo de TEA"),
                TeaLeveModerado = list.Count(x => x.Classificacao == "TEA Leve a Moderado"),
                TeaGrave = list.Count(x => x.Classificacao == "TEA Grave")
            },
            RecentEvaluations = list.Take(5).ToList()
        };
    }

    public async Task<ExportFileResultDto> ExportExcelAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var evaluation = await GetRequiredEvaluationAsync(id, actorUserId, cancellationToken);
        return EvaluationExportBuilder.BuildCsvFile(evaluation);
    }

    public async Task<ExportFileResultDto> ExportPdfAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var evaluation = await GetRequiredEvaluationAsync(id, actorUserId, cancellationToken);
        return EvaluationExportBuilder.BuildPdfFile(evaluation);
    }

    private async Task<User> GetActorAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Ativo)
        {
            throw new UnauthorizedAccessException("Usuario desativado.");
        }

        return actor;
    }

    private async Task<EvaluationResponseDto> GetRequiredEvaluationAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var evaluation = await GetByIdAsync(id, actorUserId, cancellationToken);
        return evaluation ?? throw new KeyNotFoundException("Avaliacao nao encontrada.");
    }

    private static void EnsureCanAccessGroup(User actor, Guid groupId, bool allowManagedOnly)
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

    private static void EnsureCanUsePatient(User actor, Patient patient)
    {
        if (actor.Role == UserRole.Admin)
        {
            return;
        }

        if (actor.OrganizationId.HasValue && patient.OrganizationId.HasValue && actor.OrganizationId.Value != patient.OrganizationId.Value)
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para usar este paciente.");
        }
    }

    private async Task<Guid> ResolveEvaluationGroupIdAsync(
        User actor,
        Guid? requestedGroupId,
        Guid? formGroupId,
        Guid patientGroupId,
        CancellationToken cancellationToken)
    {
        var accessScope = AccessScopeResolver.Resolve(actor);
        var groupId = requestedGroupId.GetValueOrDefault() != Guid.Empty
            ? requestedGroupId!.Value
            : formGroupId.GetValueOrDefault() != Guid.Empty
                ? formGroupId!.Value
                : accessScope.OperationalGroupIds.Count == 1
                    ? accessScope.OperationalGroupIds.Single()
                    : patientGroupId;

        if (!accessScope.IsAdmin && !accessScope.OperationalGroupIds.Contains(groupId))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para registrar avaliacao neste grupo.");
        }

        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo da avaliacao nao encontrado.");

        if (actor.OrganizationId.HasValue && group.OrganizationId.HasValue && actor.OrganizationId.Value != group.OrganizationId.Value)
        {
            throw new UnauthorizedAccessException("Grupo da avaliacao pertence a outra organizacao.");
        }

        return group.Id;
    }

}



