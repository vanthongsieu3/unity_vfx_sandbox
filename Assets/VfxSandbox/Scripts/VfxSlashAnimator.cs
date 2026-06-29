using UnityEngine;

namespace VfxSandbox
{
    public class VfxSlashAnimator : MonoBehaviour
    {
        public float swipeDuration = 0.22f; // Thời gian vung kiếm quét cung chém
        public float fadeDuration = 0.18f;  // Thời gian mờ dần và tan biến
        private float elapsed = 0f;
        private Material matInstance;

        private void Start()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                matInstance = renderer.material; // Tạo bản sao vật liệu để chỉnh sửa cục bộ không bị tràn
                matInstance.SetFloat("_Swipe", 0f);
                matInstance.SetFloat("_Opacity", 1f);
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            if (matInstance != null)
            {
                // Giai đoạn 1: Quét chém (Swipe)
                if (elapsed < swipeDuration)
                {
                    float swipeRatio = elapsed / swipeDuration;
                    float easedSwipe = swipeRatio * (2f - swipeRatio); // EaseOutQuad cho cảm giác vung mạnh lúc đầu
                    matInstance.SetFloat("_Swipe", Mathf.Lerp(0f, 1.1f, easedSwipe));
                }
                else
                {
                    matInstance.SetFloat("_Swipe", 1.1f);

                    // Giai đoạn 2: Phai nhòa (Fade Out)
                    float fadeElapsed = elapsed - swipeDuration;
                    float fadeRatio = fadeElapsed / fadeDuration;

                    if (fadeRatio >= 1f)
                    {
                        // Hủy đối tượng cha chứa toàn bộ VFX
                        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
                        return;
                    }

                    matInstance.SetFloat("_Opacity", Mathf.Clamp01(1f - fadeRatio));
                }
            }
            else
            {
                if (elapsed >= (swipeDuration + fadeDuration))
                {
                    Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (matInstance != null)
            {
                Destroy(matInstance); // Dọn dẹp bộ nhớ bản sao vật liệu
            }
        }
    }
}
