using Alura.ListaLeitura.Api.Models;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace Alura.ListaLeitura.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    //Confirguração de versionamenteo via Query String ou Header
    //[Route("api/Livros")]

    //Configuração de versionamento via rota
    [Route("api/v{version:apiVersion}/Livros")]
    public class Livros2Controller : ControllerBase
    {
        private readonly IRepository<Livro> _repository;

        public Livros2Controller(IRepository<Livro> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Recupera uma coleção paginada de livros.",
                          Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroPaginado))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult ListarLivros([FromQuery] LivroFiltro filtro,
                                          [FromQuery] LivroOrdem ordem,
                                          [FromQuery] LivroPaginacao paginacao)
        {
            var listaLivrosPaginado = _repository.All
                                                 .AplicarFiltro(filtro)
                                                 .AplicarOrdenacao(ordem)
                                                 .Select(l => l.ToLivroApi())
                                                 .ToLivroPaginado(paginacao);

            return Ok(listaLivrosPaginado);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Recupera o livro identificado por seu {id}.",
                          Tags = new[] { "Livros" },
                          Produces = new[] { "application/json", "application/xml" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
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
        [SwaggerOperation(Summary = "Recupera a capa do livro identificado por seu {id}.",
                          Tags = new[] { "Livros" },
                          Produces = new[] { "image/png" })]
        [ProducesResponseType(statusCode: 200)]
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
        [SwaggerOperation(Summary = "Registra novo livro na base.",
                          Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 400, Type = typeof(ErrorResponse))]
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

            return BadRequest(ErrorResponse.FromModelState(ModelState)); //Código 400
        }

        [HttpPut]
        [SwaggerOperation(Summary = "Modifica o livro na base.",
                          Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 400, Type = typeof(ErrorResponse))]
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

            return BadRequest(ErrorResponse.FromModelState(ModelState)); //Código 400
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Exclui o livro da base.",
                          Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 204, Type = typeof(LivroApi))]
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
