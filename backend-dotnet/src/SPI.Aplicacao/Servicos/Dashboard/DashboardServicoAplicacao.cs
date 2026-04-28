using SPI.Application.DTOs.Dashboard;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

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

    public async Task<DashboardResponseDto> GetAsync(int actorUserId, CancellationToken cancellationToken = default)
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
            var forms = await _formRepository.ListAsync(cancellationToken);
            var groups = await _groupRepository.ListAsync(cancellationToken);

            return new DashboardResponseDto
            {
                TotalUsuarios = users.Count,
                TotalPacientes = patients.Count,
                TotalAvaliacoes = evaluations.Count,
                TotalFormularios = forms.Count,
                TotalGrupos = groups.Count,
                UltimasAvaliacoes = evaluations.Take(5).Select(x => x.ToDto()).ToArray()
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

        return new DashboardResponseDto
        {
            TotalUsuarios = scopedUsers.Count,
            TotalPacientes = scopedPatients.Count,
            TotalAvaliacoes = scopedEvaluations.Count,
            TotalFormularios = scopedForms.Count,
            TotalGrupos = scopedGroups.Count,
            UltimasAvaliacoes = scopedEvaluations.Take(5).Select(x => x.ToDto()).ToArray()
        };
    }
}



