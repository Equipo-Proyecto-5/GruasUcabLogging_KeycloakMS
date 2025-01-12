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
        private readonly IKeycloakClientSecret _keycloakClientSecret;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration config,IKeycloakClientSecret keycloakClientSecret)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _httpClient = _httpClientFactory.CreateClient();
            _keycloakClientSecret = keycloakClientSecret;
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

            var tokenEndpoint = $"{_config["Keycloak:BaseUrl"]}/realms/{_config["Keycloak:Realm"]}/protocol/openid-connect/token";
            var response = await client.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode==System.Net.HttpStatusCode.BadRequest)
                {
                  
                  if(await GetRequiredActionsByUser(username))
                    {
                        throw new InvalidOperationException();
                    };
                    
                }
                throw new UnauthorizedAccessException();
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return tokenResponse.GetProperty("access_token").GetString();
          //  return await response.Content.ReadAsStringAsync();
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

            var tokenEndpoint = $"{_config["Keycloak:BaseUrl"]}/realms/master/protocol/openid-connect/token";
            var tokenResponse = await client.PostAsync(tokenEndpoint, tokenContent);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("No se pudo obtener el token de acceso.");
            }

            var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(tokenResult).RootElement.GetProperty("access_token").GetString();

            // Obtener el ID del usuario
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userEndpoint = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:Realm"]}/users?username={username}";
            var userResponse = await client.GetAsync(userEndpoint);
            if (!userResponse.IsSuccessStatusCode)
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"No se pudo obtener el ID del usuario. Código de estado: {userResponse.StatusCode}, Contenido: {errorContent}");
            }

            var userResult = await userResponse.Content.ReadAsStringAsync();
            var userId = JsonDocument.Parse(userResult).RootElement[0].GetProperty("id").GetString();

            // Usar el token de acceso para la solicitud de restablecimiento de contraseña
            var content = new StringContent("[\"UPDATE_PASSWORD\"]", Encoding.UTF8, "application/json");

            var requestEndpoint = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}/execute-actions-email";
            var request = new HttpRequestMessage(HttpMethod.Put, requestEndpoint)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"No se pudo iniciar el proceso de recuperación de contraseña. Código de estado: {response.StatusCode}, Contenido: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetAdminAccessTokenAsync()
        {
            var secreto = await _keycloakClientSecret.GetClientSecretAsync();
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = $"{_config["Keycloak:BaseUrl"]}/realms/{_config["Keycloak:Realm"]}/protocol/openid-connect/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",_config["Keycloak:ClientId"]),
                new KeyValuePair<string, string>("client_secret",secreto),
                new KeyValuePair<string, string>("grant_type", "client_credentials")

            });
            var response = await client.PostAsync(tokenEndpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return tokenResponse.GetProperty("access_token").GetString();
        }

        public async Task<Guid> GetUserByUserName(string userName)
        {
            var client = _httpClientFactory.CreateClient();
            var accessToken = await GetAdminAccessTokenAsync();

            var getUserEndpoint = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:Realm"]}/users?username={userName}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var userResponse = await client.GetAsync(getUserEndpoint);
            
            var responseContent = await userResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var users = jsonDoc.RootElement;
            var id = users[0].GetProperty("id").GetString();
            return Guid.Parse(id);

        }
        public async Task<bool> GetRequiredActionsByUser(string userName)
        {
            var client = _httpClientFactory.CreateClient();
            var accessToken = await GetAdminAccessTokenAsync();
            var userId = await GetUserByUserName(userName);
            var getUserEndpoint = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var userResponse = await client.GetAsync(getUserEndpoint);

            var responseContent = await userResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var user = jsonDoc.RootElement;
      
            if (user.TryGetProperty("requiredActions", out var requiredActions) && requiredActions.ValueKind == JsonValueKind.Array)
            {
                
                var requiresPasswordUpdate = requiredActions.EnumerateArray().Any(action => action.GetString() == "UPDATE_PASSWORD");
                return requiresPasswordUpdate;
            }

            return false;

        }
        public async Task UpdatePasswordAsync(  string password, string userName)
        {
            var id = await GetUserByUserName(userName);
            var client = _httpClientFactory.CreateClient();
            var accessToken = await GetAdminAccessTokenAsync();
            var updateUserEndpoint = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:Realm"]}/users/{id}";
            var user = new
            {
                requiredActions = new string[] { },
                credentials = new[]
                 { new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
                }
            };
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, updateUserEndpoint)
            {
                Content = content,
               
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error al modificar usuario: {errorContent}");
            }
        }

       
    }
}
