using System.IO;
using UnityEditor;
using UnityEngine;

namespace VfxSandbox.Editor
{
    public class VfxPrefabCreator
    {
        public static void GeneratePrefabs()
        {
            string prefabDir = "Assets/VfxSandbox/Prefabs";
            if (!Directory.Exists(prefabDir))
            {
                Directory.CreateDirectory(prefabDir);
            }

            // Load Materials
            Material lavaMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_lava_flow.mat");
            Material flameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_explosion_flame.mat");
            Material distMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_screen_distortion.mat");
            Material debrisMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_debris_rock.mat");
            Material fireRingMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_fire_ring.mat");
            Material smokeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_explosion_smoke.mat");
            Mesh debrisMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_rock_01.asset");
            Mesh ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_ring_01.asset");

            // Create Prefabs
            CreateTrailPrefab(prefabDir + "/vfx_prefab_trail.prefab", flameMat, smokeMat);
            CreateExplosionPrefab(prefabDir + "/vfx_prefab_explosion.prefab", flameMat, fireRingMat, smokeMat, ringMesh);
            CreateDebrisPrefab(prefabDir + "/vfx_prefab_debris.prefab", debrisMat != null ? debrisMat : lavaMat, debrisMesh);
            CreateShockwavePrefab(prefabDir + "/vfx_prefab_shockwave.prefab", distMat);
            CreateEmbersPrefab(prefabDir + "/vfx_prefab_embers.prefab", flameMat);

            AssetDatabase.Refresh();
            Debug.Log("✓ VFX Prefabs generated successfully in Assets/VfxSandbox/Prefabs");
        }

