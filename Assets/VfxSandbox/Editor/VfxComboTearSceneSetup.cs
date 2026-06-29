using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VfxSandbox.Editor
{
    public class VfxComboTearSceneSetup : EditorWindow
    {
        [MenuItem("Window/VFX/Setup Ground-Tearing Combo Scene")]
        public static void Setup()
        {
            // 1. Tạo các thư mục cần thiết
            string sceneDir = "Assets/VfxSandbox/Scenes";
            string matDir = "Assets/VfxSandbox/Materials";
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            // 2. Chạy sinh tất cả Mesh, Texture, và Prefabs phục vụ combo rẽ đất
            ProceduralTextureGenerator.Generate();
            ProceduralMeshGenerator.Generate();
            SlashPrefabCreator.GeneratePrefabs();
            TearPrefabCreator.GeneratePrefabs();

            // 3. Khởi tạo Scene mới trống hoàn toàn
            Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            demoScene.name = "VfxComboTearDemoScene";

            // 4. Tạo mặt nền phẳng rộng tối màu (Dark Floor) để vệt chém rẽ đất nổi bật
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Dark_Ground_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(8f, 1f, 8f); // Sàn rất rộng để kiếm khí phóng thoải mái
            
            var floorRenderer = floor.GetComponent<Renderer>();
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>(matDir + "/mat_slash_floor.mat");
            if (floorMat != null)
            {
                floorRenderer.sharedMaterial = floorMat;
            }

            // Thêm lớp Collider tĩnh để Raycast đâm trúng
            if (floor.GetComponent<Collider>() == null)
            {
                floor.AddComponent<MeshCollider>();
            }

            // 5. Cấu hình Camera chính nhìn từ trên cao xuống
            GameObject camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 4.5f, -9f);
            camObj.transform.rotation = Quaternion.Euler(24f, 0f, 0f);

            // Kích hoạt Opaque Texture cho Camera để URP Refraction (Spatial Tear) chạy đúng
            var cameraData = camObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.requiresColorTexture = true;

            // 6. Cấu hình ánh sáng Directional dịu nhẹ
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(0.45f, 0.45f, 0.5f, 1f);
            light.intensity = 0.8f;

            // 7. Tạo đối tượng điều khiển chém (Combo Tear Controller)
            GameObject controllerObj = new GameObject("VFX_Combo_Tear_Controller");
            controllerObj.transform.position = new Vector3(0f, 0.2f, 0f);
            var controller = controllerObj.AddComponent<ComboTearVfxController>();

            // Liên kết các Prefabs cung chém và Kiếm khí rẽ đất khổng lồ
            string slashPrefabPath = "Assets/VfxSandbox/Prefabs/vfx_prefab_slash.prefab";
            string giantWavePrefabPath = "Assets/VfxSandbox/Prefabs/vfx_prefab_giant_tear_wave.prefab";

            controller.slashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slashPrefabPath);
            controller.giantTearPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(giantWavePrefabPath);

            // Thêm mô hình cán kiếm đại diện cho vị trí người chơi vung kiếm
            GameObject bladeGizmo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(bladeGizmo.GetComponent<Collider>());
            bladeGizmo.name = "Player_Blade_Gizmo";
            bladeGizmo.transform.parent = controllerObj.transform;
            bladeGizmo.transform.localPosition = new Vector3(0f, 0f, -0.4f);
            bladeGizmo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bladeGizmo.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
            if (floorMat != null)
            {
                bladeGizmo.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 8. Lưu Scene vào thư mục Scenes
            string scenePath = sceneDir + "/VfxComboTearDemoScene.unity";
            EditorSceneManager.SaveScene(demoScene, scenePath);

            AssetDatabase.Refresh();
            Debug.Log($"✓ Setup Combo Tear Demo Scene completed: {scenePath}");
            EditorUtility.DisplayDialog("VFX Combo Tear Setup", "Đã khởi tạo xong Scene vung chém Combo phóng Kiếm Khí Rẽ Đất khổng lồ!\n\n1. Mở Scene mới tại: Assets/VfxSandbox/Scenes/VfxComboTearDemoScene.unity\n2. Nhấn Play và ấn phím SPACE để vung kiếm chém combo rung màn hình, xé toạc đất cát và bắn vụ nổ hoành tráng!", "OK");
        }
    }
}
