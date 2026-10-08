using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCliJsonArguments
    {
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
                    errorMessage = "arguments_json must contain one JSON object.";
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
