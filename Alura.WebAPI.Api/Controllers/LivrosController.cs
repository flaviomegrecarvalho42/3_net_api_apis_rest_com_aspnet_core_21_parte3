using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Alura.ListaLeitura.Api.Models;
using System.Collections.Generic;

namespace Alura.ListaLeitura.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    //Confirguração de versionamenteo via Query String ou Header
    //[Route("api/[controller]")]

    //Configuração de versionamento via rota
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LivrosController : ControllerBase
    {
        private readonly IRepository<Livro> _repository;

        public LivrosController(IRepository<Livro> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [ProducesResponseType(statusCode: 200, Type = typeof(List<LivroApi>))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult ListarLivros()
        {
            var listaLivros = _repository.All
                                         .Select(l => l.ToLivroApi())
                                         .ToList();

            return Ok(listaLivros);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 404)]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Recuperar(int id)
        {
            var livroModel = _repository.Find(id);

            if (livroModel == null)
            {
                return NotFound(); //Error 404
            }

            return Ok(livroModel.ToLivroApi());
        }

        [HttpGet("capa/{id}")]
        [ProducesResponseType(statusCode: 200)]
        [Produces("image/jpg", "application/json")]
        public IActionResult ImagemCapa(int id)
        {
            byte[] img = _repository.All
                                    .Where(l => l.Id == id)
                                    .Select(l => l.ImagemCapa)
                                    .FirstOrDefault();

            if (img != null)
            {
                return File(img, "image/png");
            }

            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        [HttpPost]
        [ProducesResponseType(statusCode: 200, Type = typeof(Livro))]
        [ProducesResponseType(statusCode: 400)]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Incluir([FromForm] LivroUpload livroUploadModel)
        {
            if (ModelState.IsValid)
            {
                var livro = livroUploadModel.ToLivro();
                _repository.Incluir(livro);

                var uri = Url.Action("Recuperar", new { id = livro.Id });
                return Created(uri, livro);
            }

            return BadRequest(); //Código 400
        }

        [HttpPut]
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(statusCode: 400)]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Atualizar([FromForm] LivroUpload livroUploadModel)
        {
            if (ModelState.IsValid)
            {
                var livro = livroUploadModel.ToLivro();

                if (livroUploadModel.Capa == null)
                {
                    livro.ImagemCapa = _repository.All
                                                  .Where(l => l.Id == livro.Id)
                                                  .Select(l => l.ImagemCapa)
                                                  .FirstOrDefault();
                }

                _repository.Alterar(livro);
                return Ok(); //Código 200
            }

            return BadRequest(); //Código 400
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(statusCode: 204)]
        [ProducesResponseType(statusCode: 404)]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Deletar(int id)
        {
            var livroModel = _repository.Find(id);

            if (livroModel == null)
            {
                return NotFound(); //Error 404
            }

            _repository.Excluir(livroModel);
            return NoContent(); //Código 204
        }
    }
}