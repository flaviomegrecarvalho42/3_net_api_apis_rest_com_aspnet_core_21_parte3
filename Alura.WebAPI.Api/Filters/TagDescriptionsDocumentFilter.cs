using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Alura.ListaLeitura.Api.Filters
{
    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new[] {
            new Tag { Name = "Livros", Description = "Consulta e mantém os livros." },
            new Tag { Name = "Listas", Description = "Consulta as listas de leitura." }
        };
        }
    }
}
