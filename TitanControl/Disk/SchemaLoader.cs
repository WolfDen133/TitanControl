using Avalonia.Platform;
using Json.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;

namespace TitanControl.Disk
{
    public static class SchemaLoader
    {
        private static readonly ConcurrentDictionary<string, Json.Schema.JsonSchema> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        public static JsonSchema Load(string schemaName)
        {
            return Cache.GetOrAdd(schemaName, LoadFromAssets);
        }

        private static JsonSchema LoadFromAssets(string schemaName)
        {
            var uri = new Uri(
                $"avares://{AppConstants.AppName}/Assets/Schemas/{schemaName}.schema.json");

            if (!AssetLoader.Exists(uri))
            {
                var ex = new FileNotFoundException($"Schema file not found for {schemaName}");
                Log.Error(ex, $"Could not load schema file for {schemaName}");
                throw ex;
            }

            using Stream stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);

            return JsonSchema.FromText(reader.ReadToEnd());
        }
    }
}
