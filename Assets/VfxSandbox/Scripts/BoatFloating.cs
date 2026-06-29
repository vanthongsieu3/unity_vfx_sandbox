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

        private Vector4[] pillarPositionsArray = new Vector4[10];
        private Vector3 lastPosition;
        private float smoothSpeed = 0f;

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            // Tính toán tốc độ phẳng của thuyền
            Vector3 velocity = (transform.position - lastPosition) / Mathf.Max(0.0001f, Time.deltaTime);
            velocity.y = 0f; 
            float currentSpeed = velocity.magnitude;
            smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, Time.deltaTime * 3.5f);
            lastPosition = transform.position;

            // Tự động tìm kiếm mặt nước nếu chưa gán
            if (waterRenderer == null)
            {
                GameObject waterObj = GameObject.Find("Stylized_Water_Surface");
                if (waterObj != null)
                {
                    waterRenderer = waterObj.GetComponent<Renderer>();
                }
            }

            // Tự động dò tìm tất cả 10 đảo đá vôi để đồng bộ mảng vị trí lên Shader và dùng cho CPU
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            Vector4[] pillarPositions = new Vector4[10];
            int count = 0;
            foreach (var go in allObjects)
            {
                if (go != null && go.name.StartsWith("Water_Rock_Obstacle_"))
                {
                    if (count < 10)
                    {
                        float radius = go.transform.localScale.x * 0.5f;
                        pillarPositions[count] = new Vector4(go.transform.position.x, go.transform.position.z, radius, 0f);
                        count++;
                    }
                }
            }
            // Điền nốt các slot trống bằng Vector4.zero
            for (int i = count; i < 10; i++)
            {
                pillarPositions[i] = Vector4.zero;
            }
            
            // Lưu lại cho CPU
            this.pillarPositionsArray = pillarPositions;

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

                // Đẩy tọa độ động của thuyền và mảng 10 cọc đá lên shader
                mat.SetVector("_BoatPos", new Vector4(transform.position.x, transform.position.z, 0f, 0f));
                mat.SetVector("_BoatDir", new Vector4(transform.forward.x, transform.forward.z, 0f, 0f));
                mat.SetFloat("_BoatSpeed", smoothSpeed);
                mat.SetVectorArray("_PillarPositions", pillarPositions);
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

            // B. Sóng phản xạ dạng vệt nước (Wake) từ 10 cọc đá vôi Vịnh Hạ Long
            float totalPillarRipple = 0f;
            if (pillarPositionsArray != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector4 p = pillarPositionsArray[i];
                    if (p.sqrMagnitude < 0.001f) continue;

                    Vector2 pPos = new Vector2(p.x, p.y);
                    Vector2 toP = new Vector2(pos.x, pos.z) - pPos;
                    float pAlong = Vector2.Dot(toP, waveDir);
                    float pPerp = Vector2.Dot(toP, waveTangent);
                    float pAlongScale = pAlong > 0.0f ? 0.65f : 2.5f;
                    float defDist = Mathf.Sqrt(pPerp * pPerp * 1.3f + pAlong * pAlong * pAlongScale);

                    float decay = pAlong > 0.0f ? rippleDecay : rippleDecay * 2.8f;
                    float ripple = Mathf.Sin(defDist * rippleScale - time * rippleSpeed) * rippleHeight * Mathf.Exp(-defDist * decay);
                    
                    // Giới hạn khoảng cách sóng cọc 3.5m khớp shader bằng hàm smoothstep
                    float tClamp = Mathf.Clamp01((3.5f - defDist) / 3.5f);
                    float weight = tClamp * tClamp * (3f - 2f * tClamp);

                    totalPillarRipple += ripple * weight;
                }
            }

            // Bổ sung sóng phản xạ hình chữ V (Wake) hoặc nhấp nhô đứng yên (Bobbing) từ chính con thuyền để khớp 100% với mặt nước biến dạng của shader
            Vector2 boatForward = new Vector2(transform.forward.x, transform.forward.z);
            if (boatForward.sqrMagnitude < 0.001f) boatForward = new Vector2(0f, 1f);
            else boatForward.Normalize();

            float speedFactor = Mathf.Clamp01(smoothSpeed * 1.5f);
            Vector2 boatRight = new Vector2(-boatForward.y, boatForward.x);
            Vector2 toBoat = new Vector2(pos.x, pos.z) - new Vector2(transform.position.x, transform.position.z);
            
            // 1. Sóng chữ V (Wake)
            float along = Vector2.Dot(toBoat, boatForward);
            float perp = Mathf.Abs(Vector2.Dot(toBoat, boatRight));
            
            float curvedAlong = along + perp * perp * 0.14f;
            float vWiggle = Mathf.Sin(along * 0.45f + perp * 0.22f + time * 1.5f) * 1.35f;
            float vPhase = (perp * 0.8f + curvedAlong * 1.8f + vWiggle) * rippleScale - time * rippleSpeed;
            float vDecay = Mathf.Exp(-(perp * 0.8f - along * 0.4f) * rippleDecay);
            
            // Hàm smoothstep thủ công tương đương smoothstep(0.2f, -0.6f, along)
            float tAlong = Mathf.Clamp01((0.2f - along) / (0.2f - (-0.6f)));
            float wAlong = tAlong * tAlong * (3f - 2f * tAlong);
            
            // Hàm smoothstep thủ công tương đương smoothstep(6.0f, 0.0f, perp)
            float tPerp = Mathf.Clamp01((6.0f - perp) / 6.0f);
            float wPerp = tPerp * tPerp * (3f - 2f * tPerp);
            
            float vWake = Mathf.Sin(vPhase) * (rippleHeight * 1.8f) * vDecay * (wAlong * wPerp) * speedFactor;

            // 2. Sóng dập dềnh đứng yên (Bobbing)
            Vector2 boatA = new Vector2(transform.position.x, transform.position.z) - boatForward * (boatLength * 0.5f);
            Vector2 boatB = new Vector2(transform.position.x, transform.position.z) + boatForward * (boatLength * 0.5f);
            Vector2 segAB = boatB - boatA;
            Vector2 vecAP = new Vector2(pos.x, pos.z) - boatA;
            float tSeg = Mathf.Clamp01(Vector2.Dot(vecAP, segAB) / Mathf.Max(0.001f, Vector2.Dot(segAB, segAB)));
            Vector2 closestPtBoat = boatA + tSeg * segAB;
            float distBoat = Vector2.Distance(new Vector2(pos.x, pos.z), closestPtBoat);

            float bobbingWake = Mathf.Sin(distBoat * rippleScale - time * rippleSpeed) * (rippleHeight * 0.8f) * Mathf.Exp(-distBoat * (rippleDecay * 1.2f)) * (1.0f - speedFactor);
            
            float tBoatDist = Mathf.Clamp01((3.5f - distBoat) / 3.5f);
            float weightBoat = tBoatDist * tBoatDist * (3f - 2f * tBoatDist);

            float rippleBoat = vWake + bobbingWake * weightBoat;

            return baseHeight + totalPillarRipple + rippleBoat;
        }
    }
}
