using UnityEngine;

namespace VfxSandbox
{
    public class VfxGroundCrackTear : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float scaleDuration = 0.12f;  // Thời gian xé toạc
        public float lifeDuration = 2.4f;    // Tổng thời gian tồn tại

        [Header("Color Settings (Void Cosmic)")]
        public Color hotColor = new Color(0f, 1.8f, 2.5f, 1.0f);   // Xanh lam điện phát sáng rực rỡ
        public Color cooledColor = new Color(0.12f, 0f, 0.22f, 0.4f); // Tím sẫm nguội lạnh
        public Color fadeColor = new Color(0.04f, 0.04f, 0.05f, 0.0f); // Tắt lịm

        private float elapsed = 0f;
        private Vector3 targetScale;
        private Material matInstance;
        private Renderer meshRenderer;

        private void Start()
        {
            targetScale = transform.localScale;
            transform.localScale = targetScale * 0.1f; // Bắt đầu từ 10% kích thước

            meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                matInstance = meshRenderer.material; // Nhân bản vật liệu để đổi màu cục bộ
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / lifeDuration);

            // 1. Hoạt ảnh xé rộng ra (Scale Animation)
            if (elapsed < scaleDuration)
            {
                float t = elapsed / scaleDuration;
                float easedT = t * (2f - t); // EaseOutQuad
                transform.localScale = Vector3.Lerp(targetScale * 0.1f, targetScale, easedT);
            }
            else
            {
                transform.localScale = targetScale;
            }

            // 2. Hoạt ảnh nguội màu sắc (Void Cyan -> Dark Purple -> Fade out)
            if (matInstance != null)
            {
                Color curColor;
                if (ratio < 0.5f)
                {
                    // Từ xanh lam điện rực sáng chuyển sang tím sẫm nguội dần
                    curColor = Color.Lerp(hotColor * 2f, cooledColor, ratio * 2f);
                }
                else
                {
                    // Từ tím sẫm tắt lịm dần
                    float t2 = (ratio - 0.5f) * 2f;
                    curColor = Color.Lerp(cooledColor, fadeColor, t2);
                }

                if (matInstance.HasProperty("_BaseColor"))
                {
                    matInstance.SetColor("_BaseColor", curColor);
                }
                else if (matInstance.HasProperty("_Color"))
                {
                    matInstance.SetColor("_Color", curColor);
                }
            }

            if (ratio >= 1.0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (matInstance != null)
            {
                Destroy(matInstance); // Tránh rò rỉ bộ nhớ
            }
        }
    }
}
