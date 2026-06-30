using UnityEngine;

namespace VfxSandbox
{
    public class BoatController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5.0f;
        public float turnSpeed = 120.0f;
        public float acceleration = 3.0f;
        public float deceleration = 2.5f;

        private float currentSpeed = 0f;

        private void Update()
        {
            // Đọc tín hiệu phím WASD hoặc phím Mũi tên
            float moveInput = Input.GetAxis("Vertical");
            float turnInput = Input.GetAxis("Horizontal");

            // Tăng tốc hoặc giảm tốc mượt mà dựa trên lực kéo động học
            float targetSpeed = moveInput * moveSpeed;
            if (moveInput != 0)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            // Di chuyển con thuyền bằng CharacterController để xử lý va chạm trượt với đảo đá và bờ cát
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
                // Di chuyển thuần ngang X-Z để nhường trục đứng Y cho BoatFloating dập dềnh theo sóng
                movement.y = 0f; 
                controller.Move(movement);
            }
            else
            {
                Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
                transform.position += movement;
            }

            // Xoay hướng mũi thuyền quanh trục đứng Y (Yaw)
            float turnRotation = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, turnRotation);
        }
    }
}
