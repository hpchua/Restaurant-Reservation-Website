using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace RestaurantReservation.Api.SwaggerConfigurations
{
    public class SwaggerGeneralConfig : IConfigureOptions<SwaggerGenOptions>
    {
        public IApiVersionDescriptionProvider ApiVersionProvider { get; }

        public SwaggerGeneralConfig(IApiVersionDescriptionProvider apiVersionProvider)
        {
            ApiVersionProvider = apiVersionProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            //This is to generate the Default UI of Swagger Documentation 
            foreach (var desc in ApiVersionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(name: desc.GroupName, new OpenApiInfo
                {
                    Version = desc.GroupName,
                    Title = $"Restaurant Reservation API {desc.GroupName}",
                    Description = "Restaurant Reservation API"
                });
            }

            options.DocumentFilter<GenerateJsonFilter>();
            options.OperationFilter<ApiVersionOperationFilter>();
            options.OperationFilter<AuthorizationOperationFilter>();
            options.CustomOperationIds(apiDescription => apiDescription.ActionDescriptor.RouteValues["action"]);

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            // Add the authorize button at top right of swagger document
            // To Enable authorization using Swagger (JWT)  
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: Bearer 12345abcdef",
            });
        }
    }
}
