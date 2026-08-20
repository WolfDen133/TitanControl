using Avalonia.Platform;
using Json.Schema;
using System;
using System.Collections.Concurrent;
using System.IO;
using TitanControl.Logging;

namespace TitanControl.Disk
{
    public static class SchemaLoader
    {
        private const string LoggingCategory = "SchemaLoader";

        private static readonly ConcurrentDictionary<string, JsonSchema> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        public static JsonSchema Load(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentException("Schema name cannot be empty.", nameof(schemaName));

            return Cache.GetOrAdd(schemaName, LoadFromAssets);
        }

        private static JsonSchema LoadFromAssets(string schemaName)
        {
            var uri = new Uri(
                $"avares://{AppConstants.AppName}/Assets/Schemas/{schemaName}.schema.json");

            if (!AssetLoader.Exists(uri))
            {
                var ex = new FileNotFoundException(
                    $"Schema asset '{schemaName}.schema.json' was not found.");

                Log.Error(
                    ex,
                    $"Schema load failed: {schemaName}",
                    LoggingCategory);

                throw ex;
            }

            using Stream stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);

            JsonSchema schema = JsonSchema.FromText(reader.ReadToEnd());

            Log.Debug(
                $"Loaded schema asset: {schemaName}.schema.json",
                LoggingCategory);

            return schema;
        }
    }
}
