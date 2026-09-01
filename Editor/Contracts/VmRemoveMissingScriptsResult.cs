using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmRemoveMissingScriptsResult : VmCommandResult
    {
        [JsonProperty("removedCount")]
        public int RemovedCount { get; }

        [JsonProperty("hierarchyPath", NullValueHandling = NullValueHandling.Ignore)]
        public string HierarchyPath { get; }

        [JsonProperty("scenePath", NullValueHandling = NullValueHandling.Ignore)]
        public string ScenePath { get; }

        [JsonProperty("sceneDirty")]
        public bool SceneDirty { get; }

        private VmRemoveMissingScriptsResult(
            bool ok,
            int removedCount,
            string hierarchyPath,
            string scenePath,
            bool sceneDirty,
            string errorCode = null,
            string errorMessage = null)
            : base(ok, errorCode, errorMessage)
        {
            RemovedCount = removedCount;
            HierarchyPath = hierarchyPath;
            ScenePath = scenePath;
            SceneDirty = sceneDirty;
        }

        public static VmRemoveMissingScriptsResult Success(
            int removedCount,
            string hierarchyPath,
            string scenePath,
            bool sceneDirty)
        {
            return new VmRemoveMissingScriptsResult(
                true,
                removedCount,
                hierarchyPath,
                scenePath,
                sceneDirty);
        }

        public static VmRemoveMissingScriptsResult Failure(
            string errorCode,
            string errorMessage)
        {
            return new VmRemoveMissingScriptsResult(
                false,
                0,
                null,
                null,
                false,
                errorCode,
                errorMessage);
        }
    }
}
