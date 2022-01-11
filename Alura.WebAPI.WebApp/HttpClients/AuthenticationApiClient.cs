using Alura.ListaLeitura.Seguranca;
using Alura.ListaLeitura.WebApp.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.WebApp.HttpClients
{
    public class AuthenticationApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthenticationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResultAuthentication> PostLoginAsync(LoginModel loginModel)
        {
            HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("Login", loginModel);
            return new LoginResultAuthentication(await responseMessage.Content.ReadAsStringAsync(), responseMessage.StatusCode);
        }

        public async Task PostRegisterAsync(RegisterViewModel registerViewModel)
        {
            HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync<RegisterViewModel>("Usuarios", registerViewModel);
            responseMessage.EnsureSuccessStatusCode();
        }
    }
}
