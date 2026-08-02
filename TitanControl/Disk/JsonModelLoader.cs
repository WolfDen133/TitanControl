using Json.Schema;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TitanControl.Logging;

namespace TitanControl.Disk
{
    public static class JsonModelLoader
    {
        private const string LoggingCategory = "JsonModelLoader";

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static T ParseAndValidate<T>(string json, JsonSchema schema)
        {
            JsonElement node = JsonElement.Parse(json);

            EvaluationResults results = schema.Evaluate(node, new EvaluationOptions()
            {
                OutputFormat = OutputFormat.List
            });

            if (!results.IsValid)
            {
                List<string> errors = results.Details!
                    .Where(detail => !detail.IsValid)
                    .Select(detail =>
                        $"{detail.InstanceLocation}: schema validation failed")
                    .ToList();

                var ex = new JsonSchemaException("Specified json is invalid");

                Log.Error(ex, $"JSON invalid against schema {schema}", LoggingCategory, new Dictionary<string, object?>()
                {
                    ["Errors"] = string.Join(",", errors)
                });
                throw ex;
            }

            var modelObject = JsonSerializer.Deserialize<T>(json, options);

            if (modelObject is null)
            {
                var ex = new InvalidDataException($"Invalid data to cast to object");
                Log.Error(ex, $"JSON could not deserialize to {typeof(T).Name}", LoggingCategory);
                throw ex;
            }

            return modelObject;
                
        }

        public static string Serialize<T>(T model)
        {
            return JsonSerializer.Serialize(model, options);
        }

    }
}
