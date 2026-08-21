using Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TitanControl.Logging;
using TitanControl.Models;
using TitanControl.Models.Session;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk
{
    public class JsonModelLoader
    {
        private const string LoggingCategory = "JsonModelHelper";

        private readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public T ParseAndValidate<T>(string json, JsonSchema schema)
        {
            return (T)ParseAndValidate(typeof(T), json, schema);
        }

        public ISaveModel ParseAndValidate(
            Type type,
            string json,
            JsonSchema schema)
        {
            if (!typeof(ISaveModel).IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    $"{type.Name} must implement {nameof(ISaveModel)}.",
                    nameof(type));
            }

            EvaluationResults results = Evaluate(json, schema);

            if (!results.IsValid)
            {
                List<string> errors = GetValidationErrors(results);
                var ex = new JsonSchemaException("Specified JSON is invalid.");

                Log.Error(
                    ex,
                    $"JSON failed schema validation for {type.Name}.",
                    LoggingCategory,
                    new Dictionary<string, object?>
                    {
                        ["Errors"] = string.Join(", ", errors),
                    });

                throw ex;
            }

            object? modelObject = JsonSerializer.Deserialize(json, type, Options);

            if (modelObject is not ISaveModel saveModel)
            {
                var ex = new InvalidDataException(
                    $"JSON could not be deserialized to {type.Name}.");

                Log.Error(
                    ex,
                    $"Deserialization failed for {type.Name}.",
                    LoggingCategory);

                throw ex;
            }

            return saveModel;
        }

        /// <summary>
        /// Returns false for invalid schema data and malformed JSON. Schema/configuration errors
        /// themselves are intentionally not swallowed.
        /// </summary>
        public bool IsValid(string json, JsonSchema schema)
        {
            try
            {
                return Evaluate(json, schema).IsValid;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Serializes using the runtime type rather than the generic/interface type. This prevents
        /// ISaveModel-typed values from being emitted as an empty object.
        /// </summary>
        public string Serialize<T>(T model)
            where T : class, ISaveModel
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            return JsonSerializer.Serialize(
                model,
                model.GetType(),
                Options);
        }

        public string MigrateJson(Type modelType, string rawJson)
        {
            JsonNode node = JsonNode.Parse(rawJson)
                ?? throw new InvalidDataException("Could not parse JSON for migration.");

            return modelType switch
            {
                Type t when t == typeof(SessionRecordModel)
                    => MigrateSessionRecord(node),

                Type t when t == typeof(WorkspaceRecordModel)
                    => MigrateWorkspaceRecord(node),

                Type t when t == typeof(WorkspaceModel)
                    => MigrateWorkspace(node),

                _ => rawJson
            };
        }

        private EvaluationResults Evaluate(string json, JsonSchema schema)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            return schema.Evaluate(
                document.RootElement,
                new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List
                });
        }

        private List<string> GetValidationErrors(EvaluationResults results)
        {
            return results.Details?
                .Where(detail => !detail.IsValid)
                .SelectMany(detail =>
                    detail.Errors?.Select(error =>
                        $"{detail.InstanceLocation} [{error.Key}]: {error.Value}")
                    ?? [])
                .ToList()
                ?? [];
        }

        private string MigrateWorkspace(JsonNode json)
        {
            if (json is not JsonObject)
            {
                var ex = new InvalidDataException(
                    "Legacy workspace JSON root must be an object.");

                Log.Error(ex, "Workspace migration failed.", LoggingCategory);
                throw ex;
            }

            WorkspaceModel? model = json.Deserialize<WorkspaceModel>(Options);

            if (model is null)
            {
                throw new InvalidDataException(
                    "Could not deserialize legacy workspace data.");
            }

            return Serialize(model);
        }

        private string MigrateWorkspaceRecord(JsonNode json)
        {
            if (json is not JsonObject)
            {
                var ex = new InvalidDataException(
                    "Legacy workspace record JSON root must be an object.");

                Log.Error(ex, "Workspace record migration failed.", LoggingCategory);
                throw ex;
            }

            WorkspaceRecordModel? model = json.Deserialize<WorkspaceRecordModel>(Options);

            if (model is null)
            {
                throw new InvalidDataException(
                    "Could not deserialize legacy workspace record data.");
            }

            return Serialize(model);
        }

        private string MigrateSessionRecord(JsonNode json)
        {
            if (json is not JsonArray data)
            {
                var ex = new InvalidDataException(
                    "Legacy session record JSON root must be an array.");

                Log.Error(ex, "Session record migration failed.", LoggingCategory);
                throw ex;
            }

            var migratedSessions = new JsonArray();

            foreach (JsonNode? item in data)
            {
                if (item is null)
                    continue;

                SessionSaveModel? model = item.Deserialize<SessionSaveModel>(Options);

                if (model is null)
                {
                    throw new InvalidDataException(
                        "Could not deserialize a legacy session entry.");
                }

                JsonNode? migratedNode = JsonSerializer.SerializeToNode(model, Options);

                if (migratedNode is not null)
                    migratedSessions.Add(migratedNode);
            }

            return new JsonObject
            {
                ["lastSession"] = Guid.Empty,
                ["sessions"] = migratedSessions
            }.ToJsonString(Options);
        }
    }
}
