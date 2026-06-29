using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VfxSandbox.Editor
{
    public class VfxWaterDebug
    {
        [MenuItem("Window/VFX/Debug Water Scene")]
        public static void DebugScene()
        {
            Debug.Log("=== VFX WATER SCENE DEBUG ===");
            Scene activeScene = SceneManager.GetActiveScene();
            Debug.Log($"Active Scene: {activeScene.name} (path: {activeScene.path})");

            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            if (rootObjects.Length == 0)
            {
                Debug.LogWarning("No root game objects found in the active scene! Did you open the VfxWaterDemoScene?");
                return;
            }

            foreach (var go in rootObjects)
            {
                DebugObjectHierarchy(go, "");
            }
            Debug.Log("=============================");
        }

        private static void DebugObjectHierarchy(GameObject go, string indent)
        {
            var renderer = go.GetComponent<Renderer>();
            string matInfo = "";
            if (renderer != null)
            {
                if (renderer.sharedMaterial != null)
                {
                    var mat = renderer.sharedMaterial;
                    string shaderName = mat.shader != null ? mat.shader.name : "NULL";
                    matInfo = $" [Material: {mat.name}, Shader: {shaderName}, Enabled: {renderer.enabled}]";
                }
                else
                {
                    matInfo = " [MeshRenderer present, but sharedMaterial is NULL!]";
                }
            }

            Debug.Log($"{indent}• {go.name} (Active: {go.activeSelf}){matInfo}");

            for (int i = 0; i < go.transform.childCount; i++)
            {
                DebugObjectHierarchy(go.transform.GetChild(i).gameObject, indent + "  ");
            }
        }
    }
}
