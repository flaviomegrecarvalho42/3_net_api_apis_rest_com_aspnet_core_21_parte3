using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.WebApp.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;

namespace Alura.ListaLeitura.WebApp.HttpClients
{
    public class LivroApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LivroApiClient(HttpClient httpClient,
                              IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LivroApi> GetLivroAsync(int id)
        {
            AddBearerToken();

            HttpResponseMessage responseMessage = await _httpClient.GetAsync($"Livros/{id}");
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsAsync<LivroApi>();
        }

        public async Task<byte[]> GetCapaLivroAsync(int id)
        {
            AddBearerToken();

            HttpResponseMessage responseMessage = await _httpClient.GetAsync($"Livros/capa/{id}");
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsByteArrayAsync();
        }

        public async Task<Lista> GetListaLeituraPorTipoAsync(TipoListaLeitura tipoListaLeitura)
        {
            AddBearerToken();

            HttpResponseMessage responseMessage = await _httpClient.GetAsync($"ListasLeitura/{tipoListaLeitura}");
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsAsync<Lista>();
        }

        public async Task PostLivroAsync(LivroUpload livroUploadModel)
        {
            AddBearerToken();

            HttpContent content = CreateMultipartFormDataContent(livroUploadModel);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("Livros", content);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Código de Status Http 201 esperado!");
            }
        }

        public async Task PutLivroAsync(LivroUpload livroUploadModel)
        {
            AddBearerToken();

            HttpContent content = CreateMultipartFormDataContent(livroUploadModel);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync("Livros", content);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Código de Status Http 200 esperado!");
            }
        }

        public async Task DeleteLivroAsync(int id)
        {
            AddBearerToken();

            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync($"Livros/{id}");
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException("Código de Status Http 204 esperado!");
            }
        }

        private HttpContent CreateMultipartFormDataContent(LivroUpload livroUploadModel)
        {
            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StringContent(livroUploadModel.Titulo), Helpers.EnvolverComAspasDuplas("titulo") },
                { new StringContent(livroUploadModel.Lista.ParaString()), Helpers.EnvolverComAspasDuplas("lista") }
            };

            if (livroUploadModel.Id > 0)
            {
                content.Add(new StringContent(livroUploadModel.Id.ToString()), Helpers.EnvolverComAspasDuplas("id"));
            }

            if (!string.IsNullOrEmpty(livroUploadModel.Subtitulo))
            {
                content.Add(new StringContent(livroUploadModel.Subtitulo), Helpers.EnvolverComAspasDuplas("subtitulo"));
            }

            if (!string.IsNullOrEmpty(livroUploadModel.Resumo))
            {
                content.Add(new StringContent(livroUploadModel.Resumo), Helpers.EnvolverComAspasDuplas("resumo"));
            }

            if (!string.IsNullOrEmpty(livroUploadModel.Autor))
            {
                content.Add(new StringContent(livroUploadModel.Autor), Helpers.EnvolverComAspasDuplas("autor"));
            }

            if (livroUploadModel.Capa != null)
            {
                var imagemContent = new ByteArrayContent(livroUploadModel.Capa.ConvertToBytes());
                imagemContent.Headers.Add("content-type", "image/png");

                content.Add(imagemContent, Helpers.EnvolverComAspasDuplas("capa"), Helpers.EnvolverComAspasDuplas("capa.png"));
            }

            return content;
        }

        private void AddBearerToken()
        {
            var token = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "Token").Value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
