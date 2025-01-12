
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using LogginMS.Service.Interfaces;

namespace LogginMS.Service
{
    public class KeycloakClientSecret : IKeycloakClientSecret
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string _clientSecret;

        // Constructor que recibe IHttpClientFactory por DI
        public KeycloakClientSecret(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Método asincrónico para obtener el cliente secreto
        public async Task<string> GetClientSecretAsync()
        {
            // Aquí puedes usar el IHttpClientFactory para hacer una llamada real o simulada
            using (var client = _httpClientFactory.CreateClient())
            {
                // Obtener el token de acceso para obtener el id del cliente secreto
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
                //Obtener el la secret key del cliente 

                var getclientEndpoint = " http://keycloak:8080/admin/realms/Gruas_UCAB_1/clients";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var clientResponse = await client.GetAsync(getclientEndpoint);

                var responseContent = await clientResponse.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);

                // Recorrer la lista de objetos JSON
                foreach (var clientObj in jsonDoc.RootElement.EnumerateArray())
                {
                    // Buscar el objeto que tiene la propiedad "clientId" con el valor "myclient"
                    if (clientObj.TryGetProperty("clientId", out var clientId) && clientId.GetString() == "myclient")
                    {
                        // Obtener el valor de la propiedad "secret"
                        if (clientObj.TryGetProperty("secret", out var secret))
                        {
                            //return secret.GetString();
                            _clientSecret= secret.GetString();
                        }
                    }
                }
                
            }

            return _clientSecret;
        }
    }
}
