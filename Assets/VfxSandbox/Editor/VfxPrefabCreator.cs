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
            Mesh debrisMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_rock_01.asset");
            Mesh ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/VfxSandbox/Meshes/vfx_mesh_ring_01.asset");

            // Create Prefabs
            CreateTrailPrefab(prefabDir + "/vfx_prefab_trail.prefab", flameMat);
            CreateExplosionPrefab(prefabDir + "/vfx_prefab_explosion.prefab", flameMat, fireRingMat, ringMesh);
            CreateDebrisPrefab(prefabDir + "/vfx_prefab_debris.prefab", debrisMat != null ? debrisMat : lavaMat, debrisMesh);
            CreateShockwavePrefab(prefabDir + "/vfx_prefab_shockwave.prefab", distMat);
            CreateEmbersPrefab(prefabDir + "/vfx_prefab_embers.prefab", flameMat);

            AssetDatabase.Refresh();
            Debug.Log("✓ VFX Prefabs generated successfully in Assets/VfxSandbox/Prefabs");
        }

        private static void CreateTrailPrefab(string path, Material mat)
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

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateExplosionPrefab(string path, Material mat, Material fireRingMat, Mesh ringMesh)
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
                scaleScript.startScale = 0.2f;
                scaleScript.endScale = 7.5f;
                scaleScript.duration = 0.8f;
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
            main.startLifetime = 1.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 18f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.gravityModifier = 1.8f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.4f;

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(90f, 360f);

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
    }
}
