

namespace LogginMS.Service.Interfaces
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string username, string password);

        Task<string> RequestPasswordResetAsync(string username);
        Task UpdatePasswordAsync( string password, string username);
    }
}
