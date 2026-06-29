using UnityEngine;

namespace VfxSandbox
{
    public class BoatFloating : MonoBehaviour
    {
        [Header("Wave Parameters (Must match Shader)")]
        public float waveHeight = 0.22f;
        public float waveScale = 0.85f;
        public float waveSpeed = 1.6f;
        public Vector2 waveDirection = new Vector2(0f, 1f);

        [Header("Obstacle Ripple Parameters (Must match Shader)")]
        public Vector2 pillar1Pos = new Vector2(1.2f, 1.5f);
        public Vector2 pillar2Pos = new Vector2(-1.8f, 3.2f);
        public float rippleHeight = 0.07f;
        public float rippleScale = 5.5f;
        public float rippleSpeed = 4.2f;
        public float rippleDecay = 0.75f;
        public float boatLength = 1.5f;

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

            // Tự động dò tìm và gán 2 đảo đá vôi gần thuyền nhất để kích hoạt bọt sóng phản chấn
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            float minDist1 = float.MaxValue;
            float minDist2 = float.MaxValue;
            Vector2 closestP1 = new Vector2(1.2f, 1.5f);
            Vector2 closestP2 = new Vector2(-1.8f, 3.2f);
            Vector2 boatPos2D = new Vector2(transform.position.x, transform.position.z);

            foreach (var go in allObjects)
            {
                if (go != null && go.name.StartsWith("Water_Rock_Obstacle_"))
                {
                    Vector2 rockPos2D = new Vector2(go.transform.position.x, go.transform.position.z);
                    float dist = Vector2.Distance(boatPos2D, rockPos2D);
                    if (dist < minDist1)
                    {
                        minDist2 = minDist1;
                        closestP2 = closestP1;
                        
                        minDist1 = dist;
                        closestP1 = rockPos2D;
                    }
                    else if (dist < minDist2)
                    {
                        minDist2 = dist;
                        closestP2 = rockPos2D;
                    }
                }
            }

