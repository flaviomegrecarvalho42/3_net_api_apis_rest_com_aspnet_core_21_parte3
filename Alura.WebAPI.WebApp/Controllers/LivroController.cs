using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.WebApp.HttpClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.WebApp.Controllers
{
    [Authorize]
    public class LivroController : Controller
    {
        private readonly LivroApiClient _apiClient;

        public LivroController(LivroApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Novo()
        {
            return View(new LivroUpload());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Novo(LivroUpload livroUploadModel)
        {
            if (ModelState.IsValid)
            {
                await _apiClient.PostLivroAsync(livroUploadModel);
                return RedirectToAction("Index", "Home");
            }

            return View(livroUploadModel);
        }

        [HttpGet]
        public async Task<IActionResult> ImagemCapa(int id)
        {
            byte[] img = await _apiClient.GetCapaLivroAsync(id);

            if (img == null)
            {
                return File("~/images/capas/capa-vazia.png", "image/png"); 
            }

            return File(img, "image/png");
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            #region Links a serem consultados
            //http://localhos:6000/api/Livros/{id}
            //http://localhos:6000/api/Livros/capa/{id}
            //http://localhos:6000/api/ListasLeitura/ParaLer
            #endregion

            LivroApi livroApiModel = await _apiClient.GetLivroAsync(id);

            if (livroApiModel == null)
            {
                return NotFound();
            }

            return View(livroApiModel.ToLivroUpload());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detalhes(LivroUpload livroUploadModel)
        {
            if (ModelState.IsValid)
            {
                await _apiClient.PutLivroAsync(livroUploadModel);
                return RedirectToAction("Index", "Home");
            }

            return View(livroUploadModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            var livroModel = await _apiClient.GetLivroAsync(id);

            if (livroModel == null)
            {
                return NotFound();
            }

            await _apiClient.DeleteLivroAsync(id);
            return RedirectToAction("Index", "Home");
        }
    }
}