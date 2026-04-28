using SPI.Domain.Entities;
using SPI.Domain.ReadModels;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class EvaluationRepository : IEvaluationRepository
{
    private readonly AppDbContext _context;

    public EvaluationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EvaluationDetails>> ListDetailedAsync(CancellationToken cancellationToken = default)
    {
        var evaluations = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<List<EvaluationDetails>> ListDetailedByGroupIdsAsync(
        IReadOnlyCollection<int> groupIds,
        CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return [];
        }

        var evaluations = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .Where(x => groupIds.Contains(x.GroupId))
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<List<EvaluationDetails>> ListDetailedByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var evaluations = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<EvaluationDetails?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var evaluation = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return evaluation is null ? null : Map(evaluation);
    }

    public Task<bool> AnyByGroupIdAsync(int groupId, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AnyAsync(x => x.GroupId == groupId, cancellationToken);

    public Task<bool> AnyByPatientIdAsync(int patientId, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AnyAsync(x => x.PatientId == patientId, cancellationToken);

    public Task<Evaluation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Evaluations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Evaluation?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Evaluations
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .ThenInclude(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Evaluation evaluation, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AddAsync(evaluation, cancellationToken).AsTask();

    public void Remove(Evaluation evaluation) => _context.Evaluations.Remove(evaluation);

    private static EvaluationDetails Map(Evaluation evaluation) => new()
    {
        Id = evaluation.Id,
        PatientId = evaluation.PatientId,
        PatientNome = evaluation.Patient.Nome,
        AvaliadorId = evaluation.AvaliadorId,
        AvaliadorNome = evaluation.Avaliador.Nome,
        GroupId = evaluation.GroupId,
        GroupNome = evaluation.Patient.Group.Nome,
        FormTemplateId = evaluation.FormTemplateId,
        FormNome = evaluation.FormTemplate?.Nome,
        Respostas = new Dictionary<int, int>(evaluation.Respostas),
        ScoreTotal = evaluation.ScoreTotal,
        PesoTotal = evaluation.PesoTotal,
        Classificacao = evaluation.Classificacao,
        DataAvaliacao = evaluation.DataAvaliacao
    };
}



