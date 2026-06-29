using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VfxSandbox.Editor
{
    public class VfxSlashDemoSceneSetup : EditorWindow
    {
        [MenuItem("Window/VFX/Setup Slash Demo Scene")]
        public static void Setup()
        {
            // 1. Tạo các thư mục cần thiết
            string sceneDir = "Assets/VfxSandbox/Scenes";
            string matDir = "Assets/VfxSandbox/Materials";
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            // 2. Chạy sinh Mesh, Texture và Prefab cho Slash
            ProceduralTextureGenerator.Generate();
            ProceduralMeshGenerator.Generate();
            SlashPrefabCreator.GeneratePrefabs();

            // 3. Khởi tạo Scene mới trống hoàn toàn
            Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            demoScene.name = "VfxSlashDemoScene";

            // 4. Tạo mặt nền tối (Dark Floor) để màu chém xanh lam phát sáng nổi bật
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Dark_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            
            var floorRenderer = floor.GetComponent<Renderer>();
            string floorMatPath = matDir + "/mat_slash_floor.mat";
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>(floorMatPath);
            if (floorMat == null)
            {
                floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(floorMat, floorMatPath);
            }
            floorMat.SetColor("_BaseColor", new Color(0.12f, 0.12f, 0.15f, 1f)); // Nền xám đậm hơi xanh
            floorRenderer.sharedMaterial = floorMat;

            // 5. Cấu hình Camera chính nhìn nghiêng từ trên xuống
            GameObject camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 3.5f, -6.5f);
            camObj.transform.rotation = Quaternion.Euler(24f, 0f, 0f);

            // Kích hoạt Opaque Texture cho Camera để các shader distortion/glass nếu có chạy đúng
            var cameraData = camObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.requiresColorTexture = true;

            // 6. Cấu hình ánh sáng môi trường dịu nhẹ (Dim Light)
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(0.45f, 0.45f, 0.5f, 1f);
            light.intensity = 0.8f;

            // 7. Tạo đối tượng điều khiển chém (Slash Controller Manager)
            GameObject controllerObj = new GameObject("VFX_Slash_Controller");
            controllerObj.transform.position = new Vector3(0f, 0.2f, 0f); // Cao hơn mặt đất một chút
            var controller = controllerObj.AddComponent<SlashVfxController>();

            // Liên kết Prefab cung chém đã tạo vào Controller
            string prefabPath = "Assets/VfxSandbox/Prefabs/vfx_prefab_slash.prefab";
            GameObject slashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            controller.slashPrefab = slashPrefab;

            // Tạo thêm mô hình kiếm đơn giản đại diện cho vị trí người chơi vung kiếm
            GameObject bladeGizmo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(bladeGizmo.GetComponent<Collider>());
            bladeGizmo.name = "Player_Blade_Gizmo";
            bladeGizmo.transform.parent = controllerObj.transform;
            bladeGizmo.transform.localPosition = new Vector3(0f, 0f, -0.4f);
            bladeGizmo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bladeGizmo.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
            
            var bladeRenderer = bladeGizmo.GetComponent<Renderer>();
            bladeRenderer.sharedMaterial = floorMat; // Dùng chung vật liệu xám tối

            // 8. Lưu Scene
            string scenePath = sceneDir + "/VfxSlashDemoScene.unity";
            EditorSceneManager.SaveScene(demoScene, scenePath);

            AssetDatabase.Refresh();
            Debug.Log($"✓ Setup Slash Demo Scene completed successfully: {scenePath}");
            EditorUtility.DisplayDialog("VFX Slash Setup", "Đã khởi tạo xong Scene vệt chém ma thuật (Magic Slash)!\n\n1. Mở Scene mới tại: Assets/VfxSandbox/Scenes/VfxSlashDemoScene.unity\n2. Nhấn Play và ấn phím SPACE để vung kiếm chém combo ma thuật cực đẹp!", "OK");
        }
    }
}
