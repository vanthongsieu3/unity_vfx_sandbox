using UnityEngine;

namespace VfxSandbox
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 3.8f, -7.5f);
        public float smoothSpeed = 6.0f;

        private void LateUpdate()
        {
            if (target == null)
            {
                // Tự động tìm kiếm thuyền nếu chưa được gán
                GameObject boat = GameObject.Find("Stylized_Sailboat");
                if (boat != null)
                {
                    target = boat.transform;
                }
                return;
            }

            // Tính toán vị trí đích của Camera ở thế giới phẳng (World Space)
            Vector3 targetPosition = target.position + offset;

            // Di chuyển mượt mà tới vị trí đích
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Luôn hướng Camera về phía thân thuyền
            Vector3 lookTarget = target.position + Vector3.up * 0.5f;
            transform.LookAt(lookTarget);
        }
    }
}
