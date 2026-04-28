namespace SPI.Application.Services.Access;

internal sealed record AccessScope(
    bool IsAdmin,
    bool IsAnalyst,
    bool IsManager,
    int? OrganizationId,
    IReadOnlyCollection<int> ManagedGroupIds,
    IReadOnlyCollection<int> MemberGroupIds)
{
    public IReadOnlyCollection<int> OperationalGroupIds =>
        ManagedGroupIds
            .Union(MemberGroupIds)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
}



