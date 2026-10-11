using System.Collections.Generic;
using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using Unity.Pipeline.Models;
using UnityEngine;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmRemoveMissingScriptsCommand
    {
        public const string CommandName = "vm_remove_missing_scripts";
        public const string Description =
            "Remove every missing MonoBehaviour slot from one loaded-scene GameObject, register Undo, and mark its scene dirty without saving it.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            VmCommandInvocation.Direct(CommandName),
            Description,
            new[] { "editor/scene/components" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    {
                        "target",
                        VmJsonSchema.Any(
                            "Official Pipeline ObjectRef for one loaded-scene GameObject.")
                    }
                },
                new[] { "target" }),
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the operation completed.") },
                    { "removedCount", VmJsonSchema.Integer("Number of missing MonoBehaviour slots removed.") },
                    { "hierarchyPath", VmJsonSchema.String("Resolved scene hierarchy path.") },
                    { "scenePath", VmJsonSchema.String("Owning scene path, if the scene has been saved before.") },
                    { "sceneDirty", VmJsonSchema.Boolean("Whether the owning scene is dirty after the operation.") },
                    { "errorCode", VmJsonSchema.String("Stable domain error code when ok is false.") },
                    { "errorMessage", VmJsonSchema.String("Domain error detail when ok is false.") }
                },
                new[] { "ok", "removedCount", "sceneDirty" }),
            new[]
            {
                "edit_mode_required",
                "editor_not_stable",
                "target_not_found",
                "target_not_game_object",
                "loaded_scene_target_required"
            },
            new[] { "sceneMutation", "undoRegistration" },
            new[]
            {
                "pipeline_connected",
                "editor_main_thread",
                "stable_edit_mode",
                "loaded_scene_target"
            },
            "Returns after every missing MonoBehaviour slot on the target has been removed and the scene has been marked dirty; the caller still owns save_scene.",
            transactionScope: "one-loaded-scene-game-object",
            transactionAtomicity: "single-editor-operation",
            transactionIsolation: "editor-main-thread",
            transactionDurability: "scene-save-required",
            transactionRollbackKind: "unity-undo");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = true,
            Tags = new[] { "editor/scene/components" })]
        public static VmRemoveMissingScriptsResult Execute(
            [CliArg(
                "target",
                "Official Pipeline ObjectRef for one loaded-scene GameObject.",
                Required = true)] ObjectRef target)
        {
            if (!ObjectResolver.TryResolve(target, out var resolved, out var error))
            {
                return VmRemoveMissingScriptsResult.Failure(
                    "target_not_found",
                    error);
            }

            var gameObject = resolved as GameObject;
            if (gameObject == null)
            {
                return VmRemoveMissingScriptsResult.Failure(
                    "target_not_game_object",
                    "The target must resolve directly to a GameObject.");
            }

            var identity = ObjectResolver.Describe(gameObject);
            return VmMissingScriptsRemoval.Execute(gameObject, identity.HierarchyPath);
        }
    }
}
