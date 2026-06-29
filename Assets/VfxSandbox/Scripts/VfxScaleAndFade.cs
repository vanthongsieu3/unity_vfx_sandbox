using UnityEngine;

namespace VfxSandbox
{
    public class VfxScaleAndFade : MonoBehaviour
    {
        public float duration = 0.5f;
        public Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 endScale = new Vector3(8.0f, 8.0f, 8.0f);
        private float elapsed = 0f;
        private Material matInstance;

        private void Start()
        {
            transform.localScale = startScale;
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                matInstance = renderer.material; // Tạo bản sao vật liệu để chỉnh sửa cục bộ
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (matInstance != null)
            {
                float fadeRatio = Mathf.Clamp01(1f - t);

                // 1. Phai nhòa _Opacity (cho custom FireRing shader)
                if (matInstance.HasProperty("_Opacity"))
                {
                    matInstance.SetFloat("_Opacity", fadeRatio);
                }

                // 2. Phai nhòa _ColorTint (cho ScreenDistortion shader)
                if (matInstance.HasProperty("_ColorTint"))
                {
                    Color col = matInstance.GetColor("_ColorTint");
                    col.a = fadeRatio;
                    matInstance.SetColor("_ColorTint", col);
                }

                // 3. Phai nhòa _BaseColor (cho URP Standard shaders)
                if (matInstance.HasProperty("_BaseColor"))
                {
                    Color col = matInstance.GetColor("_BaseColor");
                    col.a = fadeRatio;
                    matInstance.SetColor("_BaseColor", col);
                }

                // 4. Phai nhòa _Color (cho Legacy shaders)
                if (matInstance.HasProperty("_Color"))
                {
                    Color col = matInstance.GetColor("_Color");
                    col.a = fadeRatio;
                    matInstance.SetColor("_Color", col);
                }
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (matInstance != null)
            {
                Destroy(matInstance); // Dọn dẹp bản sao vật liệu để chống tràn bộ nhớ
            }
        }
    }
}
