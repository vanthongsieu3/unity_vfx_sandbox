using UnityEngine;

namespace VfxSandbox
{
    public class VfxScaleAndFade : MonoBehaviour
    {
        public float duration = 0.5f;
        public Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 endScale = new Vector3(8.0f, 8.0f, 8.0f);
        private float elapsed = 0f;

        private void Start()
        {
            transform.localScale = startScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
