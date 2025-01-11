

namespace LogginMS.Service.Interfaces
{
    public interface IKeycloakClientSecret
    {
        Task<string> GetClientSecretAsync();

    }
}
