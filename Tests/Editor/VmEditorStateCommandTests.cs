using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmEditorStateCommandTests
    {
        [Test]
        public void Execute_ReportsIndependentEditorStateFacts()
        {
            var result = VmEditorStateCommand.Execute();
            var expectedProjectPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));

            Assert.That(result.Ok, Is.True);
            Assert.That(result.IsPlaying, Is.EqualTo(EditorApplication.isPlaying));
            Assert.That(result.IsPaused, Is.EqualTo(EditorApplication.isPaused));
            Assert.That(result.IsCompiling, Is.EqualTo(EditorApplication.isCompiling));
            Assert.That(result.IsUpdating, Is.EqualTo(EditorApplication.isUpdating));
            Assert.That(
                result.IsChangingPlayMode,
                Is.EqualTo(
                    EditorApplication.isPlayingOrWillChangePlaymode !=
                    EditorApplication.isPlaying));
            Assert.That(
                result.IsIdle,
                Is.EqualTo(
                    !result.IsCompiling &&
                    !result.IsUpdating &&
                    !result.IsChangingPlayMode));
            Assert.That(result.ProjectPath, Is.EqualTo(expectedProjectPath));
            Assert.That(result.UnityVersion, Is.EqualTo(Application.unityVersion));
        }
    }
}
