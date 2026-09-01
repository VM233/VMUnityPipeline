using System.Collections.Generic;
using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using Unity.Pipeline.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return VmRemoveMissingScriptsResult.Failure(
                    "edit_mode_required",
                    "Missing-script cleanup requires stable Edit Mode.");
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return VmRemoveMissingScriptsResult.Failure(
                    "editor_not_stable",
                    "Missing-script cleanup cannot run while Unity is compiling or updating assets.");
            }

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

            var scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return VmRemoveMissingScriptsResult.Failure(
                    "loaded_scene_target_required",
                    "The target must belong to a loaded scene; prefab assets are not accepted.");
            }

            var identity = ObjectResolver.Describe(gameObject);
            var missingCount =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount == 0)
            {
                return VmRemoveMissingScriptsResult.Success(
                    0,
                    identity.HierarchyPath,
                    scene.path,
                    scene.isDirty);
            }

            Undo.RegisterCompleteObjectUndo(gameObject, "Remove Missing Scripts");
            var removedCount =
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            EditorUtility.SetDirty(gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            return VmRemoveMissingScriptsResult.Success(
                removedCount,
                identity.HierarchyPath,
                scene.path,
                scene.isDirty);
        }
    }
}
