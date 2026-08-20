using System;
using UnityEditor.PackageManager;

namespace VMUnityPipeline.Editor
{
    internal static class VmUnityPipelineInfo
    {
        public const string PackageId = "com.vm233.unity-pipeline";
        public const int ContractVersion = 1;

        public static string PackageVersion
        {
            get
            {
                var packageInfo = PackageInfo.FindForAssembly(
                    typeof(VmUnityPipelineInfo).Assembly);

                if (packageInfo == null)
                {
                    throw new InvalidOperationException(
                        $"Could not resolve the owning UPM package for '{PackageId}'.");
                }

                return packageInfo.version;
            }
        }
    }
}
