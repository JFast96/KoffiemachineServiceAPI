using KoffiemachineServiceAPI.DTOs;

namespace KoffiemachineServiceAPI.Services;

public interface IAuthService
{
    LoginResponse? Authenticate(LoginRequest request);
}
