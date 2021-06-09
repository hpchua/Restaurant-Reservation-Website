using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantReservation.Api.SwaggerConfigurations
{
    public class AuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the controller has Authorize attribute
            var isAuthorized = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!isAuthorized)
            {
                // If controller no Authorize attribute, check if the action has Authorize attribute
                isAuthorized = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
            }
            else
            {
                // If controller has Authorize attribute, check if the action has AllowAnonymous attribute
                isAuthorized = !context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
            }

            if (isAuthorized)
            {
                operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = "Forbidden" });

                // Add padlock icon
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, new List<string>()
                    }
                });
            }
        }
    }
}
