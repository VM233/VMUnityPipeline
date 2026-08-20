using System.Collections.Generic;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmCommandContractSchema
    {
        public static VmJsonSchema CreateCatalogGetOutputSchema()
        {
            var definitions = new Dictionary<string, VmJsonSchema>
            {
                { "jsonSchema", CreateJsonSchemaDefinition() },
                { "commandContract", CreateCommandContractDefinition() }
            };

            return VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the domain operation succeeded.") },
                    { "found", VmJsonSchema.Boolean("Whether the exact command contract exists.") },
                    { "contract", VmJsonSchema.ReferenceTo("#/$defs/commandContract") },
                    { "errorCode", VmJsonSchema.String("Stable domain error code when not found.") },
                    { "errorMessage", VmJsonSchema.String("Developer-facing domain error message when not found.") }
                },
                new[] { "ok", "found" },
                definitions: definitions);
        }

        private static VmJsonSchema CreateCommandContractDefinition()
        {
            return VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "name", VmJsonSchema.String("Stable command name.") },
                    { "description", VmJsonSchema.String("Command purpose.") },
                    { "package", VmJsonSchema.String("Owning UPM package identifier.") },
                    { "tags", VmJsonSchema.Array(VmJsonSchema.String("Hierarchical discovery tag.")) },
                    { "inputSchema", VmJsonSchema.ReferenceTo("#/$defs/jsonSchema") },
                    { "outputSchema", VmJsonSchema.ReferenceTo("#/$defs/jsonSchema") },
                    { "errorCodes", VmJsonSchema.Array(VmJsonSchema.String("Stable domain error code.")) },
                    { "sideEffects", VmJsonSchema.Array(VmJsonSchema.String("Declared side effect.")) },
                    { "preconditions", VmJsonSchema.Array(VmJsonSchema.String("Required execution condition.")) },
                    { "completion", VmJsonSchema.String("Success evidence contract.") },
                    { "transactionScope", VmJsonSchema.String("Transaction ownership boundary.") },
                    { "transactionAtomicity", VmJsonSchema.String("Atomicity guarantee.") },
                    { "transactionIsolation", VmJsonSchema.String("Isolation guarantee.") },
                    { "transactionDurability", VmJsonSchema.String("Durability boundary.") },
                    { "transactionRollbackKind", VmJsonSchema.String("Rollback mechanism.") }
                },
                new[]
                {
                    "name",
                    "description",
                    "package",
                    "tags",
                    "inputSchema",
                    "outputSchema",
                    "errorCodes",
                    "sideEffects",
                    "preconditions",
                    "completion",
                    "transactionScope",
                    "transactionAtomicity",
                    "transactionIsolation",
                    "transactionDurability",
                    "transactionRollbackKind"
                });
        }

        private static VmJsonSchema CreateJsonSchemaDefinition()
        {
            return VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "type", VmJsonSchema.Any("JSON value type or allowed type array.") },
                    { "$ref", VmJsonSchema.String("Local schema reference.") },
                    {
                        "$defs",
                        VmJsonSchema.Object(
                            new Dictionary<string, VmJsonSchema>(),
                            description: "Named reusable schemas.",
                            additionalProperties: VmJsonSchema.ReferenceTo("#/$defs/jsonSchema"))
                    },
                    { "description", VmJsonSchema.String("Human-readable schema purpose.") },
                    {
                        "properties",
                        VmJsonSchema.Object(
                            new Dictionary<string, VmJsonSchema>(),
                            description: "Object property schemas.",
                            additionalProperties: VmJsonSchema.ReferenceTo("#/$defs/jsonSchema"))
                    },
                    { "required", VmJsonSchema.Array(VmJsonSchema.String("Required property name.")) },
                    { "items", VmJsonSchema.ReferenceTo("#/$defs/jsonSchema") },
                    { "default", VmJsonSchema.Any("Default JSON value.") },
                    { "minimum", VmJsonSchema.Number("Inclusive numeric minimum.") },
                    { "maximum", VmJsonSchema.Number("Inclusive numeric maximum.") },
                    { "enum", VmJsonSchema.Array(VmJsonSchema.Any("Allowed JSON value.")) },
                    { "additionalProperties", VmJsonSchema.Any("Boolean or schema-valued additional-properties rule.") }
                },
                additionalProperties: VmJsonSchema.Any("Additional JSON Schema keyword."));
        }
    }
}
