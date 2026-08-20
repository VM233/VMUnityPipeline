using System;
using System.Collections.Generic;
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
                JToken token = JToken.Parse(json, settings);
                if (!(token is JObject jsonObject))
                {
                    errorMessage = "arguments_json must contain one JSON object.";
                    return false;
                }

                arguments = ConvertObject(jsonObject);
                return true;
            }
            catch (JsonException exception)
            {
                errorMessage = exception.GetBaseException().Message;
                return false;
            }
        }

        private static Dictionary<string, object> ConvertObject(JObject source)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (JProperty property in source.Properties())
                result.Add(property.Name, ConvertToken(property.Value));
            return result;
        }

        private static object ConvertToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ConvertObject((JObject)token);
                case JTokenType.Array:
                    var values = new List<object>();
                    foreach (JToken item in (JArray)token)
                        values.Add(ConvertToken(item));
                    return values;
                case JTokenType.Integer:
                    return token.Value<long>();
                case JTokenType.Float:
                    return token.Value<double>();
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