        private static void CreateTrailPrefab(string path, Material mat, Material smokeMat)
        {
            GameObject go = new GameObject("VFX_Trail_Particles");
            var ps = go.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = 0.6f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 45f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0.1f);
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.5f, 0f), 0f), new GradientColorKey(new Color(0.2f, 0f, 0f), 0.7f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1.0f) }
            );
            color.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // ─── TẠO ĐUÔI KHÓI ĐEN (DARK SMOKE TRAIL SUB-SYSTEM) ───
            if (smokeMat != null)
            {
                GameObject smokeGo = new GameObject("VFX_Trail_Dark_Smoke");
                smokeGo.transform.parent = go.transform;
                smokeGo.transform.localPosition = Vector3.zero;
                smokeGo.transform.localRotation = Quaternion.identity;

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 2f;
                smokeMain.loop = true;
                smokeMain.startLifetime = 1.0f; // Khói bay lâu hơn để tạo vệt
                smokeMain.startSpeed = 0.5f;
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;
                smokeMain.gravityModifier = -0.02f; // Khói nhẹ bay bốc lên trên

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 20f;

                var smokeShape = smokePs.shape;
                smokeShape.shapeType = ParticleSystemShapeType.Sphere;
                smokeShape.radius = 0.4f;

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                AnimationCurve smokeCurve = new AnimationCurve();
                smokeCurve.AddKey(0f, 0.4f);
                smokeCurve.AddKey(1f, 1.8f);
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                Color charcoal = new Color(0.12f, 0.12f, 0.12f, 1f);
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(charcoal, 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.4f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = smokeMat;
                smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateExplosionPrefab(string path, Material mat, Material fireRingMat, Material smokeMat, Mesh ringMesh)
        {
            GameObject go = new GameObject("VFX_Explosion_Particles");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 1.0f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 40) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.5f);
            curve.AddKey(0.2f, 1.2f);
            curve.AddKey(1f, 0f);
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.red, 0.4f), new GradientColorKey(Color.black, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.8f, 0.7f), new GradientAlphaKey(0f, 1.0f) }
            );
            color.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;

            // ─── TẠO VÒNG TRÒN LỬA LAN TỎA (FIRE RING 3D MESH OBJECT) ───
            if (ringMesh != null)
            {
                GameObject ringGo = new GameObject("VFX_Fire_Ring_Mesh");
                ringGo.transform.parent = go.transform;
                ringGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                ringGo.transform.localRotation = Quaternion.identity;

                var ringFilter = ringGo.AddComponent<MeshFilter>();
                ringFilter.sharedMesh = ringMesh;

                var ringRenderer = ringGo.AddComponent<MeshRenderer>();
                ringRenderer.sharedMaterial = fireRingMat != null ? fireRingMat : mat;

                // Thêm script tự động phóng to và mờ dần
                var scaleScript = ringGo.AddComponent<VfxScaleAndFade>();
                scaleScript.startScale = Vector3.one * 0.2f;
                scaleScript.endScale = Vector3.one * 7.5f;
                scaleScript.duration = 0.8f;
            }

            // ─── TẠO BÙNG NỔ KHÓI ĐEN (DARK SMOKE EXPLOSION SUB-SYSTEM) ───
            if (smokeMat != null)
            {
                GameObject smokeGo = new GameObject("VFX_Dark_Smoke");
                smokeGo.transform.parent = go.transform;
                smokeGo.transform.localPosition = Vector3.zero;
                smokeGo.transform.localRotation = Quaternion.identity;

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 1f;
                smokeMain.loop = false;
                smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.0f);
                smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f); // bay chậm tỏa rộng
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(2.0f, 3.5f); // khói to
                smokeMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;
                smokeMain.gravityModifier = -0.05f; // Khói bốc ngược lên trên

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 0f;
                smokeEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });

                var smokeShape = smokePs.shape;
                smokeShape.shapeType = ParticleSystemShapeType.Sphere;
                smokeShape.radius = 0.5f;

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                AnimationCurve smokeCurve = new AnimationCurve();
                smokeCurve.AddKey(0f, 0.5f);
                smokeCurve.AddKey(1f, 1.8f);
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                Color charcoal = new Color(0.12f, 0.12f, 0.12f, 1f);
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(charcoal, 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = smokeMat;
                smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateDebrisPrefab(string path, Material mat, Mesh mesh)
        {
            GameObject go = new GameObject("VFX_Debris_Particles");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = 1.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 22f); // Tăng tốc độ bắn văng
            
            // Kích hoạt kích thước 3D ngẫu nhiên giúp đá không đồng đều
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
            main.gravityModifier = 2.2f; // Trọng lực kéo đá rơi xuống nhanh hơn

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) }); // Tăng số lượng đá vụn lên 35 hạt

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f; // Tăng góc mở hình nón để bắn tỏa rộng ra xung quanh
            shape.radius = 0.3f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // Xoay hướng bắn lên trên (Y-axis)

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            // Xoay ngẫu nhiên cả 3 trục cho đá 3D sinh động
            rotation.x = new ParticleSystem.MinMaxCurve(-180f, 180f);
            rotation.y = new ParticleSystem.MinMaxCurve(-180f, 180f);
            rotation.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (mesh != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Mesh;
                renderer.mesh = mesh;
            }
            renderer.sharedMaterial = mat;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateShockwavePrefab(string path, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "VFX_Shockwave_Object";
            
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;

            // Thêm script tự phóng to và mờ dần
            var scaleScript = go.AddComponent<VfxScaleAndFade>();
            scaleScript.duration = 0.5f;
            scaleScript.startScale = Vector3.one * 0.1f;
            scaleScript.endScale = Vector3.one * 8.0f;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateEmbersPrefab(string path, Material mat)
        {
            GameObject go = new GameObject("VFX_Embers_Particles");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 3f;
            main.loop = true;
            main.startLifetime = 1.8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 25f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3f, 0.1f, 3f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.8f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        [MenuItem("Window/VFX/Upgrade Prefabs with Black Smoke (Non-Destructive)")]
        public static void UpgradePrefabsMenu()
        {
            UpgradeExistingPrefabs();
            Debug.Log("✓ Successfully upgraded existing prefabs with black smoke non-destructively!");
            EditorUtility.DisplayDialog("VFX Upgrade", "Đã nâng cấp khói đen thành công vào các Prefab hiện tại!\n\nCác chỉnh sửa tùy biến thủ công của anh đã được giữ lại trọn vẹn.", "OK");
        }

        public static void UpgradeExistingPrefabs()
        {
            string prefabDir = "Assets/VfxSandbox/Prefabs";
            Material smokeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VfxSandbox/Materials/mat_explosion_smoke.mat");
            if (smokeMat == null)
            {
                Debug.LogWarning("Không tìm thấy mat_explosion_smoke.mat! Hãy tạo lại vật liệu khói trước.");
                return;
            }
            
            // 1. Upgrade Trail Prefab
            string trailPath = prefabDir + "/vfx_prefab_trail.prefab";
            if (File.Exists(trailPath))
            {
                UpgradeTrailPrefab(trailPath, smokeMat);
            }

            // 2. Upgrade Explosion Prefab
            string explosionPath = prefabDir + "/vfx_prefab_explosion.prefab";
            if (File.Exists(explosionPath))
            {
                UpgradeExplosionPrefab(explosionPath, smokeMat);
            }
            
            AssetDatabase.Refresh();
        }

        private static void UpgradeTrailPrefab(string path, Material smokeMat)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            
            // Kiểm tra xem VFX_Trail_Dark_Smoke đã có chưa
            Transform existing = root.transform.Find("VFX_Trail_Dark_Smoke");
            if (existing == null)
            {
                GameObject smokeGo = new GameObject("VFX_Trail_Dark_Smoke");
                smokeGo.transform.parent = root.transform;
                smokeGo.transform.localPosition = Vector3.zero;
                smokeGo.transform.localRotation = Quaternion.identity;

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 2f;
                smokeMain.loop = true;
                smokeMain.startLifetime = 1.0f; // Khói bay lâu để tạo vệt
                smokeMain.startSpeed = 0.5f;
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;
                smokeMain.gravityModifier = -0.02f; // Khói nhẹ bốc lên

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 20f;

                var smokeShape = smokePs.shape;
                smokeShape.shapeType = ParticleSystemShapeType.Sphere;
                smokeShape.radius = 0.4f;

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                AnimationCurve smokeCurve = new AnimationCurve();
                smokeCurve.AddKey(0f, 0.4f);
                smokeCurve.AddKey(1f, 1.8f);
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                Color charcoal = new Color(0.12f, 0.12f, 0.12f, 1f);
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(charcoal, 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.4f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = smokeMat;
                smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void UpgradeExplosionPrefab(string path, Material smokeMat)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            
            // Kiểm tra xem VFX_Dark_Smoke đã có chưa
            Transform existing = root.transform.Find("VFX_Dark_Smoke");
            if (existing == null)
            {
                GameObject smokeGo = new GameObject("VFX_Dark_Smoke");
                smokeGo.transform.parent = root.transform;
                smokeGo.transform.localPosition = Vector3.zero;
                smokeGo.transform.localRotation = Quaternion.identity;

                var smokePs = smokeGo.AddComponent<ParticleSystem>();
                var smokeMain = smokePs.main;
                smokeMain.duration = 1f;
                smokeMain.loop = false;
                smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.0f);
                smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f); // bay chậm tỏa rộng
                smokeMain.startSize = new ParticleSystem.MinMaxCurve(2.0f, 3.5f); // khói to
                smokeMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
                smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;
                smokeMain.gravityModifier = -0.05f; // Khói bốc ngược lên trên

                var smokeEmission = smokePs.emission;
                smokeEmission.rateOverTime = 0f;
                smokeEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });

                var smokeShape = smokePs.shape;
                smokeShape.shapeType = ParticleSystemShapeType.Sphere;
                smokeShape.radius = 0.5f;

                var smokeSize = smokePs.sizeOverLifetime;
                smokeSize.enabled = true;
                AnimationCurve smokeCurve = new AnimationCurve();
                smokeCurve.AddKey(0f, 0.5f);
                smokeCurve.AddKey(1f, 1.8f);
                smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

                var smokeColor = smokePs.colorOverLifetime;
                smokeColor.enabled = true;
                Gradient smokeGrad = new Gradient();
                Color charcoal = new Color(0.12f, 0.12f, 0.12f, 1f);
                smokeGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(charcoal, 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.4f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
                );
                smokeColor.color = smokeGrad;

                var smokeRenderer = smokeGo.GetComponent<ParticleSystemRenderer>();
                smokeRenderer.sharedMaterial = smokeMat;
                smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
