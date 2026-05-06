using SPI.Domain.Entities;
using SPI.Domain.Enums;

namespace SPI.Application.Config;

public static class SystemGroupRules
{
    public const string AdminDefaultGroupName = "Grupo Padrao";

    public static bool IsAdminDefaultGroup(Group group) =>
        string.Equals(group.Nome, AdminDefaultGroupName, StringComparison.OrdinalIgnoreCase);

    public static bool IsAdminDefaultGroupName(string? groupName) =>
        string.Equals(groupName?.Trim(), AdminDefaultGroupName, StringComparison.OrdinalIgnoreCase);

    public static void EnsureNameIsAvailable(string groupName)
    {
        if (IsAdminDefaultGroupName(groupName))
        {
            throw new InvalidOperationException("O nome 'Grupo Padrao' e reservado para o grupo interno do administrador.");
        }
    }

    public static void EnsureGroupCanBeManaged(Group group)
    {
        if (IsAdminDefaultGroup(group))
        {
            throw new InvalidOperationException("O Grupo Padrao do administrador e protegido e nao pode ser alterado manualmente.");
        }
    }

    public static void EnsureGroupCanBeDeleted(Group group)
    {
        if (IsAdminDefaultGroup(group))
        {
            throw new InvalidOperationException("O Grupo Padrao do administrador nao pode ser excluido.");
        }
    }

    public static void EnsureManagerRemainsAdminForProtectedGroup(Group group, Guid gestorId, Guid adminUserId)
    {
        if (IsAdminDefaultGroup(group) && gestorId != adminUserId)
        {
            throw new InvalidOperationException("O Grupo Padrao deve permanecer vinculado ao administrador.");
        }
    }
}
