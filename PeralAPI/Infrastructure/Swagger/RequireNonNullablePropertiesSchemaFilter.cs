using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PeralAPI.Infrastructure.Swagger
{
    public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;

            var nonNullableProperties = schema.Properties
                .Where(p => !p.Value.Nullable)
                .Select(p => p.Key);

            foreach (var property in nonNullableProperties)
            {
                if (!schema.Required.Contains(property))
                    schema.Required.Add(property);
            }
        }
    }
}
