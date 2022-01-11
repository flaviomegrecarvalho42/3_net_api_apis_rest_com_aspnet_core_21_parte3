using Alura.ListaLeitura.Api.Models;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;

namespace Alura.ListaLeitura.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    //Confirguração de versionamenteo via Query String ou Header
    //[Route("api/[controller]")]

    //Configuração de versionamento via rota
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ListasLeituraController : ControllerBase
    {
        private readonly IRepository<Livro> _repository;

        public ListasLeituraController(IRepository<Livro> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Recupera as listas de leitura.",
                          Tags = new[] { "Listas" },
                          Produces = new[] { "application/json", "application/xml" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(List<Lista>))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult ListarListasLeitura()
        {
            Lista paraLer = CriarLista(TipoListaLeitura.ParaLer);
            Lista lendo = CriarLista(TipoListaLeitura.Lendo);
            Lista lidos = CriarLista(TipoListaLeitura.Lidos);

            var colecaoListaLeitura = new List<Lista> { paraLer, lendo, lidos };
            return Ok(colecaoListaLeitura);
        }

        [HttpGet("{tipoLista}")]
        [SwaggerOperation(Summary = "Recupera a lista de leitura identificada por seu {tipo}.",
                          Tags = new[] { "Listas" },
                          Produces = new[] { "application/json", "application/xml" })]
        [ProducesResponseType(statusCode: 200, Type = typeof(Lista))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult ListarListaLeitura(TipoListaLeitura tipoLista)
        {
            var listaLeitura = CriarLista(tipoLista);

            if (listaLeitura == null)
            {
                return NotFound(); //Error 404
            }

            return Ok(listaLeitura);
        }

        private Lista CriarLista(TipoListaLeitura tipoListaLeitura)
        {
            return new Lista
            {
                Tipo = tipoListaLeitura.ParaString(),
                Livros = _repository.All
                                    .Where(l => l.Lista == tipoListaLeitura)
                                    .Select(l => l.ToLivroApi())
                                    .ToList()
            };
        }
    }
}