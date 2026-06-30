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

            // Tải các tài nguyên nền
            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png");
            Texture2D voidRampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_void.png");
            Texture2D colorRampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png");
            Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_star_01.png");
            Texture2D emberTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png");
            Mesh slashMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_slash_01.asset");
            GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VfxSandbox/Prefabs/vfx_prefab_explosion.prefab");

            // 1. Tạo 5 phiên bản Phong cách kiếm khí riêng biệt
            // A. Ma pháp (Tím hồng vũ trụ mặc định)
            CreateStyle(prefabDir, matDir, "magic", 
                new Color(1f, 1f, 1f), new Color(0.05f, 0f, 0.15f), new Color(0f, 0.95f, 1f), 4.5f, 4f, -8f, 0.14f,
                new Color(0f, 1f, 1f), new Color(0f, 0.2f, 1f), starTex, slashMesh, noiseTex, voidRampTex, explosionPrefab);

            // B. Rực lửa (Hỏa kiếm đỏ cam)
            CreateStyle(prefabDir, matDir, "fire", 
                new Color(1f, 0.2f, 0f), new Color(0.2f, 0.02f, 0f), new Color(1f, 0.9f, 0.2f), 6.0f, 3.5f, -6f, 0.1f,
                new Color(1f, 0.4f, 0f), new Color(1f, 0.1f, 0f), emberTex, slashMesh, noiseTex, colorRampTex, explosionPrefab);

            // C. Băng giá (Hàn kiếm lam tuyết)
            CreateStyle(prefabDir, matDir, "ice", 
                new Color(0.3f, 0.6f, 1.0f), new Color(0f, 0.05f, 0.15f), new Color(0.7f, 0.95f, 1.0f), 4.0f, 3f, -5f, 0.08f,
                new Color(0.8f, 0.95f, 1f), new Color(0.2f, 0.5f, 1f), starTex, slashMesh, noiseTex, voidRampTex, explosionPrefab);

            // D. Lốc xoáy (Phong kiếm xanh ngọc/bạc)
            CreateStyle(prefabDir, matDir, "wind", 
                new Color(0.1f, 0.8f, 0.4f), new Color(0f, 0.15f, 0.08f), new Color(0.6f, 1.0f, 0.8f), 3.8f, 5f, -12f, 0.12f,
                new Color(0.5f, 1f, 0.7f), new Color(0f, 0.5f, 0.2f), starTex, slashMesh, noiseTex, colorRampTex, explosionPrefab);

            // E. Sấm sét (Lôi kiếm tím điện/vàng rực giật mạnh)
            CreateStyle(prefabDir, matDir, "lightning", 
                new Color(0.5f, 0f, 1.0f), new Color(0.08f, 0f, 0.15f), new Color(0f, 1.0f, 1.0f), 7.0f, 7.5f, -18f, 0.26f,
                new Color(0.8f, 1f, 0f), new Color(0f, 0.8f, 1f), emberTex, slashMesh, noiseTex, voidRampTex, explosionPrefab);

            AssetDatabase.Refresh();
            Debug.Log("✓ All 5 Magic Slash styles generated successfully!");
        }

        private static void CreateStyle(string prefabDir, string matDir, string styleName, 
            Color tintColor, Color voidColor, Color coreColor, float intensity, float waveCount, float waveSpeed, float waveAmplitude,
            Color sparkColorStart, Color sparkColorEnd, Texture2D sparkTex, Mesh slashMesh, Texture2D noiseTex, Texture2D rampTex, GameObject explosionPrefab)
        {
            // 1. Tạo vật liệu vệt chém
            string slashMatPath = $"{matDir}/mat_magic_slash_{styleName}.mat";
            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>(slashMatPath);
            if (slashMat == null)
            {
                slashMat = new Material(Shader.Find("VFX/MagicSlash"));
                AssetDatabase.CreateAsset(slashMat, slashMatPath);
            }
            slashMat.SetColor("_ColorTint", tintColor);
            slashMat.SetColor("_VoidColor", voidColor);
            slashMat.SetColor("_CoreColor", coreColor);
            slashMat.SetFloat("_Intensity", intensity);
            slashMat.SetFloat("_WaveCount", waveCount);
            slashMat.SetFloat("_WaveSpeed", waveSpeed);
            slashMat.SetFloat("_WaveAmplitude", waveAmplitude);
            if (noiseTex != null) slashMat.SetTexture("_NoiseMap", noiseTex);
            if (rampTex != null) slashMat.SetTexture("_RampMap", rampTex);

            // 2. Tạo vật liệu tia lửa
            string sparksMatPath = $"{matDir}/mat_slash_sparks_{styleName}.mat";
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>(sparksMatPath);
            if (sparksMat == null)
            {
                sparksMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(sparksMat, sparksMatPath);
            }
            sparksMat.SetFloat("_Surface", 1.0f); // Transparent
            sparksMat.SetFloat("_Blend", 1.0f);   // Additive
            sparksMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            sparksMat.EnableKeyword("_BLENDMODE_ADDITIVE");
            sparksMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            sparksMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            sparksMat.SetInt("_ZWrite", 0);
            sparksMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (sparkTex != null) sparksMat.SetTexture("_BaseMap", sparkTex);

            AssetDatabase.SaveAssets();

            // 3. Dựng Prefab vệt chém tại chỗ
            string slashPrefabPath = $"{prefabDir}/vfx_prefab_slash_{styleName}.prefab";
            CreateSlashPrefab(slashPrefabPath, slashMat, sparksMat, slashMesh, sparkColorStart, sparkColorEnd);

            // 4. Dựng Prefab kiếm khí phóng đi
            string wavePrefabPath = $"{prefabDir}/vfx_prefab_slash_wave_{styleName}.prefab";
            CreateSlashWavePrefab(wavePrefabPath, slashMat, sparksMat, explosionPrefab, slashMesh, sparkColorStart, sparkColorEnd);
        }

        private static void CreateSlashPrefab(string path, Material slashMat, Material sparksMat, Mesh slashMesh, Color sparkColorStart, Color sparkColorEnd)
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

                meshGo.AddComponent<VfxSlashAnimator>();
            }

            // B. Đối tượng con phun tia lửa (Sparks System) bay dọc theo đường chém
            GameObject sparksGo = new GameObject("Slash_Sparks");
            sparksGo.transform.parent = go.transform;
            sparksGo.transform.localPosition = Vector3.zero;
            sparksGo.transform.localRotation = Quaternion.identity;

            var sparksPs = sparksGo.AddComponent<ParticleSystem>();
            var sparksMain = sparksPs.main;
            sparksMain.duration = 0.11f;
            sparksMain.loop = false;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            sparksMain.gravityModifier = 0.25f;
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPs.emission;
            sparksEmission.rateOverTime = 160f;

            var sparksShape = sparksPs.shape;
            sparksShape.shapeType = ParticleSystemShapeType.Sphere;
            sparksShape.radius = 0.08f;
            sparksShape.randomDirectionAmount = 0.6f;

            var sparksSize = sparksPs.sizeOverLifetime;
            sparksSize.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1.0f);
            sizeCurve.AddKey(1f, 0.0f);
            sparksSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var sparksColor = sparksPs.colorOverLifetime;
            sparksColor.enabled = true;
            Gradient sparksGrad = new Gradient();
            sparksGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(sparkColorStart, 0f), new GradientColorKey(sparkColorEnd, 0.7f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
            );
            sparksColor.color = sparksGrad;

            var sparksRenderer = sparksGo.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.sharedMaterial = sparksMat;
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.lengthScale = 2.8f;
            sparksRenderer.velocityScale = 0.08f;

            // C. Đối tượng con sinh Ngôi Sao 4 Cánh Lấp Lánh (Star Flares) bay lơ lửng bám theo mũi chém
            GameObject flaresGo = new GameObject("Slash_StarFlares");
            flaresGo.transform.parent = sparksGo.transform;
            flaresGo.transform.localPosition = Vector3.zero;
            flaresGo.transform.localRotation = Quaternion.identity;

            var flaresPs = flaresGo.AddComponent<ParticleSystem>();
            var flaresMain = flaresPs.main;
            flaresMain.duration = 0.11f;
            flaresMain.loop = false;
            flaresMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            flaresMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            flaresMain.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
            flaresMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            flaresMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var flaresEmission = flaresPs.emission;
            flaresEmission.rateOverTime = 80f;

            var flaresShape = flaresPs.shape;
            flaresShape.shapeType = ParticleSystemShapeType.Sphere;
            flaresShape.radius = 0.05f;

            var flaresRot = flaresPs.rotationOverLifetime;
            flaresRot.enabled = true;
            flaresRot.z = new ParticleSystem.MinMaxCurve(90f * Mathf.Deg2Rad, 270f * Mathf.Deg2Rad);

            var flaresSize = flaresPs.sizeOverLifetime;
            flaresSize.enabled = true;
            flaresSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var flaresColor = flaresPs.colorOverLifetime;
            flaresColor.enabled = true;
            Gradient flaresGrad = new Gradient();
            flaresGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(sparkColorStart, 0f), new GradientColorKey(sparkColorEnd, 0.6f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
            );
            flaresColor.color = flaresGrad;

            var flaresRenderer = flaresGo.GetComponent<ParticleSystemRenderer>();
            flaresRenderer.sharedMaterial = sparksMat;
            flaresRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateSlashWavePrefab(string path, Material slashMat, Material sparksMat, GameObject explosionPrefab, Mesh slashMesh, Color sparkColorStart, Color sparkColorEnd)
        {
            GameObject go = new GameObject("VFX_Slash_Wave_Projectile");
            
            if (slashMesh != null)
            {
                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = slashMesh;

                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = slashMat;
            }

            var proj = go.AddComponent<SlashWaveProjectile>();
            proj.speed = 22f;
            proj.lifetime = 1.2f;
            proj.explosionPrefab = explosionPrefab;

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0f, 2.5f);
            col.size = new Vector3(5f, 1f, 2f);

            // Bột khí và tia lửa đuôi (Trail Sparks)
            GameObject trailGo = new GameObject("Trail_Sparks");
            trailGo.transform.parent = go.transform;
            trailGo.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            trailGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var trailPs = trailGo.AddComponent<ParticleSystem>();
            var trailMain = trailPs.main;
            trailMain.duration = 1f;
            trailMain.loop = true;
            trailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            trailMain.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            trailMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            trailMain.gravityModifier = 0.1f;
            trailMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var trailEmission = trailPs.emission;
            trailEmission.rateOverTime = 40f;

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
                new GradientColorKey[] { new GradientColorKey(sparkColorStart, 0f), new GradientColorKey(sparkColorEnd, 0.7f) },
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
