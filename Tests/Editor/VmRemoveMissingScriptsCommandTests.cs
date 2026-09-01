using NUnit.Framework;
using Unity.Pipeline.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmRemoveMissingScriptsCommandTests
    {
        [Test]
        public void Execute_RemovesMissingSlotAndMarksSceneDirty()
        {
            var previousActiveScene = SceneManager.GetActiveScene();
            var testScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);
            var objectName = "__VmMissingScript_" +
                             System.Guid.NewGuid().ToString("N");

            try
            {
                SceneManager.SetActiveScene(testScene);
                var gameObject = new GameObject(objectName);
                var sentinel = gameObject.AddComponent<VmMissingScriptSentinel>();
                var serializedSentinel = new SerializedObject(sentinel);
                var scriptProperty = serializedSentinel.FindProperty("m_Script");

                Assert.That(scriptProperty, Is.Not.Null);
                Assert.That(scriptProperty.editable, Is.True);
                scriptProperty.objectReferenceValue = null;
                serializedSentinel.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        gameObject),
                    Is.EqualTo(1));

                var target = new ObjectRef
                {
                    HierarchyPath = "/" + objectName
                };
                var result = VmRemoveMissingScriptsCommand.Execute(target);

                Assert.That(result.Ok, Is.True);
                Assert.That(result.RemovedCount, Is.EqualTo(1));
                Assert.That(result.HierarchyPath, Is.EqualTo("/" + objectName));
                Assert.That(result.SceneDirty, Is.True);
                Assert.That(
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        gameObject),
                    Is.Zero);

                var idempotentResult =
                    VmRemoveMissingScriptsCommand.Execute(target);
                Assert.That(idempotentResult.Ok, Is.True);
                Assert.That(idempotentResult.RemovedCount, Is.Zero);
            }
            finally
            {
                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }

                EditorSceneManager.CloseScene(testScene, true);
                Undo.ClearAll();
            }
        }

        [Test]
        public void Execute_UnknownTargetReturnsStableDomainError()
        {
            var result = VmRemoveMissingScriptsCommand.Execute(
                new ObjectRef
                {
                    HierarchyPath = "/__VmMissingScriptTargetDoesNotExist"
                });

            Assert.That(result.Ok, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("target_not_found"));
            Assert.That(result.RemovedCount, Is.Zero);
        }
    }
}
