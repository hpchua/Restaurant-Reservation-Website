using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;

namespace RestaurantReservation.Api.SwaggerConfigurations
{
    public class GenerateJsonFilter : IDocumentFilter
    {
        private readonly IWebHostEnvironment _environment;

        public GenerateJsonFilter(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (_environment.IsDevelopment())
                GenerateJson(swaggerDoc);
        }

        private void GenerateJson(OpenApiDocument swaggerDoc)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, $"Swagger{swaggerDoc.Info.Version}.json");
            var jsonContent = swaggerDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            if (!File.Exists(filePath) || string.Compare(File.ReadAllText(filePath), jsonContent, StringComparison.OrdinalIgnoreCase) != 0)
            {
                File.WriteAllText(filePath, jsonContent);
            }
        }
    }
}
