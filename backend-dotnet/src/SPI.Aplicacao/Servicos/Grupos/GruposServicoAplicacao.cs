using SPI.Application.DTOs.Groups;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

namespace SPI.Application.Services;

public sealed class GroupsAppService : IGroupsAppService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GroupsAppService(
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IEvaluationRepository evaluationRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _evaluationRepository = evaluationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<GroupResponseDto>> ListAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanAccessOperationalModules())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar grupos.");
        }

        List<SPI.Domain.Entities.Group> groups;
        if (actor.Role == UserRole.Admin)
        {
            var accessScope = AccessScopeResolver.Resolve(actor);
            groups = accessScope.OrganizationId.HasValue
                ? await _groupRepository.ListByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken)
                : await _groupRepository.ListAsync(cancellationToken);
        }
        else
        {
            var accessScope = AccessScopeResolver.Resolve(actor);
            groups = await _groupRepository.ListByIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
        }

        return groups.Select(x => x.ToDto()).ToList();
    }

    public async Task<GroupResponseDto> CreateAsync(CreateGroupRequestDto request, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageGroups())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para criar grupos.");
        }

        var gestorId = actor.Role.HasManagerPrivileges()
            ? actor.Id
            : request.GestorId;

        int resolvedGestorId;
        if (gestorId.HasValue)
        {
            var gestor = await _userRepository.GetByIdAsync(gestorId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Responsavel pelo grupo nao encontrado.");

            if (!gestor.Role.HasManagerPrivileges())
            {
                throw new InvalidOperationException("O responsavel informado precisa ter perfil de gestor.");
            }

            resolvedGestorId = gestor.Id;
        }
        else
        {
            resolvedGestorId = actor.Id;
        }

        var group = new SPI.Domain.Entities.Group(request.Nome, resolvedGestorId);

        if (actor.Role == UserRole.Admin && actor.OrganizationId.HasValue)
        {
            group.AssignOrganization(actor.OrganizationId.Value);
        }

        await _groupRepository.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _groupRepository.GetDetailedByIdAsync(group.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel carregar o grupo criado.");

        return created.ToDto();
    }

    public async Task<GroupResponseDto> UpdateAsync(int groupId, UpdateGroupRequestDto request, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageGroups())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para alterar grupos.");
        }

        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo nao encontrado.");

        if (actor.Role.HasManagerPrivileges() && group.GestorId != actor.Id)
        {
            throw new UnauthorizedAccessException("Perfil de gestao so pode alterar o proprio grupo.");
        }

        int resolvedGestorId;
        if (actor.Role.HasManagerPrivileges())
        {
            resolvedGestorId = actor.Id;
        }
        else if (request.GestorId.HasValue)
        {
            var gestor = await _userRepository.GetByIdAsync(request.GestorId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Responsavel pelo grupo nao encontrado.");

            if (!gestor.Role.HasManagerPrivileges())
            {
                throw new InvalidOperationException("O responsavel informado precisa ter perfil de gestor.");
            }

            resolvedGestorId = gestor.Id;
        }
        else
        {
            resolvedGestorId = group.GestorId;
        }

        group.Update(request.Nome, resolvedGestorId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _groupRepository.GetDetailedByIdAsync(group.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel carregar o grupo atualizado.");

        return updated.ToDto();
    }

    public async Task DeleteAsync(int groupId, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageGroups())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para excluir grupos.");
        }

        var group = await _groupRepository.GetDetailedByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo nao encontrado.");

        if (actor.Role.HasManagerPrivileges() && group.GestorId != actor.Id)
        {
            throw new UnauthorizedAccessException("Perfil de gestao so pode excluir o proprio grupo.");
        }

        if (group.Patients.Count != 0)
        {
            throw new InvalidOperationException("Nao e possivel excluir grupo com pacientes vinculados.");
        }

        if (group.Forms.Count != 0)
        {
            throw new InvalidOperationException("Nao e possivel excluir grupo com formularios vinculados.");
        }

        if (await _evaluationRepository.AnyByGroupIdAsync(group.Id, cancellationToken))
        {
            throw new InvalidOperationException("Nao e possivel excluir grupo com avaliacoes vinculadas.");
        }

        _groupRepository.Remove(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignManagerAsync(int groupId, int managerId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Grupo nao encontrado.");

        group.Update(group.Nome, managerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}



