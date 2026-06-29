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

            // Tự động sinh/cập nhật toàn bộ Textures (Noise, Normal, Caustics) trước khi nạp
            ProceduralTextureGenerator.Generate();

            // Kích hoạt Depth Texture và Opaque Texture trên URP Asset để nước hiển thị bọt viền dốc và khúc xạ đáy cát
            var pipelineAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipelineAsset == null)
            {
                pipelineAsset = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            }
            if (pipelineAsset != null)
            {
                pipelineAsset.supportsCameraDepthTexture = true;
                pipelineAsset.supportsCameraOpaqueTexture = true;
                EditorUtility.SetDirty(pipelineAsset);
            }

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
            Texture2D causticsTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_water_caustics.png");
            if (noiseTex != null)
            {
                waterMat.SetTexture("_NoiseMap", noiseTex);
            }
            if (normalTex != null)
            {
                waterMat.SetTexture("_NormalMap", normalTex);
            }
            if (causticsTex != null)
            {
                waterMat.SetTexture("_CausticsMap", causticsTex);
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
            waterMat.SetFloat("_CausticsPower", 6.5f); // Tăng lũy thừa tạo gợn sóng nắng siêu long lanh sắc sảo
            waterMat.SetFloat("_CausticsIntensity", 2.2f);
            waterMat.SetColor("_CausticsColor", new Color(0.65f, 1.0f, 0.92f, 1.0f));
            waterMat.SetColor("_SkyColor", new Color(0.45f, 0.68f, 0.9f, 1.0f));
            waterMat.SetFloat("_ReflectionStrength", 0.75f);
            waterMat.SetFloat("_Glossiness", 200.0f);
            waterMat.SetFloat("_SpecularIntensity", 3.5f);
            
            // Đồng bộ tọa độ các cột đá vào shader để vẽ sóng phản xạ vòng tròn hướng tâm
            waterMat.SetVector("_Pillar1Pos", new Vector4(1.2f, 1.5f, 0f, 0f));
            waterMat.SetVector("_Pillar2Pos", new Vector4(-1.8f, 3.2f, 0f, 0f));
            
            // Thiết lập sóng phản chấn lan rộng nhiều lớp dập dềnh mờ dần
            waterMat.SetFloat("_RippleHeight", 0.08f);
            waterMat.SetFloat("_RippleScale", 6.0f);
            waterMat.SetFloat("_RippleSpeed", 4.5f);
            waterMat.SetFloat("_RippleDecay", 0.32f); // Decay thấp để lan tỏa ra xa nhiều lớp nhỏ tới to mờ dần

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

            // H. Thêm script phản chiếu phẳng thời gian thực (PlanarReflections)
            var planarRef = waterPlane.AddComponent<PlanarReflections>();
            planarRef.textureSize = 512; // 512x512 render texture cho độ nét và hiệu năng tốt nhất
            planarRef.clipPlaneOffset = 0.02f;
            planarRef.reflectionMask = ~0; // Phản chiếu toàn bộ các vật thể trong scene (loại trừ nước qua code)

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
            hullBase.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            hullBase.transform.localScale = new Vector3(0.9f, 0.6f, 2.2f);
            hullBase.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullBase.GetComponent<Collider>());

            // B. Mạn trái thuyền (Hull Left)
            GameObject hullLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullLeft.name = "Hull_Left";
            hullLeft.transform.SetParent(boatRoot.transform);
            hullLeft.transform.localPosition = new Vector3(-0.52f, 0.35f, 0f);
            hullLeft.transform.localScale = new Vector3(0.08f, 0.9f, 2.2f);
            hullLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 12f); // Nghiêng vát ra ngoài
            hullLeft.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullLeft.GetComponent<Collider>());

            // C. Mạn phải thuyền (Hull Right)
            GameObject hullRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullRight.name = "Hull_Right";
            hullRight.transform.SetParent(boatRoot.transform);
            hullRight.transform.localPosition = new Vector3(0.52f, 0.35f, 0f);
            hullRight.transform.localScale = new Vector3(0.08f, 0.9f, 2.2f);
            hullRight.transform.localRotation = Quaternion.Euler(0f, 0f, -12f); // Nghiêng vát ra ngoài
            hullRight.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullRight.GetComponent<Collider>());

            // D. Mũi thuyền nhọn (Hull Bow)
            GameObject hullBow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullBow.name = "Hull_Bow";
            hullBow.transform.SetParent(boatRoot.transform);
            hullBow.transform.localPosition = new Vector3(0f, 0.35f, 1.25f);
            hullBow.transform.localScale = new Vector3(0.88f, 0.9f, 0.4f);
            hullBow.transform.localRotation = Quaternion.Euler(20f, 0f, 0f); // Vát nghiêng lên mũi
            hullBow.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(hullBow.GetComponent<Collider>());

            // E. Cột buồm (Mast)
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Mast";
            mast.transform.SetParent(boatRoot.transform);
            mast.transform.localPosition = new Vector3(0f, 1.3f, 0.1f);
            mast.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
            mast.GetComponent<Renderer>().sharedMaterial = woodMat;
            DestroyImmediate(mast.GetComponent<Collider>());

            // F. Cánh buồm (Sail)
            GameObject sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sail.name = "Sail";
            sail.transform.SetParent(boatRoot.transform);
            sail.transform.localPosition = new Vector3(0f, 1.7f, 0.35f);
            sail.transform.localScale = new Vector3(1.1f, 1.3f, 0.02f);
            sail.transform.localRotation = Quaternion.Euler(5f, -10f, 3f); // Hơi xoắn căng gió
            sail.GetComponent<Renderer>().sharedMaterial = sailMat;
            DestroyImmediate(sail.GetComponent<Collider>());

            // G. Thêm script điều khiển dập dềnh (BoatFloating)
            var floatingScript = boatRoot.AddComponent<BoatFloating>();
            floatingScript.waterRenderer = waterRenderer; // Liên kết trực tiếp Renderer mặt nước
            floatingScript.floatOffset = 0.18f; // Boong thuyền nổi hẳn trên mặt nước
            floatingScript.positionLerpSpeed = 12.0f;
            floatingScript.rotationLerpSpeed = 10.0f;
            floatingScript.waveHeight = 0.22f;
            floatingScript.waveScale = 0.85f;
            floatingScript.waveSpeed = 1.6f;
            floatingScript.waveDirection = new Vector2(0f, -1f);
            floatingScript.pillar1Pos = new Vector2(1.2f, 1.5f);
            floatingScript.pillar2Pos = new Vector2(-1.8f, 3.2f);
            floatingScript.rippleHeight = 0.08f;
            floatingScript.rippleScale = 6.0f;
            floatingScript.rippleSpeed = 4.5f;
            floatingScript.rippleDecay = 0.32f; // Đồng pha 32% suy hao để lan tỏa xa nhiều lớp

            // 8. Tạo hệ thống hạt bọt khí dưới nước cách điệu (Stylized Bubble Particle Systems)
            string bubbleMatPath = matDir + "/mat_particle_bubble.mat";
            Material bubbleMat = AssetDatabase.LoadAssetAtPath<Material>(bubbleMatPath);
            if (bubbleMat == null)
            {
                bubbleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(bubbleMat, bubbleMatPath);
            }
            // Thiết lập chế độ Transparent (trong suốt Alpha Blend) cho hạt để tránh viền đen/xám vuông
            bubbleMat.SetFloat("_Surface", 1.0f); // 1 = Transparent
            bubbleMat.SetFloat("_Blend", 0.0f);   // 0 = Alpha Blend
            bubbleMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bubbleMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bubbleMat.SetFloat("_ZWrite", 0.0f);  // Không ghi vào Z-Buffer tránh đè cắt hạt
            bubbleMat.DisableKeyword("_ALPHATEST_ON");
            bubbleMat.EnableKeyword("_ALPHABLEND_ON");
            bubbleMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            Texture2D bubbleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_bubble_01.png");
            if (bubbleTex != null)
            {
                bubbleMat.SetTexture("_BaseMap", bubbleTex);
            }

            // A. Bọt khí nền trôi nổi tự do (Ambient Ocean Bubbles) - sinh sát mặt nước, sủi tăm rất nhỏ rồi tan
            AddBubbleParticleSystem(waterPlane, "Ambient_Ocean_Bubbles", new Vector3(0f, -0.05f, 1.5f), new Vector3(12f, 0.1f, 12f), 30f, 0.02f, 0.08f, 0.01f, 0.05f, 0.8f, 1.6f, -0.01f, bubbleMat);

            // B. Bọt khí sục sôi sủi bọt quanh cọc đá 1 (Rock 1 Churning Bubbles) - sủi bọt nhỏ, nhanh tan ở chân cọc
            AddBubbleParticleSystem(rock1, "Rock1_Churn_Bubbles", new Vector3(0f, -0.05f, 0f), new Vector3(1.2f, 0.1f, 1.2f), 12f, 0.015f, 0.06f, 0.02f, 0.08f, 0.6f, 1.3f, -0.02f, bubbleMat);

            // C. Bọt khí sục sôi sủi bọt quanh cọc đá 2 (Rock 2 Churning Bubbles) - sủi bọt nhỏ, nhanh tan ở chân cọc
            AddBubbleParticleSystem(rock2, "Rock2_Churn_Bubbles", new Vector3(0f, -0.05f, 0f), new Vector3(1.2f, 0.1f, 1.2f), 12f, 0.015f, 0.06f, 0.02f, 0.08f, 0.6f, 1.3f, -0.02f, bubbleMat);

            // D. Bọt khí rẽ sóng dưới đáy thuyền (Boat Churning Bubbles) - sủi bọt và tan ngay dưới lườn thuyền
            AddBubbleParticleSystem(boatRoot, "Boat_Churn_Bubbles", new Vector3(0f, -0.05f, -0.2f), new Vector3(0.8f, 0.1f, 1.8f), 16f, 0.015f, 0.07f, 0.02f, 0.08f, 0.6f, 1.3f, -0.02f, bubbleMat);

            // 9. Lưu Scene
            string scenePath = sceneDir + "/VfxWaterDemoScene.unity";
            EditorSceneManager.SaveScene(waterScene, scenePath);

            AssetDatabase.Refresh();
            Debug.Log($"✓ Setup Stylized Water Scene completed successfully: {scenePath}");
            EditorUtility.DisplayDialog("VFX Water Setup", "Đã khởi tạo xong Scene Nước cách điệu Stylized Water!\n\n1. Mở Scene mới tại: Assets/VfxSandbox/Scenes/VfxWaterDemoScene.unity\n2. Nhấn Play để chiêm ngưỡng sóng Gerstner, thấu quang ngọc lục bảo, thủy triều bờ cát, bọt viền ôm vật thể, gợn nắng caustics long lanh và hệ thống hạt bọt khí sủi bọt sinh động xung quanh cọc và thuyền!", "OK");
        }

        private static void AddBubbleParticleSystem(GameObject parent, string name, Vector3 localPos, Vector3 localScale, float emissionRate, float startSizeMin, float startSizeMax, float speedMin, float speedMax, float lifeMin, float lifeMax, float gravity, Material mat)
        {
            GameObject bubbleObj = new GameObject(name);
            bubbleObj.transform.SetParent(parent.transform);
            bubbleObj.transform.localPosition = localPos;
            bubbleObj.transform.localScale = Vector3.one;

            ParticleSystem ps = bubbleObj.AddComponent<ParticleSystem>();
            
            // Main Module
            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startSize = new ParticleSystem.MinMaxCurve(startSizeMin, startSizeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // Trôi nổi tự do trong không gian thế giới
            main.playOnAwake = true;

            // Color over Lifetime (Fade in & Fade out mượt mà)
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.75f, 0.15f), new GradientAlphaKey(0.75f, 0.75f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            col.color = new ParticleSystem.MinMaxGradient(grad);

            // Size over Lifetime (Bọt nở phình nhẹ khi nổi gần mặt nước)
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            Keyframe[] keys = new Keyframe[] { new Keyframe(0f, 0.5f), new Keyframe(0.2f, 1.0f), new Keyframe(1.0f, 1.15f) };
            sz.size = new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(keys));

            // Emission
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = emissionRate;

            // Shape
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = localScale;

            // Renderer
            var psr = bubbleObj.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = mat;
            psr.renderMode = ParticleSystemRenderMode.Billboard;
        }
    }
}
