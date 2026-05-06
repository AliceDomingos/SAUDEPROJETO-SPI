using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class SpecialistRepository : ISpecialistRepository
{
    private readonly AppDbContext _context;

    public SpecialistRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Specialist>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Specialists
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<Specialist>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        _context.Specialists
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<Specialist>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        _context.Specialists
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<Specialist>> ListActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        _context.Specialists
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<Specialist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Specialists.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Specialist specialist, CancellationToken cancellationToken = default) =>
        _context.Specialists.AddAsync(specialist, cancellationToken).AsTask();
}
