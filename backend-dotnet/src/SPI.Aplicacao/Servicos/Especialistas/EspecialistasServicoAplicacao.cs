using SPI.Application.DTOs.Specialists;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Entities;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

namespace SPI.Application.Services;

public sealed class SpecialistsAppService : ISpecialistsAppService
{
    private readonly ISpecialistRepository _specialistRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpecialistsAppService(
        ISpecialistRepository specialistRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _specialistRepository = specialistRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<SpecialistResponseDto>> ListAsync(
        Guid actorUserId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        if (!actor.Role.CanAccessOperationalModules())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar especialistas.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        List<Specialist> specialists;
        if (accessScope.OrganizationId.HasValue)
        {
            specialists = activeOnly
                ? await _specialistRepository.ListActiveByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken)
                : await _specialistRepository.ListByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken);
        }
        else
        {
            specialists = activeOnly
                ? await _specialistRepository.ListActiveAsync(cancellationToken)
                : await _specialistRepository.ListAsync(cancellationToken);
        }

        return specialists.Select(x => x.ToDto()).ToList();
    }

    public async Task<SpecialistResponseDto> CreateAsync(
        CreateSpecialistRequestDto request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        EnsureCanManage(actor);

        var specialist = new Specialist(request.Nome, request.Especialidade, request.CustoConsulta);
        if (actor.OrganizationId.HasValue)
        {
            specialist.AssignOrganization(actor.OrganizationId.Value);
        }

        await _specialistRepository.AddAsync(specialist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return specialist.ToDto();
    }

    public async Task<SpecialistResponseDto> UpdateAsync(
        Guid id,
        UpdateSpecialistRequestDto request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        EnsureCanManage(actor);

        var specialist = await _specialistRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Especialista nao encontrado.");

        EnsureSameOrganization(actor, specialist);
        specialist.Update(request.Nome, request.Especialidade, request.CustoConsulta);
        if (request.Ativo)
        {
            specialist.Activate();
        }
        else
        {
            specialist.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return specialist.ToDto();
    }

    public async Task DeactivateAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(actorUserId, cancellationToken);
        EnsureCanManage(actor);

        var specialist = await _specialistRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Especialista nao encontrado.");

        EnsureSameOrganization(actor, specialist);
        specialist.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

    private static void EnsureCanManage(User actor)
    {
        if (actor.Role != UserRole.Admin && !actor.Role.HasManagerPrivileges())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para gerenciar especialistas.");
        }
    }

    private static void EnsureSameOrganization(User actor, Specialist specialist)
    {
        if (actor.OrganizationId.HasValue && specialist.OrganizationId.HasValue && actor.OrganizationId.Value != specialist.OrganizationId.Value)
        {
            throw new UnauthorizedAccessException("Especialista pertence a outra organizacao.");
        }
    }
}
