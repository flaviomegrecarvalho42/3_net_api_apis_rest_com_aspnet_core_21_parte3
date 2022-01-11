using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Linq.Dynamic.Core;   

namespace Alura.ListaLeitura.Modelos
{
    public static class LivrosExtensions
    {
        public static byte[] ConvertToBytes(this IFormFile image)
        {
            if (image == null)
            {
                return null;
            }

            using (var inputStream = image.OpenReadStream())
            using (var stream = new MemoryStream())
            {
                inputStream.CopyTo(stream);
                return stream.ToArray();
            }
        }

        public static Livro ToLivro(this LivroUpload livroUploadModel)
        {
            return new Livro
            {
                Id = livroUploadModel.Id,
                Titulo = livroUploadModel.Titulo,
                Subtitulo = livroUploadModel.Subtitulo,
                Resumo = livroUploadModel.Resumo,
                Autor = livroUploadModel.Autor,
                ImagemCapa = livroUploadModel.Capa.ConvertToBytes(),
                Lista = livroUploadModel.Lista
            };
        }

        public static LivroApi ToLivroApi(this Livro livroModel)
        {
            return new LivroApi
            {
                Id = livroModel.Id,
                Titulo = livroModel.Titulo,
                Subtitulo = livroModel.Subtitulo,
                Resumo = livroModel.Resumo,
                Autor = livroModel.Autor,
                Capa = $"/api/v1.0/Livros/capa/{livroModel.Id}",
                Lista = livroModel.Lista.ParaString()
            };
        }

        public static LivroUpload ToLivroUpload(this Livro livroModel)
        {
            return new LivroUpload
            {
                Id = livroModel.Id,
                Titulo = livroModel.Titulo,
                Subtitulo = livroModel.Subtitulo,
                Resumo = livroModel.Resumo,
                Autor = livroModel.Autor,
                Lista = livroModel.Lista
            };
        }

        public static LivroUpload ToLivroUpload(this LivroApi livroApModel)
        {
            return new LivroUpload
            {
                Id = livroApModel.Id,
                Titulo = livroApModel.Titulo,
                Subtitulo = livroApModel.Subtitulo,
                Resumo = livroApModel.Resumo,
                Autor = livroApModel.Autor,
                Lista = livroApModel.Lista.ParaTipo()
            };
        }

        public static LivroPaginado ToLivroPaginado(this IQueryable<LivroApi> query, LivroPaginacao paginacao)
        {
            int totalItens = query.Count();
            int totalPaginas = (int)Math.Ceiling(totalItens / (double)paginacao.Tamanho);

            return new LivroPaginado
            {
                Total = totalItens,
                TotalPaginas = totalItens != 0 ? totalPaginas : totalItens,
                NumeroPagina = paginacao.Pagina,
                TamanhoPagina = paginacao.Tamanho,

                Resultado = query.Skip(paginacao.Tamanho * (paginacao.Pagina - 1))
                                 .Take(paginacao.Tamanho).ToList(),

                Anterior = (paginacao.Pagina > 1) ? 
                           $"Livros?Tamanho={paginacao.Pagina-1}&Pagina={paginacao.Tamanho}" : "",

                Proximo = (paginacao.Pagina < totalPaginas) ?
                          $"Livros?Tamanho={paginacao.Pagina + 1}&Pagina={paginacao.Tamanho}" : "",
            };
        }

        public static IQueryable<Livro> AplicarFiltro(this IQueryable<Livro> query, LivroFiltro filtro)
        {
            if (filtro != null)
            {
                if (!string.IsNullOrWhiteSpace(filtro.Titulo))
                {
                    query = query.Where(l => l.Titulo.Contains(filtro.Titulo, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filtro.Subtitulo))
                {
                    query = query.Where(l => l.Subtitulo.Contains(filtro.Subtitulo, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filtro.Autor))
                {
                    query = query.Where(l => l.Autor.Contains(filtro.Autor, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filtro.Lista))
                {
                    query = query.Where(l => l.Lista == filtro.Lista.ParaTipo());
                }
            }

            return query;
        }

        public static IQueryable<Livro> AplicarOrdenacao(this IQueryable<Livro> query, LivroOrdem ordem)
        {
            if (ordem != null && (!string.IsNullOrWhiteSpace(ordem.OrdenarPor)))
            {
                query = query.OrderBy(ordem.OrdenarPor);
            }

            return query;
        }
    }
}
