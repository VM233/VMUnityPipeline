using System.Collections.Generic;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmEditorStateCommand
    {
        public const string CommandName = "vm_editor_state";
        public const string Description =
            "Return explicit Unity Editor play, pause, transition, compilation, update, scene, platform, and project state.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "editor" },
            VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>()),
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the state snapshot was produced.") },
                    { "isIdle", VmJsonSchema.Boolean("Whether compilation, asset update, and play-mode transition are all inactive.") },
                    { "isPlaying", VmJsonSchema.Boolean("Current EditorApplication.isPlaying value.") },
                    { "isPaused", VmJsonSchema.Boolean("Current EditorApplication.isPaused value.") },
                    { "isCompiling", VmJsonSchema.Boolean("Current EditorApplication.isCompiling value.") },
                    { "isUpdating", VmJsonSchema.Boolean("Current EditorApplication.isUpdating value.") },
                    { "isChangingPlayMode", VmJsonSchema.Boolean("Whether play mode is currently transitioning.") },
                    { "isPlayingOrWillChangePlaymode", VmJsonSchema.Boolean("Current EditorApplication.isPlayingOrWillChangePlaymode value.") },
                    { "activeScene", VmJsonSchema.String("Active scene name.") },
                    { "activeScenePath", VmJsonSchema.String("Active scene project path.") },
                    { "sceneDirty", VmJsonSchema.Boolean("Whether the active scene has unsaved changes.") },
                    { "unityVersion", VmJsonSchema.String("Running Unity Editor version.") },
                    { "platform", VmJsonSchema.String("Active build target.") },
                    { "projectPath", VmJsonSchema.String("Absolute Unity project path.") }
                },
                new[]
                {
                    "ok",
                    "isIdle",
                    "isPlaying",
                    "isPaused",
                    "isCompiling",
                    "isUpdating",
                    "isChangingPlayMode",
                    "isPlayingOrWillChangePlaymode",
                    "activeScene",
                    "activeScenePath",
                    "sceneDirty",
                    "unityVersion",
                    "platform",
                    "projectPath"
                }),
            new string[0],
            new[] { "read", "editor_state" },
            new[] { "pipeline_connected", "editor_main_thread" },
            "Returns one state snapshot from the current Editor update.",
            transactionDurability: "editor-update");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = true,
            Tags = new[] { "editor" })]
        public static VmEditorStateResult Execute()
        {
            var activeScene = SceneManager.GetActiveScene();
            var isPlaying = EditorApplication.isPlaying;
            var isPlayingOrWillChangePlaymode =
                EditorApplication.isPlayingOrWillChangePlaymode;
            var isChangingPlayMode = isPlayingOrWillChangePlaymode != isPlaying;
            var isCompiling = EditorApplication.isCompiling;
            var isUpdating = EditorApplication.isUpdating;
            var projectPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));

            return new VmEditorStateResult(
                !isCompiling && !isUpdating && !isChangingPlayMode,
                isPlaying,
                EditorApplication.isPaused,
                isCompiling,
                isUpdating,
                isChangingPlayMode,
                isPlayingOrWillChangePlaymode,
                activeScene.name,
                activeScene.path,
                activeScene.isDirty,
                Application.unityVersion,
                EditorUserBuildSettings.activeBuildTarget.ToString(),
                projectPath);
        }
    }
}
