using MoodPlaylistApi.Dtos.Auth;
using MoodPlaylistApi.Utilities;

namespace MoodPlaylistApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerRequest);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginRequest);
        Task<ApiResponse<string>> LogoutAsync(Guid userId);
    }
}
