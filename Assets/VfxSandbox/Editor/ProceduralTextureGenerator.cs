using System.IO;
using UnityEditor;
using UnityEngine;

namespace VfxSandbox.Editor
{
    public class ProceduralTextureGenerator : EditorWindow
    {
        [MenuItem("Window/VFX/Generate Procedural Textures")]
        public static void Generate()
        {
            string dir = "Assets/VfxSandbox/Textures";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            GenerateNoiseTexture(dir + "/vfx_tex_noise_01.png", 256);
            GenerateEmberTexture(dir + "/vfx_tex_ember_01.png", 256);
            GenerateRampTexture(dir + "/vfx_tex_ramp_01.png", 256);
            GenerateRampVoidTexture(dir + "/vfx_tex_ramp_void.png", 256); // Sinh màu chuyển Void/Kassadin
            GenerateRockTexture(dir + "/vfx_tex_rock_01.png", 256);

            AssetDatabase.Refresh();
            Debug.Log("✓ Procedural textures generated successfully in Assets/VfxSandbox/Textures");
        }

        private static void GenerateNoiseTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float scale = 4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Seamless tiling noise using 4D coordinates
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float nx = Mathf.Cos(u * Mathf.PI * 2) * scale / (Mathf.PI * 2);
                    float ny = Mathf.Sin(u * Mathf.PI * 2) * scale / (Mathf.PI * 2);
                    float nz = Mathf.Cos(v * Mathf.PI * 2) * scale / (Mathf.PI * 2);
                    float nw = Mathf.Sin(v * Mathf.PI * 2) * scale / (Mathf.PI * 2);

                    float n1 = Mathf.PerlinNoise(nx + 100, ny + 100);
                    float n2 = Mathf.PerlinNoise(nz + 200, nw + 200);
                    float val = (n1 + n2) * 0.5f;

                    // Increase contrast slightly
                    val = Mathf.Clamp01((val - 0.2f) * 1.5f);

                    tex.SetPixel(x, y, new Color(val, val, val, 1.0f));
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }

        private static void GenerateEmberTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float val = Mathf.Clamp01(1.0f - (dist / maxDist));
                    
                    // Smooth quadratic falloff
                    val = val * val;

                    tex.SetPixel(x, y, new Color(1, 1, 1, val));
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }

        private static void GenerateRampTexture(string path, int size)
        {
            var tex = new Texture2D(size, 1, TextureFormat.RGBA32, false);
            
            // Fire ramp: Black -> Red -> Orange -> Yellow -> White
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.black, 0.0f),
                    new GradientColorKey(new Color(0.8f, 0.0f, 0.0f), 0.25f),
                    new GradientColorKey(new Color(1.0f, 0.4f, 0.0f), 0.5f),
                    new GradientColorKey(new Color(1.0f, 0.95f, 0.1f), 0.75f),
                    new GradientColorKey(Color.white, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );

            for (int x = 0; x < size; x++)
            {
                float t = (float)x / (size - 1);
                tex.SetPixel(x, 0, grad.Evaluate(t));
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }

        private static void GenerateRampVoidTexture(string path, int size)
        {
            var tex = new Texture2D(size, 1, TextureFormat.RGBA32, false);
            
            // Void/Kassadin ramp: Black -> Dark Purple -> Magenta -> Electric Cyan -> White-Cyan
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.black, 0.0f),
                    new GradientColorKey(new Color(0.12f, 0.0f, 0.28f), 0.22f), // Tím sẫm hư vô
                    new GradientColorKey(new Color(0.85f, 0.0f, 0.65f), 0.52f), // Hồng tím rực (Magenta)
                    new GradientColorKey(new Color(0.0f, 0.85f, 1.0f), 0.82f),  // Xanh lam điện (Electric Cyan)
                    new GradientColorKey(new Color(0.85f, 1.0f, 1.0f), 1.0f)    // Lõi trắng cyan siêu sáng
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );

            for (int x = 0; x < size; x++)
            {
                float t = (float)x / (size - 1);
                tex.SetPixel(x, 0, grad.Evaluate(t));
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }

        private static void GenerateRockTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float scale = 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    
                    // Simple Perlin Noise for rock bump/detail
                    float n = Mathf.PerlinNoise(u * scale, v * scale);
                    
                    // Volcanic rock coloring (dark charcoal grey with slight variations)
                    float colVal = Mathf.Lerp(0.08f, 0.22f, n);
                    tex.SetPixel(x, y, new Color(colVal, colVal, colVal, 1.0f));
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }
    }
}
