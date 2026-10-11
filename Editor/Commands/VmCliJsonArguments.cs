using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCliJsonArguments
    {
        private const int MaximumFileBytes = 4 * 1024 * 1024;

        public static bool TryReadSource(string json, string file, string projectPath,
            out string source, out string code, out string error)
        {
            source = json ?? "{}"; code = null; error = null;
            if (file == null) return true;
            if (json != null)
            {
                code = "argument_conflict"; error = "Specify arguments_json or arguments_file, not both.";
                return false;
            }
            try
            {
                if (!Path.IsPathRooted(file)) throw new ArgumentException("arguments_file requires an absolute path.");
                string path = Path.GetFullPath(file);
                string root = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (!path.StartsWith(root, comparison)) throw new ArgumentException("arguments_file must belong to the bound project.");
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Length > MaximumFileBytes)
                {
                    code = "input_validation_limit"; error = "arguments_file exceeds 4 MiB.";
                    return false;
                }
                using var reader = new StreamReader(stream, new UTF8Encoding(false, true), false);
                source = reader.ReadToEnd();
                if (source.Length > 0 && source[0] == '\ufeff') source = source.Substring(1);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                code = "invalid_arguments_file"; error = exception.Message;
                return false;
            }
        }

        public static bool TryParseObject(
            string json,
            out Dictionary<string, object> arguments,
            out string errorMessage)
        {
            arguments = null;
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(json))
                json = "{}";

            try
            {
                var settings = new JsonLoadSettings
                {
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
                };
                using var reader = new JsonTextReader(new StringReader(json))
                {
                    DateParseHandling = DateParseHandling.None
                };
                JToken token = JToken.ReadFrom(reader, settings);
                // Preserve Parse's single-document validation, including trailing data.
                while (reader.Read()) { }
                if (!(token is JObject jsonObject))
                {
                    errorMessage = "Owner arguments must contain one JSON object.";
                    return false;
                }

                arguments = ConvertObject(jsonObject, new VmCliJsonNumberSource(json));
                return true;
            }
            catch (JsonException exception)
            {
                errorMessage = exception.GetBaseException().Message;
                return false;
            }
            catch (FormatException exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        private static Dictionary<string, object> ConvertObject(JObject source, VmCliJsonNumberSource numbers)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (JProperty property in source.Properties())
                result.Add(property.Name, ConvertToken(property.Value, numbers));
            return result;
        }

        private static object ConvertToken(JToken token, VmCliJsonNumberSource numbers)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ConvertObject((JObject)token, numbers);
                case JTokenType.Array:
                    var values = new List<object>();
                    foreach (JToken item in (JArray)token)
                        values.Add(ConvertToken(item, numbers));
                    return values;
                case JTokenType.Integer:
                case JTokenType.Float:
                    return numbers.Read(token);
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                case JTokenType.String:
                    return token.Value<string>();
                default:
                    return token.ToString(Formatting.None);
            }
        }
    }
}
