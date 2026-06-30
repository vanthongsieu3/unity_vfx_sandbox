using UnityEngine;

namespace VfxSandbox
{
    public class BoatVfxController : MonoBehaviour
    {
        [Header("Particle Systems")]
        public ParticleSystem sternWake;  // Bọt đuôi thuyền
        public ParticleSystem bowWakeL;    // Sóng rẽ nước mạn trái
        public ParticleSystem bowWakeR;    // Sóng rẽ nước mạn phải

        [Header("Emission Rates")]
        public float maxSternEmission = 45f;
        public float maxBowEmission = 25f;

        [Header("Particle Settings")]
        public float maxStartSpeed = 2.5f;
        public float maxStartSize = 0.08f;

        private BoatController controller;

        private void Start()
        {
            controller = GetComponent<BoatController>();
        }

        private void Update()
        {
            if (controller == null) return;

            // Tính tỷ lệ tốc độ thực tế (từ 0.0 đến 1.0)
            float speedRatio = Mathf.Clamp01(Mathf.Abs(controller.currentSpeed) / controller.moveSpeed);

            // 1. Điều khiển bọt khí ở đuôi thuyền (Stern Wake)
            if (sternWake != null)
            {
                var emission = sternWake.emission;
                // Khi đứng yên sủi tăm nhẹ (rate = 5), khi ga hết cỡ phun mạnh (rate = max)
                emission.rateOverTime = Mathf.Lerp(5f, maxSternEmission, speedRatio);

                var main = sternWake.main;
                // Khi đi nhanh bọt phun mạnh hơn về sau
                main.startSpeed = Mathf.Lerp(0.5f, maxStartSpeed, speedRatio);
                main.startSize = Mathf.Lerp(0.03f, maxStartSize, speedRatio);
            }

            // 2. Điều khiển sóng rẽ nước 2 bên mũi thuyền (Bow Waves)
            if (bowWakeL != null)
            {
                var emission = bowWakeL.emission;
                // Đứng yên thì không rẽ sóng, đi nhanh thì rẽ sóng mạnh
                emission.rateOverTime = maxBowEmission * speedRatio;

                var main = bowWakeL.main;
                main.startSpeed = Mathf.Lerp(0.2f, maxStartSpeed * 0.8f, speedRatio);
                main.startSize = Mathf.Lerp(0.02f, maxStartSize * 0.9f, speedRatio);
            }

            if (bowWakeR != null)
            {
                var emission = bowWakeR.emission;
                emission.rateOverTime = maxBowEmission * speedRatio;

                var main = bowWakeR.main;
                main.startSpeed = Mathf.Lerp(0.2f, maxStartSpeed * 0.8f, speedRatio);
                main.startSize = Mathf.Lerp(0.02f, maxStartSize * 0.9f, speedRatio);
            }
        }
    }
}
