using LogginMS.Service.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace LogginMS.Service
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _httpClient = _httpClientFactory.CreateClient();
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

        public async Task<string> RequestPasswordResetAsync(string username)
        {
            var client = _httpClientFactory.CreateClient();

            // Obtener el token de acceso
            var tokenContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("client_id", "admin-cli"),
        new KeyValuePair<string, string>("grant_type", "password"),
        new KeyValuePair<string, string>("username", "admin"),
        new KeyValuePair<string, string>("password", "admin"),
    });

            var tokenResponse = await client.PostAsync("http://keycloak:8080/realms/master/protocol/openid-connect/token", tokenContent);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("No se pudo obtener el token de acceso.");
            }

            var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(tokenResult).RootElement.GetProperty("access_token").GetString();

            // Obtener el ID del usuario
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userResponse = await client.GetAsync($"http://keycloak:8080/admin/realms/Gruas_UCAB_1/users?username={username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"No se pudo obtener el ID del usuario. Código de estado: {userResponse.StatusCode}, Contenido: {errorContent}");
            }

            var userResult = await userResponse.Content.ReadAsStringAsync();
            var userId = JsonDocument.Parse(userResult).RootElement[0].GetProperty("id").GetString();

            // Usar el token de acceso para la solicitud de restablecimiento de contraseña
            var content = new StringContent("[\"UPDATE_PASSWORD\"]", Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://keycloak:8080/admin/realms/Gruas_UCAB_1/users/{userId}/execute-actions-email")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            // Manejo de errores si el proceso falla
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"No se pudo iniciar el proceso de recuperación de contraseña. Código de estado: {response.StatusCode}, Contenido: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }


    }
}
