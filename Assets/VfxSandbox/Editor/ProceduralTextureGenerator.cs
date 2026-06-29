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
            GenerateStarTexture(dir + "/vfx_tex_star_01.png", 256); // Sinh hình ngôi sao 4 cánh rực sáng
            GenerateRampTexture(dir + "/vfx_tex_ramp_01.png", 256);
            GenerateRampVoidTexture(dir + "/vfx_tex_ramp_void.png", 256); // Sinh màu chuyển Void/Kassadin
            GenerateRockTexture(dir + "/vfx_tex_rock_01.png", 256);
            GenerateWaterNormal(dir + "/vfx_tex_water_normal.png", 256); // Sinh map pháp tuyến nước cuộn sóng mịn màng
            GenerateWaterCaustics(dir + "/vfx_tex_water_caustics.png", 256); // Sinh map vân caustics (gợn nắng tròn Voronoi) sắc sảo
            GenerateBubbleTexture(dir + "/vfx_tex_bubble_01.png", 256); // Sinh sprite bọt khí tròn long lanh có highlight

            AssetDatabase.Refresh();
            Debug.Log("✓ Procedural textures generated successfully in Assets/VfxSandbox/Textures");
        }

        private static void GenerateBubbleTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float center = size * 0.5f;
            float radius = size * 0.44f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    float color = 0.0f;
                    float alpha = 0.0f;
                    
                    if (d <= radius)
                    {
                        // Viền bong bóng sắc nét ở rìa
                        float edgeWidth = size * 0.035f;
                        if (d > radius - edgeWidth)
                        {
                            float factor = (d - (radius - edgeWidth)) / edgeWidth;
                            alpha = Mathf.Lerp(0.2f, 0.95f, factor);
                            color = 1.0f;
                        }
                        else
                        {
                            // Lòng bong bóng trong suốt nhẹ
                            alpha = 0.05f;
                            color = 0.85f;
                        }
                        
                        // Điểm lấp lánh (Highlight glint) ở góc trên bên trái
                        float hdx = x - (center - radius * 0.35f);
                        float hdy = y - (center - radius * 0.35f); // Sửa dấu để glint ở góc trên trái
                        float hd = Mathf.Sqrt(hdx * hdx + hdy * hdy);
                        if (hd < size * 0.07f)
                        {
                            float hFactor = 1.0f - (hd / (size * 0.07f));
                            alpha = Mathf.Max(alpha, hFactor * 0.92f);
                            color = 1.0f;
                        }
                    }
                    
                    tex.SetPixel(x, y, new Color(color, color, color, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            DestroyImmediate(tex);
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

        private static void GenerateStarTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center.x) / (size / 2f);
                    float dy = Mathf.Abs(y - center.y) / (size / 2f);
                    
                    // Phương trình vẽ hình ngôi sao 4 cánh nhọn hoắt (pinch axes)
                    float valX = Mathf.Max(0f, 1f - dx * 8f) * Mathf.Max(0f, 1f - dy * 1.5f);
                    float valY = Mathf.Max(0f, 1f - dy * 8f) * Mathf.Max(0f, 1f - dx * 1.5f);
                    
                    // Quầng sáng mềm ở trung tâm ngôi sao
                    float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                    float glow = Mathf.Max(0f, 1f - dist * 4f);
                    
                    float finalVal = Mathf.Clamp01(valX + valY + glow);
                    finalVal = finalVal * finalVal; // Tăng độ tương phản mịn
                    
                    tex.SetPixel(x, y, new Color(1, 1, 1, finalVal));
                }
            }
            
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
        }

        private static void GenerateWaterNormal(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true); // Linear texture for normal maps
            float du = 1.0f / size;
            float dv = 1.0f / size;

            // Hàm tính độ cao sóng tuần hoàn để triệt tiêu viền ghép nối (100% seamless)
            float GetWaveHeight(float u, float v)
            {
                float uAngle = u * Mathf.PI * 2.0f;
                float vAngle = v * Mathf.PI * 2.0f;

                // Kết hợp nhiều tần số sóng khác nhau
                float h = Mathf.Sin(uAngle * 2.0f + vAngle * 1.0f) * 0.35f +
                          Mathf.Cos(uAngle * 1.0f - vAngle * 3.0f) * 0.25f +
                          Mathf.Sin(uAngle * 4.0f + vAngle * 2.0f) * 0.18f +
                          Mathf.Cos(uAngle * 3.0f - vAngle * 5.0f) * 0.12f +
                          Mathf.Sin(uAngle * 6.0f) * 0.06f +
                          Mathf.Cos(vAngle * 6.0f) * 0.04f;
                return h;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // Sử dụng phương pháp vi phân hữu hạn Sobel/Finite-difference tính Vector pháp tuyến
                    float h_l = GetWaveHeight(u - du, v);
                    float h_r = GetWaveHeight(u + du, v);
                    float h_d = GetWaveHeight(u, v - dv);
                    float h_u = GetWaveHeight(u, v + dv);

                    float nx = (h_l - h_r) * 1.5f; // Độ gợn sóng mạnh/nhẹ
                    float ny = (h_d - h_u) * 1.5f;
                    float nz = 1.0f;

                    Vector3 normal = new Vector3(nx, ny, nz).normalized;

                    // Mã hóa véc-tơ pháp tuyến từ dải [-1, 1] sang dải màu [0, 1] cho GPU đọc
                    float r = normal.x * 0.5f + 0.5f;
                    float g = normal.y * 0.5f + 0.5f;
                    float b = normal.z * 0.5f + 0.5f;

                    tex.SetPixel(x, y, new Color(r, g, b, 1.0f));
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            DestroyImmediate(tex);
        }

        private static void GenerateWaterCaustics(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            int gridCount = 8;
            Vector2[] points = new Vector2[gridCount * gridCount];
            
            // Khởi tạo các điểm ngẫu nhiên trong lưới
            Random.InitState(42);
            for (int cy = 0; cy < gridCount; cy++)
            {
                for (int cx = 0; cx < gridCount; cx++)
                {
                    float ox = Random.value;
                    float oy = Random.value;
                    points[cy * gridCount + cx] = new Vector2(cx + ox, cy + oy);
                }
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    
                    float px = u * gridCount;
                    float py = v * gridCount;
                    
                    float minDist = 100f;
                    
                    // Duyệt các ô lân cận để tìm khoảng cách nhỏ nhất (tính toán bao quanh cho Seamless)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int gcx = Mathf.FloorToInt(px) + dx;
                            int gcy = Mathf.FloorToInt(py) + dy;
                            
                            // Bao quanh tuần hoàn (Wrap around for tiling)
                            int wrapX = (gcx + gridCount) % gridCount;
                            int wrapY = (gcy + gridCount) % gridCount;
                            
                            Vector2 pt = points[wrapY * gridCount + wrapX];
                            
                            // Tính khoảng cách có tính đến việc bao quanh lưới
                            float diffX = px - (gcx + (pt.x - wrapX));
                            float diffY = py - (gcy + (pt.y - wrapY));
                            float dist = Mathf.Sqrt(diffX * diffX + diffY * diffY);
                            
                            if (dist < minDist)
                            {
                                minDist = dist;
                            }
                        }
                    }
                    
                    // Tạo viền sắc nét giống caustics (gợn nắng nước) bằng hàm pow
                    float val = Mathf.Clamp01(1.0f - minDist);
                    float caustics = Mathf.Pow(val, 4.0f); // Tăng độ sắc sảo cho vân nắng
                    
                    tex.SetPixel(x, y, new Color(caustics, caustics, caustics, 1.0f));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            DestroyImmediate(tex);
        }
    }
}
