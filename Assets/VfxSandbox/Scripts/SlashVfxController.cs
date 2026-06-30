using UnityEngine;

namespace VfxSandbox
{
    public class SlashVfxController : MonoBehaviour
    {
        public enum SlashStyle
        {
            Magic,
            Fire,
            Ice,
            Wind,
            Lightning,
            Hell
        }

        [Header("Active Prefabs (Auto-swapped)")]
        public GameObject slashPrefab;
        public GameObject slashWavePrefab; // Prefab Kiếm Khí (projectile) phóng đi

        [Header("Style Pools (Magic, Fire, Ice, Wind, Lightning, Hell)")]
        public GameObject[] slashStylePrefabs = new GameObject[6];
        public GameObject[] slashWaveStylePrefabs = new GameObject[6];

        [Header("Current Active Style")]
        public SlashStyle currentStyle = SlashStyle.Magic;

        [Header("Combo Settings")]
        public float comboResetTime = 1.0f; // Thời gian chờ reset combo nếu không ấn tiếp

        [Header("Debug Control")]
        [Tooltip("Nhấn phím Space hoặc click checkbox này trong Editor để test chiêu chém")]
        public bool triggerSlash = false;

        private int comboIndex = 0; // Chỉ số nhát chém hiện tại (0, 1, 2)
        private float lastInputTime = 0f;

        private void Start()
        {
            UpdateActivePrefabs();
        }

        private void Update()
        {
            // Phím số 1 -> 6 để đổi hệ/kiểu kiếm khí tức thì
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SetStyle(SlashStyle.Magic);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SetStyle(SlashStyle.Fire);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SetStyle(SlashStyle.Ice);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SetStyle(SlashStyle.Wind);
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) SetStyle(SlashStyle.Lightning);
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) SetStyle(SlashStyle.Hell);

            // Tự động reset combo về nhát thứ nhất nếu để lâu không ấn chém
            if (comboIndex > 0 && Time.time - lastInputTime > comboResetTime)
            {
                comboIndex = 0;
                Debug.Log("[Combo System] Reset combo về nhát thứ nhất!");
            }

            if (triggerSlash || Input.GetKeyDown(KeyCode.Space))
            {
                triggerSlash = false;
                TriggerSlashEffect();
            }
        }

        public void SetStyle(SlashStyle style)
        {
            currentStyle = style;
            UpdateActivePrefabs();
            Debug.Log($"[Slash Style] Swapped to {style} style!");
        }

        private void UpdateActivePrefabs()
        {
            int index = (int)currentStyle;
            if (index >= 0 && index < slashStylePrefabs.Length)
            {
                slashPrefab = slashStylePrefabs[index];
            }
            if (index >= 0 && index < slashWaveStylePrefabs.Length)
            {
                slashWavePrefab = slashWaveStylePrefabs[index];
            }
        }

        public void TriggerSlashEffect()
        {
            if (slashPrefab == null)
            {
                Debug.LogWarning("[SlashVfxController] Chưa gán slashPrefab!");
                return;
            }

            lastInputTime = Time.time;
            Quaternion rotation = transform.rotation;

            // Xử lý logic Combo 3 nhát chém
            if (comboIndex == 0)
            {
                // Nhát 1: Chém xéo từ trên xuống (Trái sang Phải)
                rotation *= Quaternion.Euler(20f, -15f, 0f);
                Instantiate(slashPrefab, transform.position + transform.forward * 0.25f, rotation);
                Debug.Log("[Combo System] Strike 1: Slash Left-to-Right!");
                comboIndex = 1;
            }
            else if (comboIndex == 1)
            {
                // Nhát 2: Chém xéo từ trên xuống (Phải sang Trái - lật 180 độ trục Z)
                rotation *= Quaternion.Euler(-20f, 15f, 180f);
                Instantiate(slashPrefab, transform.position + transform.forward * 0.25f, rotation);
                Debug.Log("[Combo System] Strike 2: Slash Right-to-Left!");
                comboIndex = 2;
            }
            else
            {
                // Nhát 3 (Combo Finisher): Chém bổ từ trên xuống chính giữa
                rotation *= Quaternion.Euler(90f, 0f, 90f); // Nghiêng góc dọc chém xuống
                
                // Spawn vệt chém lớn tại chỗ (Spawns closer)
                GameObject slashObj = Instantiate(slashPrefab, transform.position + transform.forward * 0.35f, rotation);
                slashObj.transform.localScale = Vector3.one * 1.2f; // Nhát chém lớn hơn 20%

                // ĐỒNG THỜI: Phóng Kiếm Khí (Slash Wave Projectile) bay thẳng về phía trước
                if (slashWavePrefab != null)
                {
                    // Kiếm khí dựng đứng sát đất hơn
                    Quaternion waveRot = transform.rotation * Quaternion.Euler(0f, 0f, 90f);
                    Instantiate(slashWavePrefab, transform.position + transform.forward * 0.45f + new Vector3(0, 0.25f, 0), waveRot);
                    Debug.Log("[Combo System] Finisher Strike 3: Heavy Vertical Slash + Projectile Wave Launched!");
                }
                else
                {
                    Debug.LogWarning("[SlashVfxController] Chưa gán slashWavePrefab!");
                }

                comboIndex = 0; // Hoàn thành combo, quay lại nhát đầu
            }
        }
    }
}
