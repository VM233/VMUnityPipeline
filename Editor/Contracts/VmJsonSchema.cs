using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmJsonSchema
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; }

        [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Reference { get; }

        [JsonProperty("$defs", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<string, VmJsonSchema> Definitions { get; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<string, VmJsonSchema> Properties { get; }

        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Required { get; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public VmJsonSchema Items { get; }

        [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
        public object DefaultValue { get; }

        [JsonProperty("minimum", NullValueHandling = NullValueHandling.Ignore)]
        public double? Minimum { get; }

        [JsonProperty("maximum", NullValueHandling = NullValueHandling.Ignore)]
        public double? Maximum { get; }

        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> EnumValues { get; }

        [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
        public object AdditionalProperties { get; }

        private VmJsonSchema(
            string type,
            string reference,
            IReadOnlyDictionary<string, VmJsonSchema> definitions,
            string description,
            IReadOnlyDictionary<string, VmJsonSchema> properties,
            IReadOnlyList<string> required,
            VmJsonSchema items,
            object defaultValue,
            double? minimum,
            double? maximum,
            IReadOnlyList<string> enumValues,
            object additionalProperties)
        {
            Type = type;
            Reference = reference;
            Definitions = definitions;
            Description = description;
            Properties = properties;
            Required = required;
            Items = items;
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
            EnumValues = enumValues;
            AdditionalProperties = additionalProperties;
        }

        public static VmJsonSchema Object(
            IDictionary<string, VmJsonSchema> properties,
            string[] required = null,
            string description = null,
            object additionalProperties = null,
            IDictionary<string, VmJsonSchema> definitions = null)
        {
            return new VmJsonSchema(
                "object",
                null,
                definitions == null
                    ? null
                    : new ReadOnlyDictionary<string, VmJsonSchema>(
                        new Dictionary<string, VmJsonSchema>(definitions, StringComparer.Ordinal)),
                description,
                new ReadOnlyDictionary<string, VmJsonSchema>(
                    new Dictionary<string, VmJsonSchema>(properties, StringComparer.Ordinal)),
                System.Array.AsReadOnly(required ?? System.Array.Empty<string>()),
                null,
                null,
                null,
                null,
                null,
                additionalProperties ?? false);
        }

        public static VmJsonSchema ReferenceTo(string reference)
        {
            return new VmJsonSchema(
                null,
                reference,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        public static VmJsonSchema Array(VmJsonSchema items, string description = null)
        {
            return new VmJsonSchema(
                "array",
                null,
                null,
                description,
                null,
                null,
                items,
                null,
                null,
                null,
                null,
                null);
        }

        public static VmJsonSchema String(
            string description,
            string defaultValue = null,
            string[] enumValues = null)
        {
            return new VmJsonSchema(
                "string",
                null,
                null,
                description,
                null,
                null,
                null,
                defaultValue,
                null,
                null,
                enumValues == null ? null : System.Array.AsReadOnly(enumValues),
                null);
        }

        public static VmJsonSchema Integer(
            string description,
            int? defaultValue = null,
            int? minimum = null,
            int? maximum = null)
        {
            return new VmJsonSchema(
                "integer",
                null,
                null,
                description,
                null,
                null,
                null,
                defaultValue,
                minimum,
                maximum,
                null,
                null);
        }

        public static VmJsonSchema Number(string description)
        {
            return new VmJsonSchema(
                "number",
                null,
                null,
                description,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        public static VmJsonSchema Any(string description)
        {
            return new VmJsonSchema(
                null,
                null,
                null,
                description,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        public static VmJsonSchema Boolean(string description)
        {
            return new VmJsonSchema(
                "boolean",
                null,
                null,
                description,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }
}
