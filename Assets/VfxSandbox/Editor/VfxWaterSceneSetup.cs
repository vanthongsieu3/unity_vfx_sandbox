using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VfxSandbox.Editor
{
    public class VfxWaterSceneSetup : EditorWindow
    {
        [MenuItem("Window/VFX/Setup Stylized Water Scene")]
        public static void Setup()
        {
            string sceneDir = "Assets/VfxSandbox/Scenes";
            string matDir = "Assets/VfxSandbox/Materials";
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            // 1. Dựng các vật liệu (Materials)
            // A. Vật liệu nước cách điệu (Stylized Water Material)
            string waterMatPath = matDir + "/mat_stylized_water.mat";
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(waterMatPath);
            if (waterMat == null)
            {
                waterMat = new Material(Shader.Find("VFX/StylizedWater"));
                AssetDatabase.CreateAsset(waterMat, waterMatPath);
            }
            
            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png");
            Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_water_normal.png");
            if (noiseTex != null)
            {
                waterMat.SetTexture("_NoiseMap", noiseTex);
            }
            if (normalTex != null)
            {
                waterMat.SetTexture("_NormalMap", normalTex);
            }

            // Gán các thông số màu sắc nước lung linh PC-grade
            waterMat.SetColor("_ShallowColor", new Color(0.0f, 0.82f, 0.78f, 0.5f));  // Xanh lam ngọc trong trẻo nông
            waterMat.SetColor("_DeepColor", new Color(0.01f, 0.12f, 0.38f, 0.98f));   // Xanh biển đại dương sâu thẳm
            waterMat.SetFloat("_WaterOpacity", 0.45f);
            waterMat.SetFloat("_DepthMaxDistance", 3.2f);
            waterMat.SetFloat("_FoamDistance", 0.55f);
            waterMat.SetFloat("_FoamNoiseScale", 4.0f);
            waterMat.SetFloat("_FoamNoiseWeight", 0.45f); // Độ lồi lõm của viền bọt
            waterMat.SetFloat("_WaveCrestThreshold", 0.08f); // Bọt đỉnh sóng nổi lên ở đỉnh
            waterMat.SetFloat("_WaveCrestRange", 0.18f);
            
            // Thiết lập thấu quang ngọn sóng (SSS) và thủy triều xô bờ (Foam Lapping)
            waterMat.SetColor("_SssColor", new Color(0.0f, 1.0f, 0.65f, 1.0f)); // Xanh ngọc lục bảo phát sáng
            waterMat.SetFloat("_SssStrength", 1.5f);
            waterMat.SetFloat("_SssPower", 4.0f);
            waterMat.SetFloat("_FoamLappingSpeed", 1.3f);
            waterMat.SetFloat("_FoamLappingAmplitude", 0.16f);

            waterMat.SetVector("_WaveDirection", new Vector4(0f, -1f, 0f, 0f)); // Sóng đánh từ xa (sau) về gần bờ cát (trước)
            waterMat.SetFloat("_WaveHeight", 0.22f); // Sóng nhấp nhô tuyệt đẹp
            waterMat.SetFloat("_WaveScale", 0.85f);
            waterMat.SetFloat("_WaveSpeed", 1.6f);
            waterMat.SetFloat("_CausticsCutoff", 0.3f);
            waterMat.SetFloat("_CausticsIntensity", 2.0f);
            waterMat.SetColor("_CausticsColor", new Color(0.65f, 1.0f, 0.92f, 1.0f));
            waterMat.SetColor("_SkyColor", new Color(0.45f, 0.68f, 0.9f, 1.0f));
            waterMat.SetFloat("_ReflectionStrength", 0.75f);
            waterMat.SetFloat("_Glossiness", 200.0f);
            waterMat.SetFloat("_SpecularIntensity", 3.5f);
            
            // Đồng bộ tọa độ các cột đá vào shader để vẽ sóng phản xạ vòng tròn hướng tâm
            waterMat.SetVector("_Pillar1Pos", new Vector4(1.2f, 1.5f, 0f, 0f));
            waterMat.SetVector("_Pillar2Pos", new Vector4(-1.8f, 3.2f, 0f, 0f));

            // B. Vật liệu cát biển dưới đáy nước (Sand Material)
            string sandMatPath = matDir + "/mat_water_sand.mat";
            Material sandMat = AssetDatabase.LoadAssetAtPath<Material>(sandMatPath);
            if (sandMat == null)
            {
                sandMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(sandMat, sandMatPath);
            }
            sandMat.SetColor("_BaseColor", new Color(0.86f, 0.77f, 0.58f, 1.0f)); // Màu cát vàng ấm áp
            sandMat.SetFloat("_Smoothness", 0.1f); // Cát khô nhám

            // C. Vật liệu đá tảng đen nhám (Dark Rock Material)
            string rockMatPath = matDir + "/mat_water_rock.mat";
            Material rockMat = AssetDatabase.LoadAssetAtPath<Material>(rockMatPath);
            if (rockMat == null)
            {
                rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(rockMat, rockMatPath);
            }
            rockMat.SetColor("_BaseColor", new Color(0.18f, 0.18f, 0.20f, 1.0f)); // Đá xám tối
            rockMat.SetFloat("_Smoothness", 0.15f);

            AssetDatabase.SaveAssets();

            // 2. Khởi tạo Scene mới trống hoàn toàn
            Scene waterScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            waterScene.name = "VfxWaterDemoScene";

            // 3. Tạo mặt nước phẳng (Water Plane)
            GameObject waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "Stylized_Water_Surface";
            waterPlane.transform.position = Vector3.zero;
            waterPlane.transform.localScale = new Vector3(5.5f, 1f, 5.5f); // 55x55 mét diện tích nước
            
            // Xóa Collider của nước để không cản trở tia raycast của các VFX khác nếu có
            DestroyImmediate(waterPlane.GetComponent<Collider>());
            
            var waterRenderer = waterPlane.GetComponent<Renderer>();
            waterRenderer.sharedMaterial = waterMat;

            // 4. Tạo mặt cát dốc xuống dưới đáy nước (Sloping Sand Floor)
            // Mặt cát dốc thoai thoải từ sau ra trước (Phần trước là bờ cát cạn, phần sau là biển sâu)
            GameObject sandPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sandPlane.name = "Sloping_Sand_Floor";
            sandPlane.transform.position = new Vector3(0f, -1.0f, 1.5f);
            sandPlane.transform.rotation = Quaternion.Euler(9f, 0f, 0f); // Dốc nghiêng 9 độ (sát camera nông, xa camera sâu)
            sandPlane.transform.localScale = new Vector3(7.5f, 1f, 7.5f);
            
            var sandRenderer = sandPlane.GetComponent<Renderer>();
            sandRenderer.sharedMaterial = sandMat;

            // 5. Tạo các khối đá tảng nhô lên mặt nước để thể hiện bọt xô viền (Foam Intersection rings)
            GameObject rock1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rock1.name = "Water_Rock_Obstacle_1";
            rock1.transform.position = new Vector3(1.2f, -0.2f, 1.5f);
            rock1.transform.localScale = new Vector3(1.1f, 1.4f, 1.1f);
            rock1.transform.rotation = Quaternion.Euler(15f, 30f, -10f);
            rock1.GetComponent<Renderer>().sharedMaterial = rockMat;

            GameObject rock2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rock2.name = "Water_Rock_Obstacle_2";
            rock2.transform.position = new Vector3(-1.8f, -0.3f, 3.2f);
            rock2.transform.localScale = new Vector3(2.0f, 1.6f, 2.0f);
            rock2.transform.rotation = Quaternion.Euler(-10f, 45f, 15f);
            rock2.GetComponent<Renderer>().sharedMaterial = rockMat;

            // 6. Cấu hình Camera góc xiên nhìn xuống mặt nước
            GameObject camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 3.8f, -7.5f);
            camObj.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

            // BẮT BUỘC: Kích hoạt Opaque Texture và Depth Texture trên Camera để URP vẽ được bọt nước và trong suốt
            var cameraData = camObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.requiresColorTexture = true;
            cameraData.requiresDepthTexture = true; // Rất quan trọng để đo độ sâu tạo bọt biển

            // 7. Cấu hình ánh sáng Mặt Trời ấm áp
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(52f, -34f, 0f);
            light.color = new Color(1.0f, 0.96f, 0.88f, 1f); // Nắng vàng nhạt ấm áp
            light.intensity = 1.3f;
            light.shadows = LightShadows.Soft;

            // 8. Tạo mô hình con thuyền cánh buồm bằng các khối cơ bản (Procedural Stylized Sailboat)
            GameObject boatRoot = new GameObject("Stylized_Sailboat");
            boatRoot.transform.position = new Vector3(-0.5f, 0f, -1.0f); // Đặt ở vị trí trung tâm lệch nhẹ
            boatRoot.transform.rotation = Quaternion.Euler(0f, 45f, 0f); // Xoay chéo góc 45 độ so với chiều sóng

            // Tạo vật liệu gỗ và cánh buồm nhanh trong thư mục Asset để lưu trữ sạch sẽ
            string woodMatPath = matDir + "/mat_boat_wood.mat";
            Material woodMat = AssetDatabase.LoadAssetAtPath<Material>(woodMatPath);
            if (woodMat == null)
            {
                woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(woodMat, woodMatPath);
            }
            woodMat.SetColor("_BaseColor", new Color(0.35f, 0.20f, 0.10f, 1.0f)); // Nâu gỗ ấm
            woodMat.SetFloat("_Smoothness", 0.1f);

            string sailMatPath = matDir + "/mat_boat_sail.mat";
            Material sailMat = AssetDatabase.LoadAssetAtPath<Material>(sailMatPath);
            if (sailMat == null)
            {
                sailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(sailMat, sailMatPath);
            }
            sailMat.SetColor("_BaseColor", new Color(0.92f, 0.90f, 0.85f, 1.0f)); // Trắng ngà
            sailMat.SetFloat("_Smoothness", 0.05f);

            AssetDatabase.SaveAssets();

            // A. Đáy thuyền (Hull Base)
            GameObject hullBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullBase.name = "Hull_Base";
            hullBase.transform.SetParent(boatRoot.transform);
            hullBase.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            hullBase.transform.localScale = new Vector3(0.9f, 0.3f, 2.2f);
            hullBase.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullBase.GetComponent<Collider>());

            // B. Mạn trái thuyền (Hull Left)
            GameObject hullLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullLeft.name = "Hull_Left";
            hullLeft.transform.SetParent(boatRoot.transform);
            hullLeft.transform.localPosition = new Vector3(-0.48f, 0.1f, 0f);
            hullLeft.transform.localScale = new Vector3(0.08f, 0.45f, 2.2f);
            hullLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 12f); // Nghiêng vát ra ngoài
            hullLeft.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullLeft.GetComponent<Collider>());

            // C. Mạn phải thuyền (Hull Right)
            GameObject hullRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullRight.name = "Hull_Right";
            hullRight.transform.SetParent(boatRoot.transform);
            hullRight.transform.localPosition = new Vector3(0.48f, 0.1f, 0f);
            hullRight.transform.localScale = new Vector3(0.08f, 0.45f, 2.2f);
            hullRight.transform.localRotation = Quaternion.Euler(0f, 0f, -12f); // Nghiêng vát ra ngoài
            hullRight.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullRight.GetComponent<Collider>());

            // D. Mũi thuyền nhọn (Hull Bow)
            GameObject hullBow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullBow.name = "Hull_Bow";
            hullBow.transform.SetParent(boatRoot.transform);
            hullBow.transform.localPosition = new Vector3(0f, 0.1f, 1.25f);
            hullBow.transform.localScale = new Vector3(0.88f, 0.45f, 0.4f);
            hullBow.transform.localRotation = Quaternion.Euler(20f, 0f, 0f); // Vát nghiêng lên mũi
            hullBow.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullBow.GetComponent<Collider>());

            // E. Cột buồm (Mast)
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Mast";
            mast.transform.SetParent(boatRoot.transform);
            mast.transform.localPosition = new Vector3(0f, 0.9f, 0.1f);
            mast.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
            mast.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(mast.GetComponent<Collider>());

            // F. Cánh buồm (Sail)
            GameObject sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sail.name = "Sail";
            sail.transform.SetParent(boatRoot.transform);
            sail.transform.localPosition = new Vector3(0f, 1.2f, 0.35f);
            sail.transform.localScale = new Vector3(1.1f, 0.9f, 0.02f);
            sail.transform.localRotation = Quaternion.Euler(5f, -10f, 3f); // Hơi xoắn căng gió
            sail.GetComponent<Renderer>().sharedMaterial = sailMat;
            DestroyImmediate(sail.GetComponent<Collider>());

            // G. Thêm script điều khiển dập dềnh (BoatFloating)
            var floatingScript = boatRoot.AddComponent<BoatFloating>();
            floatingScript.waterRenderer = waterRenderer; // Liên kết trực tiếp Renderer mặt nước
            floatingScript.floatOffset = -0.1f;
            floatingScript.waveHeight = 0.22f;
            floatingScript.waveScale = 0.85f;
            floatingScript.waveSpeed = 1.6f;
            floatingScript.waveDirection = new Vector2(0f, -1f);
            floatingScript.pillar1Pos = new Vector2(1.2f, 1.5f);
            floatingScript.pillar2Pos = new Vector2(-1.8f, 3.2f);

            // 9. Lưu Scene
            string scenePath = sceneDir + "/VfxWaterDemoScene.unity";
            EditorSceneManager.SaveScene(waterScene, scenePath);

            AssetDatabase.Refresh();
            Debug.Log($"✓ Setup Stylized Water Scene completed successfully: {scenePath}");
            EditorUtility.DisplayDialog("VFX Water Setup", "Đã khởi tạo xong Scene Nước cách điệu Stylized Water!\n\n1. Mở Scene mới tại: Assets/VfxSandbox/Scenes/VfxWaterDemoScene.unity\n2. Nhấn Play để chiêm ngưỡng mặt nước dập dềnh sóng Gerstner, màu nước nông lam ngọc loang dần sang xanh sâu thẳm, và các viền bọt sóng trắng xô bờ quấn quanh các tảng đá tảng tự động cực đẹp!", "OK");
        }
    }
}
