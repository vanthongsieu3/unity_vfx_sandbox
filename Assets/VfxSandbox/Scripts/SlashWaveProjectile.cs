using UnityEngine;

namespace VfxSandbox
{
    public class SlashWaveProjectile : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float speed = 22f;
        public float lifetime = 1.2f;

        [Header("Impact Effects")]
        public GameObject explosionPrefab;

        private float elapsed = 0f;

        private Material matInstance;

        private void Start()
        {
            // Nhân bản vật liệu và mở rộng tối đa biên độ Swipe để hiện đầy đủ thân kiếm khí hình bán nguyệt khi bay
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                matInstance = renderer.material;
                matInstance.SetFloat("_Swipe", 1.15f);
                matInstance.SetFloat("_TailLength", 1.25f);
                matInstance.SetFloat("_Opacity", 1.0f);
            }

            // Tự động xoay nhẹ hướng của kiếm khí để tạo cảm giác bay lượn ngẫu nhiên sinh động
            transform.Rotate(Vector3.forward * Random.Range(-5f, 5f), Space.Self);
        }

        private void OnDestroy()
        {
            if (matInstance != null)
            {
                Destroy(matInstance); // Dọn dẹp bộ nhớ vật liệu nhân bản
            }
        }

        private void Update()
        {
            // Di chuyển kiếm khí tiến thẳng về phía trước
            transform.position += transform.forward * speed * Time.deltaTime;

            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Kích hoạt vụ nổ khi va chạm với chướng ngại vật (nếu có collider)
            Explode();
        }

        private void Explode()
        {
            if (explosionPrefab != null)
            {
                // Tạo vụ nổ bùng lên tại điểm va chạm cuối
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(exp, 3f);
            }

            Destroy(gameObject);
        }
    }
}
