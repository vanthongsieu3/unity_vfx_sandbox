using UnityEngine;

namespace VfxSandbox
{
    public class SlashVfxController : MonoBehaviour
    {
        [Header("VFX Prefab")]
        public GameObject slashPrefab;

        [Header("Combat Combo Animation Settings")]
        [Tooltip("Thay đổi góc quay giữa các nhát chém để tạo cảm giác combo liên hoàn trái/phải")]
        public bool alternateSwings = true;
        
        [Header("Debug Control")]
        [Tooltip("Nhấn phím Space hoặc click checkbox này trong Editor để test chiêu chém")]
        public bool triggerSlash = false;

        private bool nextSwingLeft = true;

        private void Update()
        {
            if (triggerSlash || Input.GetKeyDown(KeyCode.Space))
            {
                triggerSlash = false;
                TriggerSlashEffect();
            }
        }

        public void TriggerSlashEffect()
        {
            if (slashPrefab == null)
            {
                Debug.LogWarning("[SlashVfxController] Chưa gán slashPrefab!");
                return;
            }

            // Tính toán góc quay của cung chém để mô phỏng các góc đánh khác nhau (Combo)
            Quaternion rotation = transform.rotation;
            
            if (alternateSwings)
            {
                if (nextSwingLeft)
                {
                    // Vung chém xiên chéo từ trên xuống trái sang phải
                    rotation *= Quaternion.Euler(20f, -15f, 0f);
                }
                else
                {
                    // Vung chém ngược lại: lật 180 độ trục Z để đảo chiều cung chém
                    rotation *= Quaternion.Euler(-20f, 15f, 180f);
                }
                nextSwingLeft = !nextSwingLeft;
            }
            else
            {
                // Vung chém nằm ngang
                rotation *= Quaternion.Euler(0f, 0f, 0f);
            }

            // Khởi tạo đối tượng chém tại vị trí hiện tại
            GameObject slashObj = Instantiate(slashPrefab, transform.position + transform.forward * 0.5f, rotation);
            
            // Prefab sẽ tự động chạy hoạt ảnh quét chém, phai nhạt và tự hủy bằng script VfxSlashAnimator
            Debug.Log("[SlashVfx] Triggered Magic Sword Slash!");
        }
    }
}
