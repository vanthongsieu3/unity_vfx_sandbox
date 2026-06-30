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

            // Cấu hình các Texture trước khi tải để đảm bảo nhận diện đúng Alpha kênh trong suốt
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_star_01.png", false);
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png", false);
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png", false);
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_ramp_void.png", false);
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png", false);
            ConfigureTexture("Assets/VfxSandbox/Textures/vfx_tex_skull_01.png", false);

            // Tải các tài nguyên nền
            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_noise_01.png");
            Texture2D voidRampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_void.png");
            Texture2D colorRampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ramp_01.png");
            Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_star_01.png");
            Texture2D emberTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_ember_01.png");
            Texture2D skullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VfxSandbox/Textures/vfx_tex_skull_01.png");

            // Tải các Meshes đặc trưng để thay đổi hình thái kiếm khí
            Mesh slashMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_slash_01.asset");
            Mesh coneMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_cone_01.asset");
            Mesh funnelMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_funnel_01.asset");
            Mesh ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_ring_01.asset");

            // Tải các vật liệu phụ trợ cho vụ nổ
            Material fireRingMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_fire_ring.mat");
            Material smokeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_explosion_smoke.mat");
            
            // Tạo vật liệu đầu lâu riêng cho hệ Địa Ngục
            string skullMatPath = matDir + "/mat_slash_skulls_hell.mat";
            Material skullMat = AssetDatabase.LoadAssetAtPath<Material>(skullMatPath);
            if (skullMat == null)
            {
                skullMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(skullMat, skullMatPath);
            }
            skullMat.SetOverrideTag("RenderType", "Transparent");
            skullMat.SetFloat("_Surface", 1.0f); // Transparent
            skullMat.SetFloat("_Blend", 0.0f);   // Alpha blend
            skullMat.SetInt("_SrcBlend", 5);     // BlendMode.SrcAlpha
            skullMat.SetInt("_DstBlend", 10);    // BlendMode.OneMinusSrcAlpha
            skullMat.SetInt("_ZWrite", 0);
            skullMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            skullMat.renderQueue = 3000;
            if (skullTex != null)
            {
                skullMat.SetTexture("_BaseMap", skullTex);
                skullMat.SetTexture("_MainTex", skullTex);
            }

            // 1. Tạo 6 phiên bản Phong cách kiếm khí riêng biệt (Slash + Projectile + Custom Explosion)
            // A. Ma pháp (Tím hồng vũ trụ mặc định)
            CreateStyle(prefabDir, matDir, "magic", 
                new Color(1f, 1f, 1f), new Color(0.05f, 0f, 0.15f), new Color(0f, 0.95f, 1f), 4.5f, 4f, -8f, 0.14f,
                new Color(0f, 1f, 1f), new Color(0f, 0.2f, 1f), starTex, slashMesh, noiseTex, voidRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // B. Rực lửa (Hỏa kiếm đỏ cam + Khói dày + Bùng nổ lửa lớn)
            CreateStyle(prefabDir, matDir, "fire", 
                new Color(1f, 0.2f, 0f), new Color(0.2f, 0.02f, 0f), new Color(1f, 0.9f, 0.2f), 6.0f, 3.5f, -6f, 0.1f,
                new Color(1f, 0.4f, 0f), new Color(1f, 0.1f, 0f), emberTex, slashMesh, noiseTex, colorRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // C. Băng giá (Đinh băng nhọn hoắt + Băng vụn vỡ radial khi nổ)
            CreateStyle(prefabDir, matDir, "ice", 
                new Color(0.3f, 0.6f, 1.0f), new Color(0f, 0.05f, 0.15f), new Color(0.7f, 0.95f, 1.0f), 4.0f, 3f, -5f, 0.08f,
                new Color(0.8f, 0.95f, 1f), new Color(0.2f, 0.5f, 1f), starTex, slashMesh, noiseTex, voidRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // D. Lốc xoáy (Phong kiếm lốc cuộn + Vòi rồng mini xoáy tít khi nổ)
            CreateStyle(prefabDir, matDir, "wind", 
                new Color(0.1f, 0.8f, 0.4f), new Color(0f, 0.15f, 0.08f), new Color(0.6f, 1.0f, 0.8f), 3.8f, 5f, -12f, 0.12f,
                new Color(0.5f, 1f, 0.7f), new Color(0f, 0.5f, 0.2f), starTex, slashMesh, noiseTex, colorRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // E. Sấm sét (Lôi kiếm giật răng cưa điện + Chớp sét nổ ziczac)
            CreateStyle(prefabDir, matDir, "lightning", 
                new Color(0.5f, 0f, 1.0f), new Color(0.08f, 0f, 0.15f), new Color(0f, 1.0f, 1.0f), 7.0f, 7.5f, -18f, 0.26f,
                new Color(0.8f, 1f, 0f), new Color(0f, 0.8f, 1f), emberTex, slashMesh, noiseTex, voidRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // F. Địa ngục (Lửa đen quỷ kiếm + Phóng đầu lâu to + Vụ nổ hồn ma đầu lâu)
            CreateStyle(prefabDir, matDir, "hell", 
                new Color(0.4f, 0.0f, 0.0f), new Color(0.01f, 0.0f, 0.02f), new Color(0.15f, 0.0f, 0.0f), 2.2f, 3.8f, -4f, 0.16f,
                new Color(0.3f, 0.0f, 0.0f), new Color(0.08f, 0.0f, 0.1f), emberTex, slashMesh, noiseTex, voidRampTex, 
                fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            AssetDatabase.Refresh();
            Debug.Log("✓ All 6 Magic Slash elements and themed explosions generated successfully!");
        }

        private static void ConfigureTexture(string assetPath, bool isNormalMap)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = isNormalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput; // Lấy alpha từ ảnh đầu vào
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp; // Chặn lặp cạnh viền làm lộ khung ô vuông
                importer.SaveAndReimport();
            }
        }

        private static void CreateStyle(string prefabDir, string matDir, string styleName, 
            Color tintColor, Color voidColor, Color coreColor, float intensity, float waveCount, float waveSpeed, float waveAmplitude,
            Color sparkColorStart, Color sparkColorEnd, Texture2D sparkTex, Mesh slashMesh, Texture2D noiseTex, Texture2D rampTex,
            Material fireRingMat, Material smokeMat, Mesh ringMesh, Texture2D skullTex, Material skullMat, Mesh coneMesh, Mesh funnelMesh)
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
            // Thiết lập đầy đủ tag RenderType, Queue và Keywords để URP không ngộ nhận là Opaque
            sparksMat.SetOverrideTag("RenderType", "Transparent");
            sparksMat.SetFloat("_Surface", 1.0f); // Transparent
            sparksMat.SetFloat("_Blend", 1.0f);   // Additive
            sparksMat.SetInt("_SrcBlend", 5);     // BlendMode.SrcAlpha (Thích hợp cho Straight Alpha PNG)
            sparksMat.SetInt("_DstBlend", 1);     // BlendMode.One (Tạo hiệu ứng cộng màu phát sáng cực sạch)
            sparksMat.SetInt("_ZWrite", 0);
            sparksMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            sparksMat.renderQueue = 3000;
            if (sparkTex != null)
            {
                sparksMat.SetTexture("_BaseMap", sparkTex);
                sparksMat.SetTexture("_MainTex", sparkTex);
            }

            AssetDatabase.SaveAssets();

            // 3. Dựng Prefab vệt chém tại chỗ (Combo nhát 1 và 2)
            string slashPrefabPath = $"{prefabDir}/vfx_prefab_slash_{styleName}.prefab";
            CreateSlashPrefab(slashPrefabPath, slashMat, sparksMat, slashMesh, sparkColorStart, sparkColorEnd);

            // 4. Dựng Prefab Vụ nổ đặc trưng nguyên tố riêng biệt (Custom Element Explosion)
            string explosionPath = $"{prefabDir}/vfx_prefab_explosion_{styleName}.prefab";
            GameObject customExplosion = CreateThemedExplosionPrefab(explosionPath, styleName, sparksMat, fireRingMat, smokeMat, ringMesh, skullTex, skullMat, coneMesh, funnelMesh);

            // 5. Dựng Prefab Kiếm khí phóng đi (Projectile) với mô hình hình thái thay đổi hoàn toàn
            string wavePrefabPath = $"{prefabDir}/vfx_prefab_slash_wave_{styleName}.prefab";
            CreateSlashWavePrefab(wavePrefabPath, slashMat, sparksMat, customExplosion, slashMesh, sparkColorStart, sparkColorEnd, styleName, skullTex, skullMat, coneMesh, funnelMesh);
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

        private static GameObject CreateThemedExplosionPrefab(string path, string styleName, Material sparksMat, Material fireRingMat, Material smokeMat, Mesh ringMesh, Texture2D skullTex, Material skullMat, Mesh coneMesh, Mesh funnelMesh)
        {
            GameObject go = new GameObject($"VFX_Explosion_{styleName}");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 0.8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.4f);
            curve.AddKey(0.2f, 1.0f);
            curve.AddKey(1f, 0f);
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient grad = new Gradient();

            if (styleName == "magic")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0f, 0.95f, 1f), 0f), new GradientColorKey(new Color(0.6f, 0f, 1f), 0.5f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }
            else if (styleName == "fire")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.3f, 0f), 0.4f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }
            else if (styleName == "ice")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.3f, 0.7f, 1f), 0.4f), new GradientColorKey(new Color(0.0f, 0.2f, 0.4f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }
            else if (styleName == "wind")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.8f, 1f, 0.9f), 0f), new GradientColorKey(new Color(0.1f, 0.7f, 0.3f), 0.5f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }
            else if (styleName == "lightning")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.8f, 1f, 0f), 0.3f), new GradientColorKey(new Color(0.4f, 0f, 1f), 0.8f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }
            else if (styleName == "hell")
            {
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.8f, 0f, 0f), 0f), new GradientColorKey(new Color(0.1f, 0f, 0.15f), 0.6f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
            }

            color.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = sparksMat;

            // 1. Tạo vòng tròn lan tỏa (Ring Mesh)
            if (ringMesh != null && fireRingMat != null)
            {
                GameObject ringGo = new GameObject("Explosion_Ring");
                ringGo.transform.parent = go.transform;
                ringGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                ringGo.transform.localRotation = Quaternion.identity;

                var ringFilter = ringGo.AddComponent<MeshFilter>();
                ringFilter.sharedMesh = ringMesh;

                var ringRenderer = ringGo.AddComponent<MeshRenderer>();
                // Dùng chung vật liệu ring nhưng nhân bản để chuyển màu theo hệ
                Material tempRingMat = new Material(fireRingMat);
                if (styleName == "magic") tempRingMat.SetColor("_ColorTint", new Color(0f, 0.9f, 1f, 1f));
                else if (styleName == "fire") tempRingMat.SetColor("_ColorTint", new Color(1f, 0.3f, 0f, 1f));
                else if (styleName == "ice") tempRingMat.SetColor("_ColorTint", new Color(0.3f, 0.7f, 1f, 1f));
                else if (styleName == "wind") tempRingMat.SetColor("_ColorTint", new Color(0.1f, 0.8f, 0.4f, 1f));
                else if (styleName == "lightning") tempRingMat.SetColor("_ColorTint", new Color(0.8f, 1f, 0f, 1f));
                else if (styleName == "hell") tempRingMat.SetColor("_ColorTint", new Color(0.4f, 0f, 0.05f, 1f));

                ringRenderer.sharedMaterial = tempRingMat;

                var scaleScript = ringGo.AddComponent<VfxScaleAndFade>();
                scaleScript.startScale = Vector3.one * 0.2f;
                scaleScript.endScale = Vector3.one * 6.5f;
                scaleScript.duration = 0.7f;
            }

            // 2. Tạo hiệu ứng đặc trưng vụ nổ
            if (styleName == "fire" && smokeMat != null)
            {
                // Bùng nổ khói đen cuộn mù mịt
                GameObject smokeGo = new GameObject("Explosion_Smoke");
                smokeGo.transform.parent = go.transform;
                smokeGo.transform.localPosition = Vector3.zero;
                smokeGo.transform.localRotation = Quaternion.identity;

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 1f;
                smokeMain.loop = false;
                smokeMain.startLifetime = 1.2f;
                smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(1.5f, 2.8f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 0f;
                smokeEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 15) });

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.8f)));

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.12f, 0.12f, 0.12f), 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = smokeMat;
            }
            else if (styleName == "ice" && coneMesh != null)
            {
                // Băng hệ: Bắn ra 12 đinh băng nhọn hoắt tỏa tròn radial cực bén
                GameObject spikesGo = new GameObject("Explosion_Ice_Spikes");
                spikesGo.transform.parent = go.transform;
                spikesGo.transform.localPosition = Vector3.zero;
                spikesGo.transform.localRotation = Quaternion.identity;

                var spikesPs = spikesGo.AddComponent<ParticleSystem>();
                var spikesMain = spikesPs.main;
                spikesMain.duration = 1f;
                spikesMain.loop = false;
                spikesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
                spikesMain.startSpeed = new ParticleSystem.MinMaxCurve(8f, 16f);
                spikesMain.startSize3D = true;
                spikesMain.startSizeX = 0.15f;
                spikesMain.startSizeY = 0.15f;
                spikesMain.startSizeZ = 1.5f; // Kéo dẹt nhọn hoắt
                spikesMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var spikesEmission = spikesPs.emission;
                spikesEmission.rateOverTime = 0f;
                spikesEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 12) });

                var spikesShape = spikesPs.shape;
                spikesShape.shapeType = ParticleSystemShapeType.Sphere;
                spikesShape.radius = 0.1f;
                spikesShape.alignToDirection = true; // Quay hướng đinh ra ngoài

                var spikesColor = spikesPs.colorOverLifetime;
                spikesColor.enabled = true;
                Gradient spikesGrad = new Gradient();
                spikesGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f), new GradientColorKey(new Color(0.2f, 0.5f, 0.8f), 0.8f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
                );
                spikesColor.color = spikesGrad;

                var spikesRenderer = spikesGo.GetComponent<ParticleSystemRenderer>();
                spikesRenderer.sharedMaterial = sparksMat;
                spikesRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                spikesRenderer.mesh = coneMesh;
            }
            else if (styleName == "wind" && funnelMesh != null)
            {
                // Phong hệ: Tạo cơn lốc xoáy đứng quay tít tại tâm nổ
                GameObject tornadoGo = new GameObject("Explosion_Tornado");
                tornadoGo.transform.parent = go.transform;
                tornadoGo.transform.localPosition = Vector3.zero;
                tornadoGo.transform.localRotation = Quaternion.identity;

                var tornadoFilter = tornadoGo.AddComponent<MeshFilter>();
                tornadoFilter.sharedMesh = funnelMesh;

                var tornadoRenderer = tornadoGo.AddComponent<MeshRenderer>();
                tornadoRenderer.sharedMaterial = sparksMat;

                var scaleScript = tornadoGo.AddComponent<VfxScaleAndFade>();
                scaleScript.startScale = new Vector3(0.5f, 0.2f, 0.5f);
                scaleScript.endScale = new Vector3(3f, 4.5f, 3f); // Nở to cao lên thành vòi rồng
                scaleScript.duration = 0.8f;
                scaleScript.rotationSpeed = new Vector3(0f, 900f, 0f); // Xoay vòng tròn
            }
            else if (styleName == "lightning")
            {
                // Lôi hệ: Bắn các tia sét giật răng cưa điện xẹt
                GameObject boltsGo = new GameObject("Explosion_Lightning_Bolts");
                boltsGo.transform.parent = go.transform;
                boltsGo.transform.localPosition = Vector3.zero;
                boltsGo.transform.localRotation = Quaternion.identity;

                var boltsPs = boltsGo.AddComponent<ParticleSystem>();
                var boltsMain = boltsPs.main;
                boltsMain.duration = 0.5f;
                boltsMain.loop = false;
                boltsMain.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
                boltsMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f);
                boltsMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
                boltsMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var boltsEmission = boltsPs.emission;
                boltsEmission.rateOverTime = 0f;
                boltsEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });

                var boltsNoise = boltsPs.noise;
                boltsNoise.enabled = true;
                boltsNoise.strength = 3.5f;
                boltsNoise.frequency = 6.0f;
                boltsNoise.scrollSpeed = 2.0f;

                var boltsColor = boltsPs.colorOverLifetime;
                boltsColor.enabled = true;
                Gradient boltsGrad = new Gradient();
                boltsGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0f, 1f, 1f), 0.5f), new GradientColorKey(new Color(0.5f, 0f, 1f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
                );
                boltsColor.color = boltsGrad;

                var boltsRenderer = boltsGo.GetComponent<ParticleSystemRenderer>();
                boltsRenderer.sharedMaterial = sparksMat;
                boltsRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                boltsRenderer.lengthScale = 3f;
            }
            else if (styleName == "hell" && skullMat != null)
            {
                // Địa ngục hệ: Bùng nổ giải phóng 5 linh hồn đầu lâu đen bay sủi lên
                GameObject skullsGo = new GameObject("Explosion_Skulls");
                skullsGo.transform.parent = go.transform;
                skullsGo.transform.localPosition = Vector3.zero;
                skullsGo.transform.localRotation = Quaternion.identity;

                var skullsPs = skullsGo.AddComponent<ParticleSystem>();
                var skullsMain = skullsPs.main;
                skullsMain.duration = 1f;
                skullsMain.loop = false;
                skullsMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
                skullsMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
                skullsMain.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
                skullsMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var skullsEmission = skullsPs.emission;
                skullsEmission.rateOverTime = 0f;
                skullsEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 5) });

                var skullsShape = skullsPs.shape;
                skullsShape.shapeType = ParticleSystemShapeType.Sphere;
                skullsShape.radius = 0.5f;

                var skullsColor = skullsPs.colorOverLifetime;
                skullsColor.enabled = true;
                Gradient skullsGrad = new Gradient();
                skullsGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.9f, 0.9f, 0.9f), 0f), new GradientColorKey(new Color(0.3f, 0f, 0.1f), 0.8f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1.0f) }
                );
                skullsColor.color = skullsGrad;

                var skullsSize = skullsPs.sizeOverLifetime;
                skullsSize.enabled = true;
                skullsSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.2f), new Keyframe(0.2f, 1.0f), new Keyframe(1.0f, 0.0f)));

                var skullsRenderer = skullsGo.GetComponent<ParticleSystemRenderer>();
                skullsRenderer.sharedMaterial = skullMat;
                skullsRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void CreateSlashWavePrefab(string path, Material slashMat, Material sparksMat, GameObject explosionPrefab, Mesh slashMesh, Color sparkColorStart, Color sparkColorEnd, string styleName, Texture2D skullTex, Material skullMat, Mesh coneMesh, Mesh funnelMesh)
        {
            GameObject go = new GameObject("VFX_Slash_Wave_Projectile");
            
            var proj = go.AddComponent<SlashWaveProjectile>();
            proj.speed = 22f;
            proj.lifetime = 1.2f;
            proj.explosionPrefab = explosionPrefab;

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0f, 2.5f);
            col.size = new Vector3(5f, 1f, 2f);

            // Dựng mô hình kiếm ý đặc trưng cho từng Hệ (Mục 2 của yêu cầu người dùng)
            GameObject modelGo = new GameObject("Projectile_Model");
            modelGo.transform.parent = go.transform;
            modelGo.transform.localPosition = Vector3.zero;

            var filter = modelGo.AddComponent<MeshFilter>();
            var renderer = modelGo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = slashMat;

            if (styleName == "ice" && coneMesh != null)
            {
                // Băng hệ: Phóng đinh băng nhọn hoắt hướng về phía trước
                filter.sharedMesh = coneMesh;
                modelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Mũi nhọn đâm tới (Z)
                modelGo.transform.localScale = new Vector3(0.5f, 0.5f, 3.5f);     // Kéo dài nhọn bén
            }
            else if (styleName == "wind" && funnelMesh != null)
            {
                // Phong hệ: Phóng vòi rồng lốc gió xoáy cuộn di chuyển thẳng tiến
                filter.sharedMesh = funnelMesh;
                // Bù trừ góc xoay Z = 90 độ của cha bằng cách xoay con Z = -90 độ để vòi rồng đứng thẳng
                modelGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                modelGo.transform.localScale = new Vector3(1.2f, 1.8f, 1.2f);
                
                // Thêm script xoay tự động quay tít khi di chuyển
                var scaleScript = modelGo.AddComponent<VfxScaleAndFade>();
                scaleScript.startScale = new Vector3(1.2f, 1.8f, 1.2f);
                scaleScript.endScale = new Vector3(1.2f, 1.8f, 1.2f);
                scaleScript.duration = proj.lifetime;
                scaleScript.rotationSpeed = new Vector3(0f, 1000f, 0f);
            }
            else if (styleName == "hell" && skullTex != null)
            {
                // Địa ngục hệ: Phóng đầu lâu khổng lồ đen thui lướt đi
                filter.sharedMesh = CreateQuadMesh();
                renderer.sharedMaterial = skullMat;
                // Bù trừ góc xoay Z = 90 độ của cha bằng cách xoay con Z = -90 độ để đầu lâu đứng thẳng
                modelGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                modelGo.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
            else
            {
                // Ma pháp, Hỏa, Lôi: Dùng vệt chém hình cung dựng đứng
                filter.sharedMesh = slashMesh;
                modelGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                modelGo.transform.localScale = Vector3.one;
            }

            // A. Hệ thống tia lửa đuôi mặc định (Trail Sparks)
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
            trailMain.gravityModifier = (styleName == "ice") ? 0.2f : 0.05f; 
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

            // B. Thêm hiệu ứng đặc trưng theo từng Hệ
            if (styleName == "fire")
            {
                GameObject smokeGo = new GameObject("Trail_Smoke");
                smokeGo.transform.parent = go.transform;
                smokeGo.transform.localPosition = new Vector3(0f, 0f, 1.2f);
                smokeGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 1f;
                smokeMain.loop = true;
                smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
                smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.45f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 25f;

                var smokeShape = smokePs.shape;
                smokeShape.shapeType = ParticleSystemShapeType.Sphere;
                smokeShape.radius = 0.4f;

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                Keyframe[] smokeKeys = new Keyframe[] { new Keyframe(0f, 0.8f), new Keyframe(0.2f, 1.0f), new Keyframe(1.0f, 1.6f) };
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(smokeKeys));

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.15f, 0.15f, 0.15f), 0f), new GradientColorKey(new Color(0.08f, 0.08f, 0.08f), 0.7f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.4f, 0.15f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = sparksMat;
            }
            else if (styleName == "ice")
            {
                var mainModule = trailPs.main;
                mainModule.gravityModifier = 0.2f;
                var rot = trailPs.rotationOverLifetime;
                rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(90f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);
                trailRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }
            else if (styleName == "wind")
            {
                GameObject windGo = new GameObject("Trail_Wind_Swirls");
                windGo.transform.parent = go.transform;
                windGo.transform.localPosition = new Vector3(0f, 0f, 1.0f);
                windGo.transform.localRotation = Quaternion.identity;

                var windPs = windGo.AddComponent<ParticleSystem>();
                var windMain = windPs.main;
                windMain.duration = 1f;
                windMain.loop = true;
                windMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
                windMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
                windMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
                windMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var windEmission = windPs.emission;
                windEmission.rateOverTime = 30f;

                var vel = windPs.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
                vel.orbitalY = new ParticleSystem.MinMaxCurve(4.0f, 7.0f);
                vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);

                var windColor = windPs.colorOverLifetime;
                windColor.enabled = true;
                Gradient windGrad = new Gradient();
                windGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.7f, 1f, 0.85f), 0f), new GradientColorKey(new Color(0.4f, 0.8f, 0.6f), 0.8f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(0.35f, 0.15f), new GradientAlphaKey(0f, 1.0f) }
                );
                windColor.color = windGrad;

                var windRenderer = windGo.GetComponent<ParticleSystemRenderer>();
                windRenderer.sharedMaterial = sparksMat;
                windRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }
            else if (styleName == "lightning")
            {
                var noise = trailPs.noise;
                noise.enabled = true;
                noise.strength = 1.8f;
                noise.frequency = 4.5f;
                noise.scrollSpeed = 2.0f;
            }
            else if (styleName == "hell")
            {
                GameObject skullGo = new GameObject("Trail_Skulls");
                skullGo.transform.parent = go.transform;
                skullGo.transform.localPosition = new Vector3(0f, 0.05f, 1.5f);
                skullGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                var skullPs = skullGo.AddComponent<ParticleSystem>();
                var skullMain = skullPs.main;
                skullMain.duration = 1f;
                skullMain.loop = true;
                skullMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.3f);
                skullMain.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 2.2f);
                skullMain.startSize = new ParticleSystem.MinMaxCurve(0.26f, 0.44f);
                skullMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var skullEmission = skullPs.emission;
                skullEmission.rateOverTime = 12f;

                var skullShape = skullPs.shape;
                skullShape.shapeType = ParticleSystemShapeType.Box;
                skullShape.scale = new Vector3(1.0f, 0.1f, 0.3f);

                var rot = skullPs.rotationOverLifetime;
                rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(-45f * Mathf.Deg2Rad, 45f * Mathf.Deg2Rad);

                var skullColor = skullPs.colorOverLifetime;
                skullColor.enabled = true;
                Gradient skullGrad = new Gradient();
                skullGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.9f, 0.9f, 0.9f), 0f), new GradientColorKey(new Color(0.2f, 0.0f, 0.3f), 0.8f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0f), new GradientAlphaKey(0.85f, 0.2f), new GradientAlphaKey(0.85f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
                skullColor.color = skullGrad;

                var skullSize = skullPs.sizeOverLifetime;
                skullSize.enabled = true;
                Keyframe[] skullSizeKeys = new Keyframe[] { new Keyframe(0f, 0.2f), new Keyframe(0.2f, 1.0f), new Keyframe(0.7f, 1.1f), new Keyframe(1.0f, 0.0f) };
                skullSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(skullSizeKeys));

                var skullRenderer = skullGo.GetComponent<ParticleSystemRenderer>();
                skullRenderer.sharedMaterial = skullMat;
                skullRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
