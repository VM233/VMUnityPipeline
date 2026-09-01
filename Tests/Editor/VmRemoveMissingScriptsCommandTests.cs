using NUnit.Framework;
using Unity.Pipeline;
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
            var testScene = EditorSceneManager.NewPreviewScene();
            var identity = System.Guid.NewGuid().ToString("N");
            var objectName = "__VmMissingScript_" + identity;
            GameObject gameObject = null;

            try
            {
                gameObject = new GameObject(objectName);
                SceneManager.MoveGameObjectToScene(gameObject, testScene);
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
                    InstanceId = PipelineUtils.GetObjectId(gameObject)
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
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }

                if (testScene.IsValid() && testScene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(testScene);
                }

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
