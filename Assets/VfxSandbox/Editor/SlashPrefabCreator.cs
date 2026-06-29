using System.IO;
using UnityEditor;
using UnityEngine;

namespace VfxSandbox.Editor
{
    public class SlashPrefabCreator
    {
        [MenuItem("Window/VFX/Generate Slash Prefabs (Non-Destructive)")]
        public static void GeneratePrefabs()
        {
            string prefabDir = "Assets/VfxSandbox/Prefabs";
            string matDir = "Assets/VfxSandbox/Materials";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            // 1. Tạo Vật liệu (Materials)
            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png");
            Texture2D voidRampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_void.png");
            Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_star_01.png");
            Mesh slashMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_slash_01.asset");

            // A. Magic Slash Material (Custom Slash Shader)
            string slashMatPath = matDir + "/mat_magic_slash.mat";
            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>(slashMatPath);
            if (slashMat == null)
            {
                slashMat = new Material(Shader.Find("VFX/MagicSlash"));
                AssetDatabase.CreateAsset(slashMat, slashMatPath);
            }
            slashMat.SetColor("_ColorTint", Color.white); // Đặt Tint là trắng vì Ramp Void đã chứa sẵn dải màu đa sắc tím-lam cực đẹp
            if (noiseTex != null) slashMat.SetTexture("_NoiseMap", noiseTex);
            if (voidRampTex != null) slashMat.SetTexture("_RampMap", voidRampTex);
            slashMat.SetFloat("_Intensity", 4.5f);
            slashMat.SetVector("_ScrollSpeed", new Vector4(-2f, 0.4f, 0f, 0f));

            // B. Sparks Material (URP Additive Particles)
            string sparksMatPath = matDir + "/mat_slash_sparks.mat";
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>(sparksMatPath);
            if (sparksMat == null)
            {
                sparksMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(sparksMat, sparksMatPath);
            }
            sparksMat.SetFloat("_Surface", 1.0f); // Transparent
            sparksMat.SetFloat("_Blend", 1.0f);   // Additive blending
            sparksMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            sparksMat.EnableKeyword("_BLENDMODE_ADDITIVE");
            sparksMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            sparksMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            sparksMat.SetInt("_ZWrite", 0);
            sparksMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (starTex != null) sparksMat.SetTexture("_BaseMap", starTex);

            AssetDatabase.SaveAssets();

            // 2. Dựng Prefab Cung Chém (vfx_prefab_slash.prefab)
            string prefabPath = prefabDir + "/vfx_prefab_slash.prefab";
            CreateSlashPrefab(prefabPath, slashMat, sparksMat, slashMesh);

            // 3. Dựng Prefab Kiếm Khí (vfx_prefab_slash_wave.prefab)
            GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_explosion.prefab");
            string wavePath = prefabDir + "/vfx_prefab_slash_wave.prefab";
            CreateSlashWavePrefab(wavePath, slashMat, sparksMat, explosionPrefab, slashMesh);

            AssetDatabase.Refresh();
            Debug.Log("✓ Magic Slash prefabs and materials generated successfully!");
        }

