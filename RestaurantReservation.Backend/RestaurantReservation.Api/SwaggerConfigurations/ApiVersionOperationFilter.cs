using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace RestaurantReservation.Api.SwaggerConfigurations
{
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;

            var apiVersionParam = operation.Parameters.FirstOrDefault(p => p.Name == "api-version");
            var apiVersionParamDescription = context.ApiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == "api-version");

            if (apiVersionParam != null)
            {
                if (apiVersionParam.Schema.Default == null && apiVersionParamDescription.DefaultValue != null)
                {
                    apiVersionParam.Schema.Default = OpenApiAnyFactory.CreateFor(apiVersionParam.Schema, apiVersionParamDescription.DefaultValue);
                }
            }
        }
    }
}
