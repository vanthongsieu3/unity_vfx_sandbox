using System.IO;
using UnityEditor;
using UnityEngine;

namespace VfxSandbox.Editor
{
    public class TearPrefabCreator
    {
        [MenuItem("Window/VFX/Generate Ground-Tearing Prefabs (Non-Destructive)")]
        public static void GeneratePrefabs()
        {
            string prefabDir = "Assets/VfxSandbox/Prefabs";
            string matDir = "Assets/VfxSandbox/Materials";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            // Load các assets cần thiết
            Texture2D groundTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ground_01.png");
            Texture2D smokeTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png"); // Dùng noise làm khói cuộn
            Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_star_01.png");
            Mesh slashMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_slash_01.asset");
            Mesh rockMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_rock_01.asset");
            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>(matDir + "/mat_magic_slash.mat");
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>(matDir + "/mat_slash_sparks.mat");
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>(matDir + "/mat_slash_floor.mat");
            GameObject terminalExplosion = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_explosion.prefab");

            // 1. Tạo các vật liệu phục vụ rẽ đất
            // A. Vật liệu vết rách đất (Void Ground Crack) - Dùng Alpha Blend để giữ màu charcoal của đá và màu neon xanh lam rực
            string crackMatPath = matDir + "/mat_void_ground_crack.mat";
            Material crackMat = AssetDatabase.LoadAssetAtPath<Material>(crackMatPath);
            if (crackMat == null)
            {
                crackMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(crackMat, crackMatPath);
            }
            crackMat.SetFloat("_Surface", 1.0f); // Transparent
            crackMat.SetFloat("_Blend", 0.0f);   // Alpha blend
            crackMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            crackMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            crackMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            crackMat.SetInt("_ZWrite", 0);
            crackMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (groundTex != null) crackMat.SetTexture("_BaseMap", groundTex);

            // B. Vật liệu khói bụi bốc lên khi rách đất (Tear Smoke Material)
            string smokeMatPath = matDir + "/mat_tear_smoke.mat";
            Material smokeMat = AssetDatabase.LoadAssetAtPath<Material>(smokeMatPath);
            if (smokeMat == null)
            {
                smokeMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(smokeMat, smokeMatPath);
            }
            smokeMat.SetFloat("_Surface", 1.0f); // Transparent
            smokeMat.SetFloat("_Blend", 0.0f);   // Alpha blend
            smokeMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            smokeMat.SetInt("_ZWrite", 0);
            smokeMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (smokeTex != null) smokeMat.SetTexture("_BaseMap", smokeTex);

            AssetDatabase.SaveAssets();

            // 2. Dựng Prefab vết nứt đất rẽ ra (vfx_prefab_ground_crack_decal.prefab)
            string crackPrefabPath = prefabDir + "/vfx_prefab_ground_crack_decal.prefab";
            CreateGroundCrackPrefab(crackPrefabPath, crackMat);

            // 3. Dựng Prefab Bụi đất và Đá văng lên khi rách (vfx_prefab_tear_impact_dust.prefab)
            string dustPrefabPath = prefabDir + "/vfx_prefab_tear_impact_dust.prefab";
            CreateTearDustPrefab(dustPrefabPath, smokeMat, floorMat, rockMesh);

            // 4. Dựng Prefab Kiếm Khí Khổng Lồ Rẽ Đất (vfx_prefab_giant_tear_wave.prefab)
            string wavePrefabPath = prefabDir + "/vfx_prefab_giant_tear_wave.prefab";
            GameObject crackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(crackPrefabPath);
            GameObject dustPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dustPrefabPath);
            CreateGiantTearWavePrefab(wavePrefabPath, slashMat, sparksMat, crackPrefab, dustPrefab, terminalExplosion, slashMesh);

            AssetDatabase.Refresh();
            Debug.Log("✓ Ground-Tearing prefabs generated successfully!");
        }

