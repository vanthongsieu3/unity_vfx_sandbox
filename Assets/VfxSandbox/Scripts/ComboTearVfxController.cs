using System.Collections;
using UnityEngine;

namespace VfxSandbox
{
    public class ComboTearVfxController : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        public GameObject slashPrefab;         // Vệt chém thường
        public GameObject giantTearPrefab;      // Kiếm khí khổng lồ rẽ đất

        [Header("Combo Settings")]
        public float comboResetTime = 1.2f;    // Thời gian reset combo

        [Header("Debug Trigger")]
        public bool triggerSlash = false;

        private int comboIndex = 0;
        private float lastInputTime = 0f;
        
        // Cấu hình Camera Shake tự động
        private Transform mainCamTrans;
        private Vector3 camOriginalPos;
        private Coroutine shakeCoroutine;

        private void Start()
        {
            if (Camera.main != null)
            {
                mainCamTrans = Camera.main.transform;
                camOriginalPos = mainCamTrans.position;
            }
        }

        private void Update()
        {
            // Reset combo nếu người chơi để quá lâu không chém tiếp
            if (comboIndex > 0 && Time.time - lastInputTime > comboResetTime)
            {
                comboIndex = 0;
                Debug.Log("[Combo Tear] Reset combo!");
            }

            if (triggerSlash || Input.GetKeyDown(KeyCode.Space))
            {
                triggerSlash = false;
                TriggerCombo();
            }
        }

        public void TriggerCombo()
        {
            if (slashPrefab == null)
            {
                Debug.LogWarning("[ComboTearVfxController] Chưa gán slashPrefab!");
                return;
            }

            lastInputTime = Time.time;
            Quaternion rotation = transform.rotation;

            if (comboIndex == 0)
            {
                // Nhát 1: Chém xéo ngang từ Trái sang Phải
                rotation *= Quaternion.Euler(18f, -12f, 0f);
                Instantiate(slashPrefab, transform.position + transform.forward * 0.25f, rotation);
                
                StartCameraShake(0.08f, 0.05f); // Rung nhẹ tạo cảm giác chém thép
                comboIndex = 1;
                Debug.Log("[Combo Tear] Nhát 1: Chém Trái");
            }
            else if (comboIndex == 1)
            {
                // Nhát 2: Chém xéo ngang từ Phải sang Trái (Đảo trục)
                rotation *= Quaternion.Euler(-18f, 12f, 180f);
                Instantiate(slashPrefab, transform.position + transform.forward * 0.25f, rotation);
                
                StartCameraShake(0.08f, 0.05f); // Rung nhẹ
                comboIndex = 2;
                Debug.Log("[Combo Tear] Nhát 2: Chém Phải");
            }
            else
            {
                // Nhát 3 (Finisher): Chém dọc từ trên xuống rực sáng cực đại
                rotation *= Quaternion.Euler(90f, 0f, 90f);
                
                // A. Tạo vệt chém bổ dọc lớn tại chỗ
                GameObject slashObj = Instantiate(slashPrefab, transform.position + transform.forward * 0.35f, rotation);
                slashObj.transform.localScale = Vector3.one * 1.4f; // Vệt chém finisher to hơn 40%

                // B. Phóng Kiếm Khí Khổng Lồ Rẽ Đất bay vút đi
                if (giantTearPrefab != null)
                {
                    Quaternion waveRot = transform.rotation * Quaternion.Euler(0f, 0f, 90f);
                    // Sinh kiếm khí đứng cách đất một chút để dò raycast xuống
                    Instantiate(giantTearPrefab, transform.position + transform.forward * 0.45f + new Vector3(0f, 0.3f, 0f), waveRot);
                }
                else
                {
                    Debug.LogWarning("[ComboTearVfxController] Chưa gán giantTearPrefab!");
                }

                StartCameraShake(0.24f, 0.25f); // Rung lắc cực mạnh chấn động mặt đất
                comboIndex = 0;
                Debug.Log("[Combo Tear] Nhát 3: Bổ Dọc Khổng Lồ - Phóng Kiếm Khí Rẽ Đất!");
            }
        }

        private void StartCameraShake(float duration, float intensity)
        {
            if (mainCamTrans == null && Camera.main != null)
            {
                mainCamTrans = Camera.main.transform;
                camOriginalPos = mainCamTrans.position;
            }

            if (mainCamTrans != null)
            {
                if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
                shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float intensity)
        {
            float elapsed = 0f;
            // Lưu lại vị trí ban đầu của camera trước khi lắc để không bị lệch tâm vĩnh viễn
            Vector3 basePos = camOriginalPos;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Tạo độ lệch ngẫu nhiên
                Vector2 randomOffset = Random.insideUnitCircle * intensity;
                mainCamTrans.position = basePos + new Vector3(randomOffset.x, randomOffset.y, 0f);

                yield return null;
            }

            // Trả camera về vị trí cũ an toàn
            mainCamTrans.position = basePos;
            shakeCoroutine = null;
        }
    }
}
