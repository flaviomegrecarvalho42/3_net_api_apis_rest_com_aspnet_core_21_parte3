using System.Net;

namespace Alura.ListaLeitura.Seguranca
{
    public class LoginResultAuthentication
    {
        public bool IsSuceeded { get; set; }
        public string Token { get; set; }

        public LoginResultAuthentication(string token, HttpStatusCode statusCode)
        {
            Token = token;
            IsSuceeded = (statusCode == HttpStatusCode.OK);
        }
    }
}
