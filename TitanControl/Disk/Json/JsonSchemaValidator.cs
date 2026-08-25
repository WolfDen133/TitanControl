using Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TitanControl.Logging;

namespace TitanControl.Disk.Json
{
    public class JsonSchemaValidator
    {
        private const string LoggingCategory = "JsonValidator";

        public string Report = string.Empty;

        public EvaluationResults Evaluate(string json, JsonSchema schema)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            return schema.Evaluate(
                document.RootElement,
                new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List
                });
        }

        public bool IsValid(string json, JsonSchema schema)
        {
            try
            {
                EvaluationResults results = Evaluate(json, schema);

                if (results.IsValid)
                    return true;

                string report = BuildSchemaErrorReport(json, results);

                var ex = new JsonSchemaException(
                    "JSON failed schema validation.");

                Log.Error(
                    ex,
                    report,
                    LoggingCategory);

                return false;
            }
            catch (JsonException ex)
            {
                string report = BuildJsonParseErrorReport(json, ex);

                Log.Error(
                    ex,
                    report,
                    LoggingCategory);

                return false;
            }
        }

        private static string BuildSchemaErrorReport(
            string json,
            EvaluationResults results)
        {
            var builder = new StringBuilder();

            string formattedJson;

            try
            {
                JsonNode? node = JsonNode.Parse(json);

                formattedJson = node?.ToJsonString(
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }) ?? json;
            }
            catch
            {
                formattedJson = json;
            }

            string[] lines = formattedJson
                .Replace("\r\n", "\n")
                .Split('\n');

            builder.AppendLine("JSON schema validation failed:");
            builder.AppendLine();

            foreach (string line in lines)
                builder.AppendLine(line);

            builder.AppendLine();

            foreach (EvaluationResults detail in results.Details!
                         .Where(x => !x.IsValid))
            {
                if (detail.Errors is null ||
                    detail.Errors.Count == 0)
                {
                    continue;
                }

                string path = detail.InstanceLocation.ToString();

                AppendSchemaError(
                    builder,
                    lines,
                    path,
                    detail.Errors);
            }

            return builder.ToString();
        }

        private static void AppendSchemaError(
            StringBuilder builder,
            string[] lines,
            string jsonPath,
            IReadOnlyDictionary<string, string> errors)
        {
            string propertyName =
                GetLastJsonPathComponent(jsonPath);

            int lineIndex =
                FindPropertyLine(lines, propertyName);

            if (lineIndex >= 0)
            {
                string line = lines[lineIndex];

                int propertyIndex = line.IndexOf(
                    $"\"{propertyName}\"",
                    StringComparison.Ordinal);

                if (propertyIndex < 0)
                    propertyIndex = 0;

                builder.AppendLine(
                    $"Line {lineIndex + 1}:");

                builder.AppendLine(line);

                builder.AppendLine(
                    new string('_', propertyIndex) +
                    "^");
            }

            foreach ((string keyword, string message) in errors)
            {
                builder.AppendLine(
                    $"└─ {jsonPath} [{keyword}]: {message}");
            }

            builder.AppendLine();
        }

        private static int FindPropertyLine(
            string[] lines,
            string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return -1;

            string search = $"\"{propertyName}\"";

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(
                    search,
                    StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetLastJsonPathComponent(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string part = path
                .Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault()
                ?? string.Empty;

            return part
                .Replace("~1", "/")
                .Replace("~0", "~");
        }

        private static string BuildJsonParseErrorReport(
            string json,
            JsonException exception)
        {
            var builder = new StringBuilder();

            string[] lines = json
                .Replace("\r\n", "\n")
                .Split('\n');

            builder.AppendLine("Malformed JSON:");
            builder.AppendLine();

            foreach (string line in lines)
                builder.AppendLine(line);

            builder.AppendLine();

            long lineNumber = exception.LineNumber ?? 0;
            long bytePosition = exception.BytePositionInLine ?? 0;

            if (lineNumber >= 0 &&
                lineNumber < lines.Length)
            {
                string line = lines[lineNumber];

                builder.AppendLine(
                    $"Line {lineNumber + 1}:");

                builder.AppendLine(line);

                builder.AppendLine(
                    new string('_', (int)bytePosition) +
                    "^");
            }

            builder.AppendLine(
                $"└─ {exception.Message}");

            return builder.ToString();
        }


    }
}
