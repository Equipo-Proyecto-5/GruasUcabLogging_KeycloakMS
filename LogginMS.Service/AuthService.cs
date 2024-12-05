using LogginMS.Service.Interfaces;


namespace LogginMS.Service
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "client-public"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password"),
            });

            var response = await client.PostAsync("http://keycloak:8080/realms/Gruas_UCAB_1/protocol/openid-connect/token", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException();
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
