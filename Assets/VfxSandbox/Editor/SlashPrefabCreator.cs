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
            Texture2D rampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png");
            Texture2D emberTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png");
            Mesh slashMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_slash_01.asset");

            // A. Magic Slash Material (Custom Slash Shader)
            string slashMatPath = matDir + "/mat_magic_slash.mat";
            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>(slashMatPath);
            if (slashMat == null)
            {
                slashMat = new Material(Shader.Find("VFX/MagicSlash"));
                AssetDatabase.CreateAsset(slashMat, slashMatPath);
            }
            slashMat.SetColor("_ColorTint", new Color(0f, 0.65f, 1f, 1f)); // Màu lam ma thuật rực rỡ
            if (noiseTex != null) slashMat.SetTexture("_NoiseMap", noiseTex);
            if (rampTex != null) slashMat.SetTexture("_RampMap", rampTex);
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
            if (emberTex != null) sparksMat.SetTexture("_BaseMap", emberTex);

            AssetDatabase.SaveAssets();

            // 2. Dựng Prefab Cung Chém (vfx_prefab_slash.prefab)
            string prefabPath = prefabDir + "/vfx_prefab_slash.prefab";
            CreateSlashPrefab(prefabPath, slashMat, sparksMat, slashMesh);

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

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