            // Tự động đồng bộ hóa toàn bộ thông số từ Material sang C# để làm Single Source of Truth
            if (waterRenderer != null && waterRenderer.sharedMaterial != null)
            {
                Material mat = waterRenderer.sharedMaterial;
                waveHeight = mat.GetFloat("_WaveHeight");
                waveScale = mat.GetFloat("_WaveScale");
                waveSpeed = mat.GetFloat("_WaveSpeed");
                
                Vector4 wDir = mat.GetVector("_WaveDirection");
                waveDirection = new Vector2(wDir.x, wDir.y);
                
                rippleHeight = mat.GetFloat("_RippleHeight");
                rippleScale = mat.GetFloat("_RippleScale");
                rippleSpeed = mat.GetFloat("_RippleSpeed");
                rippleDecay = mat.GetFloat("_RippleDecay");
                boatLength = mat.GetFloat("_BoatLength");

                // Đẩy tọa độ động của thuyền và 2 cột gần nhất lên shader để vẽ sóng phản chấn hình capsule
                mat.SetVector("_BoatPos", new Vector4(transform.position.x, transform.position.z, 0f, 0f));
                mat.SetVector("_BoatDir", new Vector4(transform.forward.x, transform.forward.z, 0f, 0f));
                mat.SetVector("_Pillar1Pos", new Vector4(closestP1.x, closestP1.y, 0f, 0f));
                mat.SetVector("_Pillar2Pos", new Vector4(closestP2.x, closestP2.y, 0f, 0f));

                // Lưu lại tọa độ cọc để phục vụ hàm tính toán GetWaveHeight nổi của CPU
                pillar1Pos = closestP1;
                pillar2Pos = closestP2;
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

            // A. Sóng Gerstner chính có biến dạng uốn lượn đa tầng (Multi-frequency organic wiggle)
            Vector2 waveDir = waveDirection.normalized;
            Vector2 waveTangent = new Vector2(-waveDir.y, waveDir.x);
            float tangentPos = pos.x * waveTangent.x + pos.z * waveTangent.y;
            
            float phasePerp1 = tangentPos * (waveScale * 0.35f) - time * 0.7f;
            float phasePerp2 = tangentPos * (waveScale * 0.85f) + time * 1.1f;
            float phasePerp3 = tangentPos * (waveScale * 1.75f) - time * 1.6f;

            float wiggle = Mathf.Sin(phasePerp1) * 1.8f + Mathf.Cos(phasePerp2) * 0.65f + Mathf.Sin(phasePerp3) * 0.22f;
            float wavePos = (pos.x * waveDir.x + pos.z * waveDir.y) + wiggle;
            float wave1 = Mathf.Sin(wavePos * waveScale - time * waveSpeed) * waveHeight;

            // Sóng phụ chéo góc cũng được uốn cong đa tầng
            Vector2 waveDir2 = new Vector2(waveDir.x * 0.8f - waveDir.y * 0.6f, waveDir.y * 0.8f + waveDir.x * 0.6f);
            Vector2 waveTangent2 = new Vector2(-waveDir2.y, waveDir2.x);
            float tangentPos2 = pos.x * waveTangent2.x + pos.z * waveTangent2.y;
            
            float phasePerp2_1 = tangentPos2 * (waveScale * 0.4f) - time * 0.55f;
            float phasePerp2_2 = tangentPos2 * (waveScale * 0.9f) + time * 0.85f;

            float wiggle2 = Mathf.Sin(phasePerp2_1) * 1.3f + Mathf.Cos(phasePerp2_2) * 0.45f;
            float wavePos2 = (pos.x * waveDir2.x + pos.z * waveDir2.y) + wiggle2;
            float wave2 = Mathf.Cos(wavePos2 * (waveScale * 1.35f) - time * (waveSpeed * 1.15f)) * (waveHeight * 0.55f);

            float baseHeight = wave1 + wave2;

            // B. Sóng phản xạ dạng vệt nước (Wake) uốn cong theo dòng chảy của sóng chính từ 2 cột đá
            Vector2 toP1 = new Vector2(pos.x, pos.z) - pillar1Pos;
            float p1Along = Vector2.Dot(toP1, waveDir);
            float p1Perp = Vector2.Dot(toP1, waveTangent);
            float p1AlongScale = p1Along > 0.0f ? 0.65f : 2.5f;
            float defDist1 = Mathf.Sqrt(p1Perp * p1Perp * 1.3f + p1Along * p1Along * p1AlongScale);

            Vector2 toP2 = new Vector2(pos.x, pos.z) - pillar2Pos;
            float p2Along = Vector2.Dot(toP2, waveDir);
            float p2Perp = Vector2.Dot(toP2, waveTangent);
            float p2AlongScale = p2Along > 0.0f ? 0.65f : 2.5f;
            float defDist2 = Mathf.Sqrt(p2Perp * p2Perp * 1.3f + p2Along * p2Along * p2AlongScale);

            float decay1 = p1Along > 0.0f ? rippleDecay : rippleDecay * 2.8f;
            float decay2 = p2Along > 0.0f ? rippleDecay : rippleDecay * 2.8f;

            float ripple1 = Mathf.Sin(defDist1 * rippleScale - time * rippleSpeed) * rippleHeight * Mathf.Exp(-defDist1 * decay1);
            float ripple2 = Mathf.Sin(defDist2 * rippleScale - time * rippleSpeed) * rippleHeight * Mathf.Exp(-defDist2 * decay2);

            // Bổ sung sóng phản xạ hình capsule từ chính con thuyền để khớp 100% với mặt nước biến dạng của shader
            Vector2 boatForward = new Vector2(transform.forward.x, transform.forward.z);
            if (boatForward.sqrMagnitude < 0.001f) boatForward = new Vector2(0f, 1f);
            else boatForward.Normalize();

            Vector2 boatA = new Vector2(transform.position.x, transform.position.z) - boatForward * (boatLength * 0.5f);
            Vector2 boatB = new Vector2(transform.position.x, transform.position.z) + boatForward * (boatLength * 0.5f);
            Vector2 segAB = boatB - boatA;
            Vector2 vecAP = new Vector2(pos.x, pos.z) - boatA;
            float tSeg = Mathf.Clamp01(Vector2.Dot(vecAP, segAB) / Mathf.Max(0.001f, Vector2.Dot(segAB, segAB)));
            Vector2 closestPtBoat = boatA + tSeg * segAB;
            float distBoat = Vector2.Distance(new Vector2(pos.x, pos.z), closestPtBoat);

            float rippleBoat = Mathf.Sin(distBoat * rippleScale - time * rippleSpeed) * (rippleHeight * 0.8f) * Mathf.Exp(-distBoat * (rippleDecay * 1.2f));

            return baseHeight + ripple1 + ripple2 + rippleBoat;
        }
    }
}
