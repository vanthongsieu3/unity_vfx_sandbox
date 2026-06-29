using UnityEngine;

namespace VfxSandbox
{
    public class BoatFloating : MonoBehaviour
    {
        [Header("Wave Parameters (Must match Shader)")]
        public float waveHeight = 0.22f;
        public float waveScale = 0.85f;
        public float waveSpeed = 1.6f;
        public Vector2 waveDirection = new Vector2(0f, -1f);

        [Header("Obstacle Ripple Parameters (Must match Shader)")]
        public Vector2 pillar1Pos = new Vector2(1.2f, 1.5f);
        public Vector2 pillar2Pos = new Vector2(-1.8f, 3.2f);
        public float rippleHeight = 0.07f;
        public float rippleScale = 5.5f;
        public float rippleSpeed = 4.2f;
        public float rippleDecay = 0.75f;

        [Header("Buoyancy Settings")]
        public float floatOffset = 0.18f; // Chiều cao nổi của thuyền so với mặt nước (dương để deck cao hơn sóng)
        public float positionLerpSpeed = 12.0f; // Tăng tốc độ đuổi theo sóng để không bị sóng trùm lên deck
        public float rotationLerpSpeed = 10.0f; // Tăng tốc độ nghiêng theo sóng để giữ thuyền luôn nổi song song mặt sóng
        
        [Header("References")]
        public Renderer waterRenderer;

        private void Update()
        {
            // Tự động tìm kiếm mặt nước nếu chưa gán
            if (waterRenderer == null)
            {
                GameObject waterObj = GameObject.Find("Stylized_Water_Surface");
                if (waterObj != null)
                {
                    waterRenderer = waterObj.GetComponent<Renderer>();
                }
            }

            // Đồng bộ vị trí thuyền vào vật liệu nước để vẽ sóng phản xạ
            if (waterRenderer != null && waterRenderer.sharedMaterial != null)
            {
                waterRenderer.sharedMaterial.SetVector("_BoatPos", new Vector4(transform.position.x, transform.position.z, 0f, 0f));
            }

            Vector3 currentPos = transform.position;

            // 1. Lấy vị trí 4 điểm xung quanh thuyền để tính góc nghiêng (Pitch và Roll)
            Vector3 posFront = transform.TransformPoint(new Vector3(0, 0, 1.0f));
            Vector3 posBack = transform.TransformPoint(new Vector3(0, 0, -1.0f));
            Vector3 posLeft = transform.TransformPoint(new Vector3(-0.4f, 0, 0));
            Vector3 posRight = transform.TransformPoint(new Vector3(0.4f, 0, 0));

            // 2. Tính toán chiều cao sóng thực tế tại các điểm
            float hCenter = GetWaveHeight(currentPos);
            float hFront = GetWaveHeight(posFront);
            float hBack = GetWaveHeight(posBack);
            float hLeft = GetWaveHeight(posLeft);
            float hRight = GetWaveHeight(posRight);

            // 3. Nội suy vị trí Y mượt mà
            float targetY = hCenter + floatOffset;
            Vector3 newPos = new Vector3(currentPos.x, targetY, currentPos.z);
            transform.position = Vector3.Lerp(currentPos, newPos, Time.deltaTime * positionLerpSpeed);

            // 4. Tính toán góc nghiêng dọc (Pitch) và nghiêng ngang (Roll) dựa trên độ dốc của sóng
            float pitch = Mathf.Atan2(hFront - hBack, 2.0f) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(hRight - hLeft, 0.8f) * Mathf.Rad2Deg;

            // Xoay thuyền nhấp nhô theo mặt nước
            Quaternion targetRot = Quaternion.Euler(pitch, transform.rotation.eulerAngles.y, -roll);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
        }

        // Sao chép thuật toán dựng sóng Gerstner và sóng phản xạ cọc của Shader nước
        public float GetWaveHeight(Vector3 pos)
        {
            float time = Time.time;

            // A. Sóng Gerstner chính
            Vector2 waveDir = waveDirection.normalized;
            float wavePos = pos.x * waveDir.x + pos.z * waveDir.y;
            float wave1 = Mathf.Sin(wavePos * waveScale + time * waveSpeed) * waveHeight;

            // Sóng phụ chéo góc
            Vector2 waveDir2 = new Vector2(waveDir.x * 0.8f - waveDir.y * 0.6f, waveDir.y * 0.8f + waveDir.x * 0.6f);
            float wavePos2 = pos.x * waveDir2.x + pos.z * waveDir2.y;
            float wave2 = Mathf.Cos(wavePos2 * (waveScale * 1.35f) + time * (waveSpeed * 1.15f)) * (waveHeight * 0.55f);

            float baseHeight = wave1 + wave2;

            // B. Sóng phản xạ đồng tâm từ 2 cột đá
            float dist1 = Vector2.Distance(new Vector2(pos.x, pos.z), pillar1Pos);
            float dist2 = Vector2.Distance(new Vector2(pos.x, pos.z), pillar2Pos);

            float ripple1 = Mathf.Sin(dist1 * rippleScale - time * rippleSpeed) * rippleHeight * Mathf.Exp(-dist1 * rippleDecay);
            float ripple2 = Mathf.Sin(dist2 * rippleScale - time * rippleSpeed) * rippleHeight * Mathf.Exp(-dist2 * rippleDecay);

            // Bổ sung sóng phản xạ từ chính con thuyền để khớp 100% với mặt nước biến dạng của shader
            float distBoat = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(transform.position.x, transform.position.z));
            float rippleBoat = Mathf.Sin(distBoat * rippleScale - time * rippleSpeed) * (rippleHeight * 0.8f) * Mathf.Exp(-distBoat * (rippleDecay * 1.2f));

            return baseHeight + ripple1 + ripple2 + rippleBoat;
        }
    }
}
