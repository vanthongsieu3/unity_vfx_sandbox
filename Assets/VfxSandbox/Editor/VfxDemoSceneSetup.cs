using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VfxSandbox.Editor
{
    public class VfxDemoSceneSetup : EditorWindow
    {
        [MenuItem("Window/VFX/Setup Demo Scene")]
        public static void Setup()
        {
            // 1. Tạo các thư mục cần thiết và cấu hình URP
            string settingsDir = "Assets/VfxSandbox/Settings";
            string matDir = "Assets/VfxSandbox/Materials";
            string sceneDir = "Assets/VfxSandbox/Scenes";
            if (!Directory.Exists(settingsDir)) Directory.CreateDirectory(settingsDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);

            // Cấu hình URP Asset tự động để sửa lỗi vật liệu màu hồng (pink shader error)
            string rendererPath = settingsDir + "/CustomRendererData.asset";
            string urpAssetPath = settingsDir + "/CustomURPAsset.asset";
            
            var urpAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(urpAssetPath);
            if (urpAsset == null)
            {
                var rendererData = ScriptableObject.CreateInstance<UnityEngine.Rendering.Universal.UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, rendererPath);

                urpAsset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.Create(rendererData);
                urpAsset.supportsCameraOpaqueTexture = true; // Kích hoạt Opaque Texture để cho phép bẻ cong màn hình
                AssetDatabase.CreateAsset(urpAsset, urpAssetPath);
                
                EditorUtility.SetDirty(urpAsset);
                AssetDatabase.SaveAssets();
            }
            else if (!urpAsset.supportsCameraOpaqueTexture)
            {
                urpAsset.supportsCameraOpaqueTexture = true;
                EditorUtility.SetDirty(urpAsset);
                AssetDatabase.SaveAssets();
            }

            // Gán URP Asset làm render pipeline mặc định cho dự án
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = urpAsset;
            UnityEngine.QualitySettings.renderPipeline = urpAsset;

            AssetDatabase.Refresh();

            // Cấu hình Texture Importer để tạo Alpha Channel từ kênh màu xám (Grayscale)
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_circle_01.png", true);
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_ground_01.png", true);
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png", false);
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png", false); // SỬA: Giữ nguyên Alpha gốc của tệp PNG
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png", false);
            ConfigureTextureImporter("Assets/VfxSandbox/Textures/vfx_tex_rock_01.png", false);

            AssetDatabase.Refresh();

            // Nạp các Texture và Mesh đã tạo
            Texture2D circleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_circle_01.png");
            Texture2D cracksTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ground_01.png");
            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png");
            Texture2D emberTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png");
            Texture2D rampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png");
            Texture2D rockTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_rock_01.png");

            Mesh meteorMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_rock_01.asset");
            Mesh funnelMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_funnel_01.asset");
            Mesh coneMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_cone_01.asset");

            // 2. Tạo các Vật liệu (Materials)
            // A. Lava Flow Material
            string lavaMatPath = matDir + "/mat_lava_flow.mat";
            Material lavaMat = new Material(Shader.Find("VFX/LavaFlow"));
            if (rockTex != null) lavaMat.SetTexture("_BaseMap", rockTex);
            if (noiseTex != null) lavaMat.SetTexture("_NoiseMap", noiseTex);
            if (rampTex != null) lavaMat.SetTexture("_RampMap", rampTex);
            lavaMat.SetFloat("_LavaIntensity", 3f);
            lavaMat.SetFloat("_DisplacementStrength", 0.12f);
            AssetDatabase.CreateAsset(lavaMat, lavaMatPath);

            // B. Screen Distortion Material
            string distMatPath = matDir + "/mat_screen_distortion.mat";
            Material distMat = new Material(Shader.Find("VFX/ScreenDistortion"));
            if (circleTex != null) distMat.SetTexture("_DistortionMap", circleTex);
            distMat.SetFloat("_DistortionStrength", 0.05f);
            AssetDatabase.CreateAsset(distMat, distMatPath);

            // B2. Magic Circle Material (Transparent Unlit)
            string circleMatPath = matDir + "/mat_magic_circle.mat";
            Material circleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            circleMat.SetFloat("_Surface", 1.0f); // Transparent
            circleMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            circleMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            circleMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            circleMat.SetInt("_ZWrite", 0);
            circleMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (circleTex != null)
            {
                circleMat.SetTexture("_BaseMap", circleTex);
                circleMat.SetColor("_BaseColor", new Color(1f, 0.4f, 0f, 1f));
            }
            AssetDatabase.CreateAsset(circleMat, circleMatPath);

            // C. Ground Cracks Material (Transparent Decal shader hoặc Standard)
            string cracksMatPath = matDir + "/mat_ground_cracks.mat";
            Material cracksMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            cracksMat.SetFloat("_Surface", 1.0f); // Set to transparent
            cracksMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); // Bật URP Transparency
            cracksMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            cracksMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            cracksMat.SetInt("_ZWrite", 0);
            cracksMat.DisableKeyword("_ALPHATEST_ON");
            cracksMat.EnableKeyword("_ALPHABLEND_ON");
            cracksMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (cracksTex != null)
            {
                cracksMat.SetTexture("_BaseMap", cracksTex);
                cracksMat.SetColor("_BaseColor", new Color(1f, 0.4f, 0f, 1f));
            }
            AssetDatabase.CreateAsset(cracksMat, cracksMatPath);

            // D. Particle Flame Material
            string flameMatPath = matDir + "/mat_explosion_flame.mat";
            Material flameMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            flameMat.SetFloat("_Surface", 1.0f); // Transparent
            flameMat.SetFloat("_Blend", 1.0f);   // 1.0 is Additive (URP property name is _Blend)
            flameMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); // Bật URP Transparency
            flameMat.EnableKeyword("_BLENDMODE_ADDITIVE");      // Bật URP Additive blending
            flameMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            flameMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            flameMat.SetInt("_ZWrite", 0);
            flameMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (emberTex != null) flameMat.SetTexture("_BaseMap", emberTex);
            AssetDatabase.CreateAsset(flameMat, flameMatPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ─── TỰ ĐỘNG TẠO PREFABS HỆ THỐNG HẠT ───
            VfxPrefabCreator.GeneratePrefabs();
            
            // Nạp các Prefabs đã sinh
            GameObject trailPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_trail.prefab");
            GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_explosion.prefab");
            GameObject debrisPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_debris.prefab");
            GameObject shockwavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_shockwave.prefab");
            GameObject embersPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_embers.prefab");

            // 3. Tạo Scene mới
            Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            demoScene.name = "VfxDemoScene";

            // Thêm Mặt Đất (Ground Plane)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3, 1, 3);
            var groundRenderer = ground.GetComponent<Renderer>();
            Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.12f, 0.15f, 0.18f); // Đá xám tối
            groundRenderer.sharedMaterial = groundMat;

            // Thêm Camera
            GameObject camObj = new GameObject("Main Camera");
            var camera = camObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camObj.transform.position = new Vector3(0, 10, -14);
            camObj.transform.rotation = Quaternion.Euler(35, 0, 0);

            // Thêm Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.5f;
            light.color = new Color(0.68f, 0.78f, 0.9f); // Ánh trăng xanh nhẹ
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Thêm Vfx Manager
            GameObject vfxMgr = new GameObject("Meteor_Vfx_Manager");
            vfxMgr.transform.position = Vector3.zero;
            var controller = vfxMgr.AddComponent<MeteorVfxController>();
            
            // Liên kết tài nguyên đã tạo
            controller.meteorMaterial = lavaMat;
            controller.trailSmokeMaterial = flameMat;
            controller.sparkMaterial = flameMat;
            controller.explosionMaterial = flameMat;
            controller.shockwaveMaterial = distMat;
            controller.groundCracksMaterial = cracksMat;
            controller.emberMaterial = flameMat;
            controller.magicCircleMaterial = circleMat; // Liên kết vật liệu vòng tròn ma pháp mới

            controller.meteorMesh = meteorMesh;
            controller.debrisMesh = meteorMesh;
            controller.funnelMesh = funnelMesh;

            // Gán các Prefabs cho Controller
            controller.trailPrefab = trailPrefab;
            controller.explosionPrefab = explosionPrefab;
            controller.debrisPrefab = debrisPrefab;
            controller.shockwavePrefab = shockwavePrefab;
            controller.embersPrefab = embersPrefab;

            // Lưu Scene
            string scenePath = sceneDir + "/VfxDemoScene.unity";
            EditorSceneManager.SaveScene(demoScene, scenePath);

            AssetDatabase.Refresh();
            Debug.Log($"✓ Setup Demo Scene completed: {scenePath}");
            EditorUtility.DisplayDialog("VFX Setup", "Đã khởi tạo xong dự án VFX!\n\nHãy nhấn vào menu:\n1. Window ▸ VFX ▸ Generate Procedural Textures\n2. Window ▸ VFX ▸ Generate Procedural Meshes\n3. Mở scene VfxDemoScene và nhấn Play, ấn phím SPACE để xem thiên thạch rơi!", "OK");
        }

        private static void ConfigureTextureImporter(string path, bool alphaFromGrayscale)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (alphaFromGrayscale)
                {
                    importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                }
                else
                {
                    importer.alphaSource = TextureImporterAlphaSource.FromInput; // Đọc Alpha từ tệp ảnh gốc
                }
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
    }
}
