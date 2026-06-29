using UnityEngine;

namespace VfxSandbox
{
    public class SlashWaveTear : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float speed = 18f;
        public float lifetime = 1.6f;

        [Header("Ground Tear Settings")]
        public GameObject groundCrackPrefab;
        public GameObject dustImpactPrefab;
        public float spawnInterval = 0.05f;       // Khoảng thời gian giữa các lần sinh vết nứt
        public float minDistanceBetweenCracks = 0.7f; // Khoảng cách tối thiểu để tránh spawn chồng chéo

        [Header("Impact Settings")]
        public GameObject terminalExplosionPrefab;

        private float elapsed = 0f;
        private float lastSpawnTime = 0f;
        private Vector3 lastSpawnPosition;

        private void Start()
        {
            lastSpawnPosition = transform.position;
            // Hơi nghiêng nhẹ ngẫu nhiên để đường bay sinh động
            transform.Rotate(Vector3.forward * Random.Range(-4f, 4f), Space.Self);
            
            // Thực hiện quét đất ngay khi xuất hiện
            TearGround();
        }

        private void Update()
        {
            // Di chuyển kiếm khí khổng lồ tiến về phía trước
            transform.position += transform.forward * speed * Time.deltaTime;

            elapsed += Time.deltaTime;

            // Kiểm tra chu kỳ sinh vết rẽ đất
            if (elapsed - lastSpawnTime >= spawnInterval)
            {
                TearGround();
            }

            if (elapsed >= lifetime)
            {
                Explode();
            }
        }

        private void TearGround()
        {
            lastSpawnTime = elapsed;

            // Bắn tia Raycast xuống mặt đất để xác định cao độ nền
            Ray ray = new Ray(transform.position + Vector3.up * 1.5f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 4.0f))
            {
                // Kiểm tra xem đã bay đủ xa khỏi vị trí cũ chưa để tránh spawn dày đặc
                if (Vector3.Distance(hit.point, lastSpawnPosition) >= minDistanceBetweenCracks)
                {
                    lastSpawnPosition = hit.point;

                    // 1. Tạo vết nứt xé toạc đất rực sáng tím-xanh
                    if (groundCrackPrefab != null)
                    {
                        // Sinh vết nứt, áp đặt quay phẳng góc 90 độ đặt sát đất
                        Quaternion crackRot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
                        GameObject crackObj = Instantiate(groundCrackPrefab, hit.point + new Vector3(0f, 0.015f, 0f), crackRot);
                        
                        // Cung cấp tỉ lệ kích thước vết rẽ đất ngẫu nhiên
                        float size = Random.Range(1.8f, 2.5f);
                        crackObj.transform.localScale = new Vector3(size, size, 1f);
                    }

                    // 2. Tạo bụi đất phun trào và đá bay lên
                    if (dustImpactPrefab != null)
                    {
                        GameObject dustObj = Instantiate(dustImpactPrefab, hit.point + new Vector3(0f, 0.05f, 0f), Quaternion.identity);
                        Destroy(dustObj, 2.0f); // Tự hủy sau khi phun xong
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Đâm trúng vật cản sẽ nổ ngay
            Explode();
        }

        private void Explode()
        {
            if (terminalExplosionPrefab != null)
            {
                GameObject exp = Instantiate(terminalExplosionPrefab, transform.position, Quaternion.identity);
                Destroy(exp, 4f);
            }

            Destroy(gameObject);
        }
    }
}
