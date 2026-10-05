using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCommandInvocation
    {
        [JsonProperty("command")]
        public string Command { get; }

        [JsonProperty("arguments")]
        public IReadOnlyDictionary<string, string> Arguments { get; }

        [JsonProperty("argumentsJsonParameter", NullValueHandling = NullValueHandling.Ignore)]
        public string ArgumentsJsonParameter { get; }

        private VmCommandInvocation(string command, Dictionary<string, string> arguments,
            string argumentsJsonParameter)
        {
            Command = command;
            Arguments = new ReadOnlyDictionary<string, string>(arguments);
            ArgumentsJsonParameter = argumentsJsonParameter;
        }

        public static VmCommandInvocation Direct(string command)
        {
            return new VmCommandInvocation(command, new Dictionary<string, string>(), null);
        }

        public static VmCommandInvocation Automation(string command)
        {
            return new VmCommandInvocation(VmAutomationCallCommand.CommandName,
                new Dictionary<string, string>
                {
                    { VmAutomationCallCommand.TargetCommandParameter, command }
                }, VmAutomationCallCommand.ArgumentsJsonParameter);
        }

        public static VmJsonSchema CreateSchema()
        {
            return VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>
            {
                { "command", VmJsonSchema.String("Registered native command to invoke through Unity CLI.") },
                { "arguments", VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>
                    {
                        { VmAutomationCallCommand.TargetCommandParameter,
                            VmJsonSchema.String("Exact owner identifier passed to the Automation facade.") }
                    }, description: "Fixed native parameters; preserve their names and values.") },
                { "argumentsJsonParameter", VmJsonSchema.String(
                    "When present, serialize inputSchema arguments as one JSON object in this native parameter. Otherwise pass them as direct native parameters.",
                    enumValues: new[] { VmAutomationCallCommand.ArgumentsJsonParameter }) }
            }, new[] { "command", "arguments" },
                "Official CLI invocation mapping. Preserve the caller's absolute project binding and the selected command's preconditions.");
        }
    }
}
