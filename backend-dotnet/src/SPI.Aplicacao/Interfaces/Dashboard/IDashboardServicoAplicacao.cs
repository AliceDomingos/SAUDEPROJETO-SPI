using SPI.Application.DTOs.Dashboard;
using SPI.Application.DTOs.Groups;

namespace SPI.Application.Interfaces;

public interface IDashboardAppService
{
    Task<DashboardResponseDto> GetAsync(
        Guid actorUserId,
        string? risco = null,
        string? especialista = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        Guid? grupoId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GroupResponseDto>> ListFilterGroupsAsync(
        Guid actorUserId,
        CancellationToken cancellationToken = default);
}
