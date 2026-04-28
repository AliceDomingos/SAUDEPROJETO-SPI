using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Patient>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Patients
            .AsNoTracking()
            .Include(x => x.Group)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<Patient>> ListByGroupIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return Task.FromResult(new List<Patient>());
        }

        return _context.Patients
            .AsNoTracking()
            .Include(x => x.Group)
            .Where(x => groupIds.Contains(x.GroupId))
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Patients
            .Include(x => x.Group)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Patient>> ListByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default) =>
        _context.Patients
            .AsNoTracking()
            .Include(x => x.Group)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Patient patient, CancellationToken cancellationToken = default) =>
        _context.Patients.AddAsync(patient, cancellationToken).AsTask();

    public void Remove(Patient patient) => _context.Patients.Remove(patient);
}



