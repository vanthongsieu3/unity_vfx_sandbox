using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VfxSandbox
{
    public class MeteorVfxController : MonoBehaviour
    {
        [Header("VFX Assets")]
        public Material meteorMaterial;
        public Material trailSmokeMaterial;
        public Material sparkMaterial;
        public Material explosionMaterial;
        public Material shockwaveMaterial;
        public Material groundCracksMaterial;
        public Material emberMaterial;
        public Material magicCircleMaterial; // Vật liệu vòng tròn ma pháp chuyên biệt
        public Mesh meteorMesh;
        public Mesh debrisMesh;
        public Mesh funnelMesh;

        [Header("VFX Prefabs (Gán Prefabs có sẵn)")]
        public GameObject trailPrefab;
        public GameObject explosionPrefab;
        public GameObject debrisPrefab;
        public GameObject shockwavePrefab;
        public GameObject embersPrefab;

        [Header("VFX Parameters")]
        public float nạpThờiGian = 1.2f;
        public float tốcĐộRơi = 35f;
        public float sátThươngBánKính = 6f;
        public float lựcRungCamera = 0.5f;
        public float thờiGianRung = 0.4f;

        [Header("Debug Controls")]
        [Tooltip("Nhấn phím Space hoặc click checkbox này trong Editor để test chiêu thức")]
        public bool triggerTest = false;

        private Camera mainCamera;
        private Vector3 originalCameraPos;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPos = mainCamera.transform.position;
            }
        }

        private void Update()
        {
            if (triggerTest || Input.GetKeyDown(KeyCode.Space))
            {
                triggerTest = false;
                StartCoroutine(ExecuteMeteorStrike(transform.position));
            }
        }

        public void TriggerStrike(Vector3 targetPos)
        {
            StartCoroutine(ExecuteMeteorStrike(targetPos));
        }

        private IEnumerator ExecuteMeteorStrike(Vector3 targetPos)
        {
            Debug.Log($"[MeteorVfx] Giai đoạn 1: Khởi động nạp lực ({nạpThờiGian}s). Chỉ thị vùng đánh.");
            
            // 1. Tạo Magic Circle Indicator
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(indicator.GetComponent<Collider>());
            indicator.name = "VFX_Indicator_Circle";
            indicator.transform.position = targetPos + new Vector3(0, 0.05f, 0);
            indicator.transform.rotation = Quaternion.Euler(90, 0, 0);
            indicator.transform.localScale = Vector3.zero;
            
            var indicatorRenderer = indicator.GetComponent<Renderer>();
            indicatorRenderer.sharedMaterial = magicCircleMaterial != null ? magicCircleMaterial : explosionMaterial;

            // Animate indicator scale (từ bé nở ra to)
            float t = 0;
            while (t < nạpThờiGian)
            {
                t += Time.deltaTime;
                float ratio = t / nạpThờiGian;
                indicator.transform.localScale = Vector3.one * Mathf.Lerp(0f, 6.0f, ratio);
                
                // Nhấp nháy độ sáng lúc thiên thạch sắp chạm đất (Hỗ trợ cả URP _BaseColor và _Color)
                var mat = indicatorRenderer.material;
                if (mat.HasProperty("_BaseColor"))
                {
                    Color col = Color.Lerp(new Color(1f, 0.3f, 0f, 0f), new Color(1f, 0.5f, 0f, 1f), ratio);
                    if (ratio > 0.8f) col *= 2.0f; // rực sáng lên
                    mat.SetColor("_BaseColor", col);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color col = Color.Lerp(new Color(1f, 0.3f, 0f, 0f), new Color(1f, 0.5f, 0f, 1f), ratio);
                    if (ratio > 0.8f) col *= 2.0f; // rực sáng lên
                    mat.SetColor("_Color", col);
                }
                yield return null;
            }

            Destroy(indicator);

            // 2. Giai đoạn 2: Thiên thạch lao xuống
            Debug.Log("[MeteorVfx] Giai đoạn 2: Phát lực - Thiên thạch rơi từ trên cao.");
            Vector3 startPos = targetPos + new Vector3(-15, 25, 0); // rơi chéo góc 45 độ
            GameObject meteor = new GameObject("VFX_Meteor");
            meteor.transform.position = startPos;
            
            var meshFilter = meteor.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = meteorMesh != null ? meteorMesh : Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            var meshRenderer = meteor.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = meteorMaterial;

            // Đuôi khói & lửa (Trail Particle) từ Prefab
            if (trailPrefab != null)
            {
                GameObject trailObj = Instantiate(trailPrefab, meteor.transform);
                trailObj.transform.localPosition = Vector3.zero;
                trailObj.transform.localRotation = Quaternion.identity;
            }

            // Di chuyển thiên thạch tới mục tiêu
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / tốcĐộRơi;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                meteor.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                
                // Xoay nhẹ thiên thạch trên đường bay
                meteor.transform.Rotate(Vector3.up * 180f * Time.deltaTime, Space.Self);
                yield return null;
            }

            // Va chạm đất: Hủy thiên thạch chính, kích hoạt nổ
            Vector3 impactPos = targetPos;
            Destroy(meteor);

            // 3. Giai đoạn 3: Va chạm chấn động & Sóng nổ (Impact / Explode)
            Debug.Log("[MeteorVfx] Giai đoạn 3: Thu chiêu - Va chạm bùng nổ, tạo sóng xung kích.");
            TriggerImpactEffects(impactPos);

            // Camera Shake
            if (mainCamera != null)
            {
                StartCoroutine(CameraShakeEffect());
            }
        }

        private void TriggerImpactEffects(Vector3 pos)
        {
            // A. Explosion (Hiệu ứng lửa bùng nổ) từ Prefab
            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, pos, Quaternion.identity);
                Destroy(exp, 4f);
            }

            // B. Debris (Đá vỡ văng tung tóe) từ Prefab
            if (debrisPrefab != null)
            {
                GameObject deb = Instantiate(debrisPrefab, pos, Quaternion.identity);
                Destroy(deb, 3f);
            }

            // C. Shockwave (Sóng xung kích distortion) từ Prefab
            if (shockwavePrefab != null)
            {
                GameObject sw = Instantiate(shockwavePrefab, pos + new Vector3(0, 0.05f, 0), Quaternion.Euler(90, 0, 0));
                Destroy(sw, 1.5f);
            }

            // D. Embers (Tàn lửa bốc lên sau nổ) từ Prefab
            if (embersPrefab != null)
            {
                GameObject emb = Instantiate(embersPrefab, pos, Quaternion.identity);
                Destroy(emb, 6f);
            }

            // E. Ground Cracks Decal
            GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(crack.GetComponent<Collider>());
            crack.name = "VFX_Impact_Cracks";
            crack.transform.position = pos + new Vector3(0, 0.02f, 0);
            crack.transform.rotation = Quaternion.Euler(90, 0, 0);
            crack.transform.localScale = Vector3.one * 0.5f; // Bắt đầu nhỏ để xé rộng ra ngoài
            var crackRenderer = crack.GetComponent<Renderer>();
            crackRenderer.material = groundCracksMaterial;
            StartCoroutine(FadeOutCracks(crackRenderer, 4.5f, 9.5f));
        }

        private IEnumerator FadeOutCracks(Renderer r, float dur, float maxScale)
        {
            float elapsed = 0;
            Vector3 startScale = Vector3.one * (maxScale * 0.15f);
            Vector3 targetScale = Vector3.one * maxScale;
            float propagationDuration = 0.25f; // Thời gian vết nứt xé toạc ra (0.25 giây)

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / dur;

                if (r != null)
                {
                    // 1. Hiệu ứng xé nứt lan rộng từ trong ra ngoài (Scale Animation)
                    if (elapsed < propagationDuration)
                    {
                        float scaleT = elapsed / propagationDuration;
                        float easedT = scaleT * (2f - scaleT); // EaseOutQuad giúp xé nứt nhanh lúc đầu
                        r.transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
                    }
                    else
                    {
                        r.transform.localScale = targetScale;
                    }

                    // 2. Hiệu ứng nguội dung nham đỏ rực -> xám xịt -> mờ dần hẳn (Color Animation)
                    if (r.material != null)
                    {
                        Color hotColor = Color.Lerp(Color.red * 2.5f, new Color(0.2f, 0.05f, 0f, 0.8f), ratio);
                        if (ratio > 0.6f)
                        {
                            float fadeOutT = (ratio - 0.6f) / 0.4f;
                            hotColor = Color.Lerp(new Color(0.2f, 0.05f, 0f, 0.8f), new Color(0.05f, 0.05f, 0.05f, 0.0f), fadeOutT);
                        }

                        if (r.material.HasProperty("_Color"))
                        {
                            r.material.color = hotColor;
                        }
                        else if (r.material.HasProperty("_BaseColor"))
                        {
                            r.material.SetColor("_BaseColor", hotColor);
                        }
                    }
                }
                yield return null;
            }
            if (r != null) Destroy(r.gameObject);
        }

        private IEnumerator CameraShakeEffect()
        {
            float elapsed = 0f;
            while (elapsed < thờiGianRung)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / thờiGianRung;
                float damper = 1.0f - percent;
                
                float rx = Random.Range(-1f, 1f) * lựcRungCamera * damper;
                float ry = Random.Range(-1f, 1f) * lựcRungCamera * damper;
                
                mainCamera.transform.position = originalCameraPos + new Vector3(rx, ry, 0);
                yield return null;
            }
            mainCamera.transform.position = originalCameraPos;
        }
    }
}
