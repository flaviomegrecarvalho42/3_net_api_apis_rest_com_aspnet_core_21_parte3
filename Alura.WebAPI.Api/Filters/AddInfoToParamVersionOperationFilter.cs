using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Alura.ListaLeitura.Api.Filters
{
    public class AddInfoToParamVersionOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var paramVersion = operation.Parameters
                .Where(p => p.Name == "version")
                .FirstOrDefault();

            if (paramVersion != null)
            {
                paramVersion.Description = "Versão da API";
            }
        }
    }
}
