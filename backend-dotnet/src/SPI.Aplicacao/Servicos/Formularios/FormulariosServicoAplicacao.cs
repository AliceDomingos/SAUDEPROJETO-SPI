using SPI.Application.DTOs.Forms;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Application.Services.Access;
using SPI.Domain.Enums;
using SPI.Domain.Repositories;

namespace SPI.Application.Services;

public sealed class FormsAppService : IFormsAppService
{
    private readonly IFormRepository _formRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FormsAppService(
        IFormRepository formRepository,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<FormResponseDto>> ListAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanViewForms())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar formularios.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        List<SPI.Domain.Entities.FormTemplate> forms;
        if (actor.Role == UserRole.Analyst)
        {
            forms = await _formRepository.ListAsync(cancellationToken);
        }
        else if (accessScope.IsAdmin && accessScope.OrganizationId.HasValue)
        {
            forms = await _formRepository.ListByOrganizationIdAsync(accessScope.OrganizationId.Value, cancellationToken);
        }
        else
        {
            forms = await _formRepository.ListByGroupIdsAsync(accessScope.OperationalGroupIds, cancellationToken);
        }

        return forms.Select(x => x.ToDto()).ToList();
    }

    public async Task<FormResponseDto?> GetByIdAsync(int formId, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanViewForms())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar formularios.");
        }

        var form = await _formRepository.GetDetailedByIdAsync(formId, cancellationToken);
        if (form is null)
        {
            return null;
        }

        EnsureCanAccessForm(actor, form.GroupId);
        return form.ToDto();
    }

    public async Task<FormResponseDto> CreateAsync(CreateFormRequestDto request, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageForms())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para criar formularios.");
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        ValidateFormGroupAccess(actor.Role, request.GroupId, accessScope);

        if (request.GroupId.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(request.GroupId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Grupo nao encontrado.");
        }

        var form = new SPI.Domain.Entities.FormTemplate(
            request.Nome,
            request.Descricao,
            actor.Id,
            request.GroupId,
            request.Perguntas.Select(x => (x.Texto, x.Peso, x.Ordem)));

        if (actor.Role == UserRole.Admin && actor.OrganizationId.HasValue)
        {
            form.AssignOrganization(actor.OrganizationId.Value);
        }

        await _formRepository.AddAsync(form, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _formRepository.GetDetailedByIdAsync(form.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel carregar o formulario criado.");

        return created.ToDto();
    }

    public async Task<FormResponseDto> UpdateAsync(int formId, UpdateFormRequestDto request, int actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        if (!actor.Role.CanManageForms())
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para alterar formularios.");
        }

        var form = await _formRepository.GetByIdAsync(formId, cancellationToken)
            ?? throw new KeyNotFoundException("Formulario nao encontrado.");

        var accessScope = AccessScopeResolver.Resolve(actor);
        ValidateFormGroupAccess(actor.Role, form.GroupId, accessScope);
        ValidateFormGroupAccess(actor.Role, request.GroupId, accessScope);

        if (request.GroupId.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(request.GroupId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Grupo nao encontrado.");
        }

        form.Update(
            request.Nome,
            request.Descricao,
            request.GroupId,
            request.Perguntas.Select(x => (x.Texto, x.Peso, x.Ordem)));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _formRepository.GetDetailedByIdAsync(form.Id, cancellationToken)
            ?? throw new InvalidOperationException("Nao foi possivel carregar o formulario atualizado.");

        return updated.ToDto();
    }

    private static void ValidateFormGroupAccess(UserRole role, int? groupId, AccessScope accessScope)
    {
        if (accessScope.IsAdmin)
        {
            return;
        }

        if (groupId is null)
        {
            throw new UnauthorizedAccessException("Somente administradores podem criar formularios globais.");
        }

        var allowedGroupIds = role.HasManagerPrivileges()
            ? accessScope.ManagedGroupIds
            : accessScope.OperationalGroupIds;

        if (!allowedGroupIds.Contains(groupId.Value))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para criar ou alterar formularios deste grupo.");
        }
    }

    private static void EnsureCanAccessForm(SPI.Domain.Entities.User actor, int? groupId)
    {
        if (actor.Role == UserRole.Analyst || actor.Role == UserRole.Admin || groupId is null)
        {
            return;
        }

        var accessScope = AccessScopeResolver.Resolve(actor);
        if (!accessScope.OperationalGroupIds.Contains(groupId.Value))
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para acessar este formulario.");
        }
    }
}



