using SPI.Application.DTOs.Patients;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

using SPI.Application.Config;

namespace SPI.Application.Services;

public sealed class PatientsAppService : IPatientsAppService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PatientsAppService(
        IPatientRepository patientRepository,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        IEvaluationRepository evaluationRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _evaluationRepository = evaluationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<PatientResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        return await ListReusableAsync(actorUserId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PatientResponseDto>> ListReusableAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanViewPatients())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar pacientes.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        List<SPI.Domain.Entities.Patient> patients;
        if (accessScope.OrganizationId.HasValue)
        {
            patients = await _patientRepository.ListReusableByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken);
        }
        else if (accessScope.IsAdmin)
        {
            patients = await _patientRepository.ListAsync(cancellationToken);
        }
        else
        {
            patients = await _patientRepository.ListAsync(cancellationToken);
        }

        return patients.Select(x => x.ToDto()).ToList();
    }

    public async Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManagePatients())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para cadastrar pacientes.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        var groupId = ResolveGroupId(request.GroupId, actor.Role, accessScope);
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo nao encontrado.");

        var patient = new SPI.Domain.Entities.Patient(
            request.Nome,
            request.Cpf,
            request.DataNascimento?.Date ?? default,
            request.Sexo,
            request.Telefone,
            request.Email,
            request.NomeResponsavel,
            request.Cep,
            request.Estado,
            request.Cidade,
            request.Bairro,
            request.Rua,
            request.Numero,
            request.Complemento,
            request.Observacoes,
            actorUserId,
            group.Id);
        if (accessScope.OrganizationId.HasValue)
        {
            patient.AssignOrganization(accessScope.OrganizationId.Value);
        }

        await _patientRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdPatient = await _patientRepository.GetByIdAsync(patient.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel carregar o paciente criado.");

        return createdPatient.ToDto();
    }

    public async Task<PatientResponseDto> UpdateAsync(Guid id, UpdatePatientRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManagePatients())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para editar pacientes.");
        }

        var patient = await _patientRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente nao encontrado.");

        var accessScope = AccessScopeResolver.Resolve(actor);
        if (!accessScope.IsAdmin && !accessScope.OperationalGroupIds.Contains(patient.GroupId))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para editar este paciente.");
        }

        var groupId = ResolveExistingOrRequestedGroupId(request.GroupId, patient.GroupId, actor.Role, accessScope);
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo nao encontrado.");

        patient.UpdateDetails(
            request.Nome,
            request.Cpf,
            request.DataNascimento?.Date ?? default,
            request.Sexo,
            request.Telefone,
            request.Email,
            request.NomeResponsavel,
            request.Cep,
            request.Estado,
            request.Cidade,
            request.Bairro,
            request.Rua,
            request.Numero,
            request.Complemento,
            request.Observacoes,
            group.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return patient.ToDto();
    }

    public async Task DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanAccessOperationalModules())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para excluir pacientes.");
        }

        var patient = await _patientRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente nao encontrado.");

        var accessScope = AccessScopeResolver.Resolve(actor);
        if (actor.Role != UserRole.Admin && !accessScope.OperationalGroupIds.Contains(patient.GroupId))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para excluir este paciente.");
        }

        if (await _evaluationRepository.AnyByPatientIdAsync(patient.Id, cancellationToken))
        {
            throw new InvalidOperationException("Nao e possivel excluir paciente com avaliacoes vinculadas.");
        }

        _patientRepository.Remove(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Guid ResolveGroupId(Guid? requestGroupId, UserRole actorRole, AccessScope accessScope)
    {
        if (requestGroupId.HasValue && requestGroupId.Value != Guid.Empty)
        {
            if (!accessScope.IsAdmin && !accessScope.OperationalGroupIds.Contains(requestGroupId.Value))
            {
                throw new UnauthorizedAccessException("Usuario sem permissao para operar neste grupo.");
            }

            return requestGroupId.Value;
        }

        if (accessScope.OperationalGroupIds.Count == 1)
        {
            return accessScope.OperationalGroupIds.Single();
        }

        throw new InvalidOperationException("O grupo do paciente deve ser informado.");
    }

    private static Guid ResolveExistingOrRequestedGroupId(Guid? requestGroupId, Guid currentGroupId, UserRole actorRole, AccessScope accessScope)
    {
        if (requestGroupId.HasValue && requestGroupId.Value != Guid.Empty)
        {
            return ResolveGroupId(requestGroupId, actorRole, accessScope);
        }

        if (!accessScope.IsAdmin && !accessScope.OperationalGroupIds.Contains(currentGroupId))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para operar neste grupo.");
        }

        return currentGroupId;
    }
}



