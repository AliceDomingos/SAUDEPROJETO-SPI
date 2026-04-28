using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _context;

    public OrganizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Organization?> GetByAdminIdAsync(int adminId, CancellationToken cancellationToken = default) =>
        _context.Organizations
            .FirstOrDefaultAsync(x => x.AdminId == adminId, cancellationToken);

    public Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Organizations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Organization organization, CancellationToken cancellationToken = default) =>
        _context.Organizations.AddAsync(organization, cancellationToken).AsTask();
}
