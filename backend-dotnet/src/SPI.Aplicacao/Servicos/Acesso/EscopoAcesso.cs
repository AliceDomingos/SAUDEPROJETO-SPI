namespace SPI.Application.Services.Access;

internal sealed record AccessScope(
    bool IsAdmin,
    bool IsAnalyst,
    bool IsManager,
    Guid? OrganizationId,
    IReadOnlyCollection<Guid> ManagedGroupIds,
    IReadOnlyCollection<Guid> MemberGroupIds)
{
    public IReadOnlyCollection<Guid> OperationalGroupIds =>
        ManagedGroupIds
            .Union(MemberGroupIds)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
}


