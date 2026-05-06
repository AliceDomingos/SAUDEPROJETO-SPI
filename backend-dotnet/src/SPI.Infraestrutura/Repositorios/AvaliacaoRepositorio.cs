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
            .Include(x => x.Referral)
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<List<EvaluationDetails>> ListDetailedByGroupIdsAsync(
        IReadOnlyCollection<Guid> groupIds,
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
            .Include(x => x.Referral)
            .Where(x => groupIds.Contains(x.GroupId))
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<List<EvaluationDetails>> ListDetailedByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var evaluations = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .Include(x => x.Referral)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.DataAvaliacao)
            .ToListAsync(cancellationToken);

        return evaluations.Select(Map).ToList();
    }

    public async Task<EvaluationDetails?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evaluation = await _context.Evaluations
            .AsNoTracking()
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .Include(x => x.Referral)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return evaluation is null ? null : Map(evaluation);
    }

    public Task<bool> AnyByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AnyAsync(x => x.GroupId == groupId, cancellationToken);

    public Task<bool> AnyByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AnyAsync(x => x.PatientId == patientId, cancellationToken);

    public Task<Evaluation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Evaluations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Evaluation?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Evaluations
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Avaliador)
            .Include(x => x.FormTemplate)
            .ThenInclude(x => x.Questions)
            .Include(x => x.Referral)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Evaluation?> GetByIdWithReferralAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Evaluations
            .Include(x => x.Patient)
            .ThenInclude(x => x.Group)
            .Include(x => x.Referral)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Evaluation evaluation, CancellationToken cancellationToken = default) =>
        _context.Evaluations.AddAsync(evaluation, cancellationToken).AsTask();

    public Task AddReferralAsync(EvaluationReferral referral, CancellationToken cancellationToken = default) =>
        _context.EvaluationReferrals.AddAsync(referral, cancellationToken).AsTask();

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
        Respostas = new Dictionary<string, int>(evaluation.Respostas),
        ScoreTotal = evaluation.ScoreTotal,
        PesoTotal = evaluation.PesoTotal,
        Classificacao = evaluation.Classificacao,
        Observacoes = evaluation.Observacoes,
        DataAvaliacao = evaluation.DataAvaliacao,
        Referral = evaluation.Referral is null
            ? null
            : new EvaluationReferralDetails
            {
                Id = evaluation.Referral.Id,
                EvaluationId = evaluation.Referral.EvaluationId,
                PatientId = evaluation.Referral.PatientId,
                Encaminhado = evaluation.Referral.Encaminhado,
                SpecialistId = evaluation.Referral.SpecialistId,
                SpecialistNome = evaluation.Referral.SpecialistNome,
                Especialidade = evaluation.Referral.Especialidade,
                CustoEstimado = evaluation.Referral.CustoEstimado,
                CriadoEm = evaluation.Referral.CriadoEm
            }
    };
}



