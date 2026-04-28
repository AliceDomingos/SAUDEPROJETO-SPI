using SPI.Application.DTOs.Users;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

namespace SPI.Application.Services;

public sealed class UsersAppService : IUsersAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupsAppService _groupsAppService;
    private readonly IUnitOfWork _unitOfWork;

    public UsersAppService(IUserRepository userRepository, IGroupRepository groupRepository, IGroupsAppService groupsAppService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _groupsAppService = groupsAppService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> ListAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageUsers())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para listar usuarios.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        IReadOnlyCollection<SPI.Domain.Entities.User> users;
        if (actor.Role == UserRole.Admin)
        {
            users = await _userRepository.ListAsync(cancellationToken);
        }
        else
        {
            var allInScope = await _userRepository.ListByGroupIdsAsync(accessScope.ManagedGroupIds, cancellationToken);
            users = allInScope.Where(u => u.Role != UserRole.Admin).ToArray();
        }

        return users.Select(x => x.ToDto()).ToList();
    }

    public async Task DeactivateAsync(int userId, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageUsers())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para desativar usuarios.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        var user = await _userRepository.GetDetailedByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        var targetGroupIds = user.GroupMemberships.Select(x => x.GroupId).Distinct().ToArray();
        if (actor.Role == UserRole.Admin && user.Role != UserRole.Analyst && targetGroupIds.Any(x => !accessScope.OperationalGroupIds.Contains(x)))
        {
            throw new UnauthorizedAccessException("Administrador so pode desativar usuarios dos grupos aos quais esta vinculado.");
        }

        if (actor.Role.HasManagerPrivileges())
        {
            if (user.Role is UserRole.Admin or UserRole.Analyst)
            {
                throw new UnauthorizedAccessException("Perfil de gestao nao pode desativar este tipo de usuario.");
            }

            if (targetGroupIds.Any() && targetGroupIds.Any(x => !accessScope.ManagedGroupIds.Contains(x)))
            {
                throw new UnauthorizedAccessException("Perfil de gestao so pode desativar usuarios dos grupos que gerencia.");
            }
        }

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateGroupsAsync(
        int userId,
        UpdateUserGroupsRequestDto request,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageUsers())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para alterar grupos.");
        }

        var user = await _userRepository.GetDetailedByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        var allRequestedGroups = await _groupRepository.ListByIdsAsync(
            request.GroupIds.Where(x => x > 0).Distinct().ToArray(),
            cancellationToken);

        var requestedGroupIds = allRequestedGroups
            .Select(g => g.Id)
            .OrderBy(x => x)
            .ToArray();

        var validCount = request.GroupIds.Where(x => x > 0).Distinct().Count();
        if (allRequestedGroups.Count != validCount)
        {
            throw new KeyNotFoundException("Um ou mais grupos informados nao existem.");
        }

        if (user.Role == UserRole.Analyst && requestedGroupIds.Length > 0)
        {
            throw new UnauthorizedAccessException("Analistas nao podem ser vinculados a grupos.");
        }


        if (actor.Role.HasManagerPrivileges())
        {
            var accessScope = AccessScopeResolver.Resolve(actor);
            if (user.Role is UserRole.Admin or UserRole.Analyst)
            {
                throw new UnauthorizedAccessException("Perfil de gestao nao pode alterar grupos deste tipo de usuario.");
            }

            if (requestedGroupIds.Any(x => !accessScope.ManagedGroupIds.Contains(x)))
            {
                throw new UnauthorizedAccessException("Perfil de gestao so pode vincular usuarios aos grupos que gerencia.");
            }

            var currentGroupIds = user.GroupMemberships.Select(x => x.GroupId).Distinct().ToArray();
            if (currentGroupIds.Any(x => !accessScope.ManagedGroupIds.Contains(x)))
            {
                throw new UnauthorizedAccessException("Perfil de gestao so pode alterar usuarios dentro dos grupos que gerencia.");
            }
        }

        await _userRepository.ReplaceGroupMembershipsAsync(userId, requestedGroupIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (actor.Role == UserRole.Admin && user.Role == UserRole.Manager)
        {
            var adminScope = AccessScopeResolver.Resolve(actor);
            var selectedGroupId = requestedGroupIds
                .FirstOrDefault(x => adminScope.OperationalGroupIds.Contains(x));

            if (selectedGroupId > 0)
            {
                await _groupsAppService.AssignManagerAsync(selectedGroupId, user.Id, cancellationToken);
            }
        }
    }
}



