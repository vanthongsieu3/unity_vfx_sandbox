using UnityEngine;

namespace VfxSandbox
{
    public class VfxSlashAnimator : MonoBehaviour
    {
        public float swipeDuration = 0.11f; // Thời gian vung kiếm quét cung chém (nhanh gấp đôi để tăng lực)
        public float fadeDuration = 0.14f;  // Thời gian mờ dần và tan biến
        private float elapsed = 0f;
        private Material matInstance;
        private Transform sparksTrans;

        // Cấu trúc hình học của cung chém để đồng bộ vị trí phát tia lửa (Sparks) ở mũi chém
        private float arcAngleRad = 140f * Mathf.Deg2Rad;
        private float outerRadius = 3.2f;

        private void Start()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                matInstance = renderer.material; // Tạo bản sao vật liệu để chỉnh sửa cục bộ không bị tràn
                matInstance.SetFloat("_Swipe", 0f);
                matInstance.SetFloat("_Opacity", 1f);
            }

            // Tìm đối tượng con chứa tia lửa
            if (transform.parent != null)
            {
                sparksTrans = transform.parent.Find("Slash_Sparks");
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
                    
                    float swipeProgress = Mathf.Lerp(0f, 1.15f, easedSwipe);
                    matInstance.SetFloat("_Swipe", swipeProgress);

                    // Đồng bộ di chuyển bộ phát tia lửa ở mũi chém dọc theo viền ngoài của cung chém
                    if (sparksTrans != null)
                    {
                        float currentAngle = -arcAngleRad * 0.5f + Mathf.Clamp01(swipeProgress) * arcAngleRad;
                        float x = Mathf.Sin(currentAngle) * outerRadius;
                        float z = Mathf.Cos(currentAngle) * outerRadius;
                        sparksTrans.localPosition = new Vector3(x, 0.02f, z);
                    }
                }
                else
                {
                    matInstance.SetFloat("_Swipe", 1.5f); // Quét sạch hoàn toàn

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