        private static void CreateGroundCrackPrefab(string path, Material crackMat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "VFX_Ground_Crack_Tear";
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = crackMat;

            // Đính kèm bộ xử lý tự mở rộng và nguội lạnh
            go.AddComponent<VfxGroundCrackTear>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateTearDustPrefab(string path, Material smokeMat, Material rockMat, Mesh rockMesh)
        {
            GameObject go = new GameObject("VFX_Tear_Impact_Dust");

            // A. Khói cuộn bốc lên (Billowy Smoke)
            GameObject smokeGo = new GameObject("Impact_Smoke");
            smokeGo.transform.parent = go.transform;
            smokeGo.transform.localPosition = Vector3.zero;
            smokeGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Phun thẳng đứng lên trên

            var smokePs = smokeGo.AddComponent<ParticleSystem>();
            var smokeMain = smokePs.main;
            smokeMain.duration = 1.0f;
            smokeMain.loop = false;
            smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            smokeMain.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
            smokeMain.gravityModifier = -0.08f; // Bay bốc nhẹ lên
            smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var smokeEmission = smokePs.emission;
            smokeEmission.rateOverTime = 0f;
            smokeEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 10) });

            var smokeShape = smokePs.shape;
            smokeShape.shapeType = ParticleSystemShapeType.Cone;
            smokeShape.angle = 25f;
            smokeShape.radius = 0.2f;

            var smokeSize = smokePs.sizeOverLifetime;
            smokeSize.enabled = true;
            AnimationCurve smokeSizeCurve = new AnimationCurve();
            smokeSizeCurve.AddKey(0f, 0.4f);
            smokeSizeCurve.AddKey(0.2f, 1.0f);
            smokeSizeCurve.AddKey(1f, 1.4f); // Phồng to dần rồi mờ hẳn
            smokeSize = smokePs.sizeOverLifetime;
            smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);

            var smokeColor = smokePs.colorOverLifetime;
            smokeColor.enabled = true;
            Gradient smokeGrad = new Gradient();
            // Khói bụi xám sẫm mờ dần
            smokeGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.18f, 0.15f, 0.22f), 0f), new GradientColorKey(new Color(0.12f, 0.12f, 0.15f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.6f, 0.15f), new GradientAlphaKey(0f, 1.0f) }
            );
            smokeColor.color = smokeGrad;

            var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
            smokeRenderer.sharedMaterial = smokeMat;

            // B. Đá tảng văng lên bắn ra ngoài (Rock Debris)
            GameObject rockGo = new GameObject("Impact_Rocks");
            rockGo.transform.parent = go.transform;
            rockGo.transform.localPosition = Vector3.zero;
            rockGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var rockPs = rockGo.AddComponent<ParticleSystem>();
            var rockMain = rockPs.main;
            rockMain.duration = 1.0f;
            rockMain.loop = false;
            rockMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            rockMain.startSpeed = new ParticleSystem.MinMaxCurve(5f, 9f);
            rockMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.38f);
            rockMain.startRotation3D = true; // Xoay 3D ngẫu nhiên các tảng đá
            rockMain.startRotationX = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            rockMain.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            rockMain.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            rockMain.gravityModifier = 1.2f; // Rơi xuống đất
            rockMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var rockEmission = rockPs.emission;
            rockEmission.rateOverTime = 0f;
            rockEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 14) });

            var rockShape = rockPs.shape;
            rockShape.shapeType = ParticleSystemShapeType.Cone;
            rockShape.angle = 35f;
            rockShape.radius = 0.1f;

            var rockRot = rockPs.rotationOverLifetime;
            rockRot.enabled = true;
            rockRot.x = new ParticleSystem.MinMaxCurve(1f, 3f);
            rockRot.y = new ParticleSystem.MinMaxCurve(1f, 3f);
            rockRot.z = new ParticleSystem.MinMaxCurve(1f, 3f); // Xoay điên cuồng trong không khí

            var rockRenderer = rockGo.GetComponent<ParticleSystemRenderer>();
            if (rockMesh != null)
            {
                rockRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                rockRenderer.mesh = rockMesh;
            }
            rockRenderer.sharedMaterial = rockMat != null ? rockMat : smokeMat; // Dùng vật liệu sàn xám tối

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateGiantTearWavePrefab(string path, Material slashMat, Material sparksMat, GameObject crackPrefab, GameObject dustPrefab, GameObject explosionPrefab, Mesh slashMesh)
        {
            // Đối tượng gốc của kiếm khí khổng lồ rẽ đất
            GameObject go = new GameObject("VFX_Giant_Tear_Wave");
            go.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f); // Kích thước khổng lồ

            // 1. Kết xuất lưới kiếm khí dựng đứng
            if (slashMesh != null)
            {
                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = slashMesh;

                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = slashMat;
            }

            // 2. Cài đặt script điều khiển di chuyển rẽ đất
            var proj = go.AddComponent<SlashWaveTear>();
            proj.speed = 18f;
            proj.lifetime = 1.5f;
            proj.groundCrackPrefab = crackPrefab;
            proj.dustImpactPrefab = dustPrefab;
            proj.terminalExplosionPrefab = explosionPrefab;

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0f, 1.2f);
            col.size = new Vector3(3f, 1f, 1f);

            // 3. Hệ thống hạt đuôi lấp lánh (Trail Sparks)
            GameObject sparksGo = new GameObject("Trail_Sparks");
            sparksGo.transform.parent = go.transform;
            sparksGo.transform.localPosition = new Vector3(0f, 0f, 0.8f);
            sparksGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var sparksPs = sparksGo.AddComponent<ParticleSystem>();
            var sparksMain = sparksPs.main;
            sparksMain.duration = 1.0f;
            sparksMain.loop = true;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPs.emission;
            sparksEmission.rateOverTime = 30f;

            var sparksShape = sparksPs.shape;
            sparksShape.shapeType = ParticleSystemShapeType.Sphere;
            sparksShape.radius = 0.2f;

            var sparksRenderer = sparksGo.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.sharedMaterial = sparksMat;
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.lengthScale = 2.0f;
            sparksRenderer.velocityScale = 0.05f;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
