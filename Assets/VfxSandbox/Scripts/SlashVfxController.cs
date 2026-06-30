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

                // ĐỒNG THỜI: Phóng Kiếm Khí (Slash Wave Projectile) bay thẳng về phía trước theo dạng trận pháp Tu Tiên hoành tráng!
                if (slashWavePrefab != null)
                {
                    SpawnThemedProjectiles();
                }
                else
                {
                    Debug.LogWarning("[SlashVfxController] Chưa gán slashWavePrefab!");
                }

                comboIndex = 0; // Hoàn thành combo, quay lại nhát đầu
            }
        }

        private void SpawnThemedProjectiles()
        {
            Vector3 spawnBase = transform.position + transform.forward * 0.45f + new Vector3(0f, 0.25f, 0f);
            Quaternion baseRot = transform.rotation;

            switch (currentStyle)
            {
                case SlashStyle.Magic:
                    // 🔮 Ma pháp: Tam Kiếm Khí Hình Quạt (3 waves spreading)
                    {
                        float[] angles = { -15f, 0f, 15f };
                        foreach (float angle in angles)
                        {
                            Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 90f); // Nghiêng góc ngang Y, đứng dọc Z 90
                            Instantiate(slashWavePrefab, spawnBase, rot);
                        }
                        Debug.Log("[Tu Tien VFX] Spawned 3-Way Magic Wave Array!");
                    }
                    break;

                case SlashStyle.Fire:
                    // 🔥 Hỏa hệ: Tam Hỏa Phách Bức Tường Lửa (3 waves side-by-side clearing the path)
                    {
                        float[] offsetsX = { -1.4f, 0f, 1.4f };
                        foreach (float ox in offsetsX)
                        {
                            Vector3 pos = spawnBase + transform.right * ox;
                            Quaternion rot = baseRot * Quaternion.Euler(0f, 0f, 90f);
                            GameObject go = Instantiate(slashWavePrefab, pos, rot);
                            go.transform.localScale = Vector3.one * 1.15f; // To hầm hố
                        }
                        Debug.Log("[Tu Tien VFX] Spawned 3-Way Fire Wall Array!");
                    }
                    break;

                case SlashStyle.Ice:
                    // ❄️ Băng hệ: Vạn Băng Trực Tiễn (5 ice spikes fan array)
                    {
                        float[] angles = { -22f, -11f, 0f, 11f, 22f };
                        float[] offsetsX = { -1.8f, -0.9f, 0f, 0.9f, 1.8f };
                        for (int i = 0; i < 5; i++)
                        {
                            Vector3 pos = spawnBase + transform.right * offsetsX[i];
                            Quaternion rot = baseRot * Quaternion.Euler(0f, angles[i], 0f); // Con tự xoay X 90 rồi nên cha xoay Y thôi
                            GameObject go = Instantiate(slashWavePrefab, pos, rot);
                            
                            float scaleFactor = 1f - Mathf.Abs(angles[i]) / 60f;
                            go.transform.localScale = Vector3.one * scaleFactor * 1.1f;
                        }
                        Debug.Log("[Tu Tien VFX] Spawned 5-Way Ice Spike Barrage!");
                    }
                    break;

                case SlashStyle.Wind:
                    // 🌪️ Phong hệ: Tam Long Cuồng Phong (1 giant tornado center + 2 smaller tornadoes side)
                    {
                        // Giữa (Giant)
                        GameObject centerTornado = Instantiate(slashWavePrefab, spawnBase, baseRot * Quaternion.Euler(0f, 0f, 90f));
                        centerTornado.transform.localScale = Vector3.one * 1.6f;

                        // Trái (Small)
                        Vector3 leftPos = spawnBase - transform.right * 1.5f - transform.forward * 0.3f;
                        GameObject leftTornado = Instantiate(slashWavePrefab, leftPos, baseRot * Quaternion.Euler(0f, -10f, 90f));
                        leftTornado.transform.localScale = Vector3.one * 0.9f;

                        // Phải (Small)
                        Vector3 rightPos = spawnBase + transform.right * 1.5f - transform.forward * 0.3f;
                        GameObject rightTornado = Instantiate(slashWavePrefab, rightPos, baseRot * Quaternion.Euler(0f, 10f, 90f));
                        rightTornado.transform.localScale = Vector3.one * 0.9f;

                        Debug.Log("[Tu Tien VFX] Spawned Triple Wind Tornado Array!");
                    }
                    break;

                case SlashStyle.Lightning:
                    // ⚡ Lôi hệ: Lôi Điện Liên Hoàn (3 electric arcs, fast and randomized scale)
                    {
                        float[] angles = { -12f, 0f, 12f };
                        foreach (float angle in angles)
                        {
                            Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 90f);
                            GameObject go = Instantiate(slashWavePrefab, spawnBase, rot);
                            go.transform.localScale = new Vector3(Random.Range(0.8f, 1.3f), Random.Range(0.8f, 1.3f), 1f);
                        }
                        Debug.Log("[Tu Tien VFX] Spawned Triple Lightning Strike!");
                    }
                    break;

                case SlashStyle.Hell:
                    // 💀 Địa ngục hệ: Bách Quỷ Dạ Hành (5 skulls flying in swarm with height offsets)
                    {
                        float[] offsetsX = { -1.5f, -0.7f, 0f, 0.7f, 1.5f };
                        float[] heights = { 0.1f, 0.4f, 0.2f, 0.5f, 0.1f };
                        float[] delays = { 0f, 0.05f, 0.02f, 0.06f, 0f };

                        for (int i = 0; i < 5; i++)
                        {
                            Vector3 pos = spawnBase + transform.right * offsetsX[i] + new Vector3(0f, heights[i], 0f);
                            Quaternion rot = baseRot * Quaternion.Euler(0f, offsetsX[i] * -5f, 90f);
                            
                            GameObject go = Instantiate(slashWavePrefab, pos - transform.forward * delays[i] * 10f, rot);
                            go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
                        }
                        Debug.Log("[Tu Tien VFX] Spawned 5-Skull Hell Swarm!");
                    }
                    break;
            }
        }
    }
}
