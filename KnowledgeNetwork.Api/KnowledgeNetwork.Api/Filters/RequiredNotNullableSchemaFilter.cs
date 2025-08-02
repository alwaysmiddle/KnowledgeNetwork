using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace KnowledgeNetwork.Api.Filters;

/// <summary>
/// Schema filter to mark required properties that are not nullable
/// This enhances the OpenAPI documentation by clearly indicating required fields
/// </summary>
public class RequiredNotNullableSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies the filter to modify the schema for better documentation
    /// </summary>
    /// <param name="schema">The OpenAPI schema to modify</param>
    /// <param name="context">The schema filter context</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
            return;

        var requiredProperties = context.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => 
                prop.GetCustomAttribute<RequiredAttribute>() != null ||
                (prop.PropertyType.IsValueType && 
                 Nullable.GetUnderlyingType(prop.PropertyType) == null))
            .Select(prop => prop.Name.ToLowerInvariant())
            .ToList();

        foreach (var property in schema.Properties)
        {
            if (requiredProperties.Contains(property.Key.ToLowerInvariant()))
            {
                property.Value.Nullable = false;
                
                // Add the property to the required list if it's not already there
                schema.Required ??= new HashSet<string>();
                if (!schema.Required.Contains(property.Key))
                {
                    schema.Required.Add(property.Key);
                }
            }
        }
    }
}