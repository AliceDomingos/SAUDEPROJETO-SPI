using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class FormRepository : IFormRepository
{
    private readonly AppDbContext _context;

    public FormRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<FormTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.FormTemplates
            .AsNoTracking()
            .Include(x => x.Group)
            .Include(x => x.CriadoPorUsuario)
            .Include(x => x.Questions)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<FormTemplate>> ListByGroupIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default) =>
        _context.FormTemplates
            .AsNoTracking()
            .Include(x => x.Group)
            .Include(x => x.CriadoPorUsuario)
            .Include(x => x.Questions)
            .Where(x => x.GroupId == null || groupIds.Contains(x.GroupId.Value))
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<FormTemplate>> ListByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default) =>
        _context.FormTemplates
            .AsNoTracking()
            .Include(x => x.Group)
            .Include(x => x.CriadoPorUsuario)
            .Include(x => x.Questions)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<FormTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.FormTemplates
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<FormTemplate?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.FormTemplates
            .Include(x => x.Group)
            .Include(x => x.CriadoPorUsuario)
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(FormTemplate formTemplate, CancellationToken cancellationToken = default) =>
        _context.FormTemplates.AddAsync(formTemplate, cancellationToken).AsTask();
}