        private static void CreateSlashPrefab(string path, Material slashMat, Material sparksMat, Mesh slashMesh)
        {
            GameObject go = new GameObject("VFX_Magic_Slash_Root");

            // A. Đối tượng con chứa Mesh Cung chém chính
            GameObject meshGo = new GameObject("Slash_Mesh");
            meshGo.transform.parent = go.transform;
            meshGo.transform.localPosition = Vector3.zero;
            meshGo.transform.localRotation = Quaternion.identity;

            if (slashMesh != null)
            {
                var filter = meshGo.AddComponent<MeshFilter>();
                filter.sharedMesh = slashMesh;

                var renderer = meshGo.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = slashMat;

                // Gán script tự động quét và mờ dần
                meshGo.AddComponent<VfxSlashAnimator>();
            }

            // B. Đối tượng con phun tia lửa (Sparks System) bay dọc theo đường chém
            GameObject sparksGo = new GameObject("Slash_Sparks");
            sparksGo.transform.parent = go.transform;
            sparksGo.transform.localPosition = Vector3.zero;
            sparksGo.transform.localRotation = Quaternion.identity;

            var sparksPs = sparksGo.AddComponent<ParticleSystem>();
            var sparksMain = sparksPs.main;
            sparksMain.duration = 0.11f; // Phát tia lửa đúng trong thời gian quét chém nhanh
            sparksMain.loop = false;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            sparksMain.gravityModifier = 0.25f; // Rơi nhẹ
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPs.emission;
            sparksEmission.rateOverTime = 160f; // Phun liên tục cực đại dọc đường đi của mũi chém
            sparksEmission.SetBursts(new ParticleSystem.Burst[] { }); // Không dùng Burst tĩnh

            var sparksShape = sparksPs.shape;
            sparksShape.shapeType = ParticleSystemShapeType.Sphere; // Emitter dạng khối cầu nhỏ di động
            sparksShape.radius = 0.08f;
            sparksShape.randomDirectionAmount = 0.6f; // Hướng bung tia lửa mạnh mẽ

            var sparksSize = sparksPs.sizeOverLifetime;
            sparksSize.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1.0f);
            sizeCurve.AddKey(1f, 0.0f); // Thu nhỏ dần rồi biến mất
            sparksSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var sparksColor = sparksPs.colorOverLifetime;
            sparksColor.enabled = true;
            Gradient sparksGrad = new Gradient();
            // Lửa điện lam ma thuật (Cyan rực sáng chuyển sang lam sẫm rồi tắt)
            sparksGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0f, 1f, 1f), 0f), new GradientColorKey(new Color(0f, 0.2f, 1f), 0.7f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
            );
            sparksColor.color = sparksGrad;

            var sparksRenderer = sparksGo.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.sharedMaterial = sparksMat;
            // Stretch tạo vệt dài tốc độ
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.lengthScale = 2.8f;
            sparksRenderer.velocityScale = 0.08f; // Sửa speedScale thành velocityScale

            // C. Đối tượng con sinh Ngôi Sao 4 Cánh Lấp Lánh (Star Flares) bay lơ lửng bám theo mũi chém
            GameObject flaresGo = new GameObject("Slash_StarFlares");
            flaresGo.transform.parent = sparksGo.transform; // Làm con của sparksGo để tự động di chuyển đồng bộ
            flaresGo.transform.localPosition = Vector3.zero;
            flaresGo.transform.localRotation = Quaternion.identity;

            var flaresPs = flaresGo.AddComponent<ParticleSystem>();
            var flaresMain = flaresPs.main;
            flaresMain.duration = 0.11f; // Đồng bộ thời gian quét chém
            flaresMain.loop = false;
            flaresMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            flaresMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            flaresMain.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
            flaresMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad); // Xoay ngẫu nhiên ngôi sao
            flaresMain.simulationSpace = ParticleSystemSimulationSpace.World; // Bay lơ lửng lại trong thế giới

            var flaresEmission = flaresPs.emission;
            flaresEmission.rateOverTime = 80f; // Phun liên tục rực rỡ lấp lánh
            flaresEmission.SetBursts(new ParticleSystem.Burst[] { });

            var flaresShape = flaresPs.shape;
            flaresShape.shapeType = ParticleSystemShapeType.Sphere;
            flaresShape.radius = 0.05f;

            var flaresRot = flaresPs.rotationOverLifetime;
            flaresRot.enabled = true;
            flaresRot.z = new ParticleSystem.MinMaxCurve(90f * Mathf.Deg2Rad, 270f * Mathf.Deg2Rad); // Xoay tự động lấp lánh

            var flaresSize = flaresPs.sizeOverLifetime;
            flaresSize.enabled = true;
            flaresSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var flaresColor = flaresPs.colorOverLifetime;
            flaresColor.enabled = true;
            Gradient flaresGrad = new Gradient();
            // Lấp lánh màu vàng/cam chuyển sang hồng tím ma thuật rực rỡ
            flaresGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0f), 0f), new GradientColorKey(new Color(0.9f, 0f, 0.7f), 0.6f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
            );
            flaresColor.color = flaresGrad;

            var flaresRenderer = flaresGo.GetComponent<ParticleSystemRenderer>();
            flaresRenderer.sharedMaterial = sparksMat;
            flaresRenderer.renderMode = ParticleSystemRenderMode.Billboard; // Giữ nguyên hình dáng sao 4 cánh, không kéo giãn dẹt

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateSlashWavePrefab(string path, Material slashMat, Material sparksMat, GameObject explosionPrefab, Mesh slashMesh)
        {
            // Tạo đối tượng gốc của kiếm khí bay đi
            GameObject go = new GameObject("VFX_Slash_Wave_Projectile");
            
            // 1. Thêm bộ lọc và kết xuất lưới để vẽ kiếm khí dựng đứng
            if (slashMesh != null)
            {
                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = slashMesh;

                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = slashMat;
            }

            // 2. Thêm script chuyển động bay đi và phát nổ khi va đập
            var proj = go.AddComponent<SlashWaveProjectile>();
            proj.speed = 22f;
            proj.lifetime = 1.2f;
            proj.explosionPrefab = explosionPrefab;

            // Thêm collider để nhận biết va chạm (Trigger)
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0f, 2.5f);
            col.size = new Vector3(5f, 1f, 2f);

            // 3. Thêm hệ thống hạt đuôi lửa xẹt sau (Trail Sparks)
            GameObject trailGo = new GameObject("Trail_Sparks");
            trailGo.transform.parent = go.transform;
            trailGo.transform.localPosition = new Vector3(0f, 0f, 1.5f); // Đặt ở sau mũi kiếm khí
            trailGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Phun ngược lại phía sau

            var trailPs = trailGo.AddComponent<ParticleSystem>();
            var trailMain = trailPs.main;
            trailMain.duration = 1f;
            trailMain.loop = true;
            trailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            trailMain.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            trailMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            trailMain.gravityModifier = 0.1f;
            trailMain.simulationSpace = ParticleSystemSimulationSpace.World; // Hạt bay lơ lửng lại trong thế giới

            var trailEmission = trailPs.emission;
            trailEmission.rateOverTime = 40f; // Phun 40 hạt/s để làm đuôi dày

            var trailShape = trailPs.shape;
            trailShape.shapeType = ParticleSystemShapeType.Sphere;
            trailShape.radius = 0.3f;
            trailShape.randomDirectionAmount = 0.2f;

            var trailSize = trailPs.sizeOverLifetime;
            trailSize.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1.0f);
            sizeCurve.AddKey(1f, 0.0f);
            trailSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var trailColor = trailPs.colorOverLifetime;
            trailColor.enabled = true;
            Gradient trailGrad = new Gradient();
            trailGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0f, 0.8f, 1f), 0f), new GradientColorKey(new Color(0f, 0.1f, 0.6f), 0.7f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
            );
            trailColor.color = trailGrad;

            var trailRenderer = trailGo.GetComponent<ParticleSystemRenderer>();
            trailRenderer.sharedMaterial = sparksMat;
            trailRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            trailRenderer.lengthScale = 2.0f;
            trailRenderer.velocityScale = 0.05f;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
