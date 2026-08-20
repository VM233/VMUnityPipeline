using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmEditorStateResult : VmCommandResult
    {
        [JsonProperty("isIdle")]
        public bool IsIdle { get; }

        [JsonProperty("isPlaying")]
        public bool IsPlaying { get; }

        [JsonProperty("isPaused")]
        public bool IsPaused { get; }

        [JsonProperty("isCompiling")]
        public bool IsCompiling { get; }

        [JsonProperty("isUpdating")]
        public bool IsUpdating { get; }

        [JsonProperty("isChangingPlayMode")]
        public bool IsChangingPlayMode { get; }

        [JsonProperty("isPlayingOrWillChangePlaymode")]
        public bool IsPlayingOrWillChangePlaymode { get; }

        [JsonProperty("activeScene")]
        public string ActiveScene { get; }

        [JsonProperty("activeScenePath")]
        public string ActiveScenePath { get; }

        [JsonProperty("sceneDirty")]
        public bool SceneDirty { get; }

        [JsonProperty("unityVersion")]
        public string UnityVersion { get; }

        [JsonProperty("platform")]
        public string Platform { get; }

        [JsonProperty("projectPath")]
        public string ProjectPath { get; }

        public VmEditorStateResult(
            bool isIdle,
            bool isPlaying,
            bool isPaused,
            bool isCompiling,
            bool isUpdating,
            bool isChangingPlayMode,
            bool isPlayingOrWillChangePlaymode,
            string activeScene,
            string activeScenePath,
            bool sceneDirty,
            string unityVersion,
            string platform,
            string projectPath)
            : base(true)
        {
            IsIdle = isIdle;
            IsPlaying = isPlaying;
            IsPaused = isPaused;
            IsCompiling = isCompiling;
            IsUpdating = isUpdating;
            IsChangingPlayMode = isChangingPlayMode;
            IsPlayingOrWillChangePlaymode = isPlayingOrWillChangePlaymode;
            ActiveScene = activeScene;
            ActiveScenePath = activeScenePath;
            SceneDirty = sceneDirty;
            UnityVersion = unityVersion;
            Platform = platform;
            ProjectPath = projectPath;
        }
    }
}
