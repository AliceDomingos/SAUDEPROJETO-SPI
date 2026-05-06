using SPI.Application.DTOs.Profile;
using SPI.Application.DTOs.Users;
using SPI.Application.Interfaces;
using SPI.Application.Mappings;
using SPI.Domain.Repositories;
using SPI.Domain.ValueObjects;

namespace SPI.Application.Services;

public sealed class ProfileAppService : IProfileAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileAppService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponseDto> GetAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        return user.ToDto();
    }

    public async Task<UserResponseDto> UpdateAsync(UpdateProfileRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetDetailedByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null && existingUser.Id != user.Id)
        {
            throw new InvalidOperationException("Ja existe usuario com este email.");
        }

        user.UpdateProfile(request.Nome, new Email(request.Email));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}



