using UnityEngine;

namespace VfxSandbox
{
    public class WeatherController : MonoBehaviour
    {
        public enum WeatherPreset { Calm, Sunset, Storm }

        [Header("References")]
        public Renderer waterRenderer;
        public BoatFloating boatFloating;
        public Light sunLight;
        public Camera mainCamera;

        [Header("Transition Speed")]
        public float transitionSpeed = 1.5f;

        private WeatherPreset currentPreset = WeatherPreset.Calm;

        // Target values for Lerp
        private float targetWaveHeight;
        private float targetWaveSpeed;
        private Color targetShallowColor;
        private Color targetDeepColor;
        private Color targetSkyColor;
        private Color targetFoamColor;
        private float targetRippleHeight;

        private Vector3 targetSunRotation;
        private Color targetSunColor;
        private float targetSunIntensity;
        private Color targetCameraBg;

        // Current Lerped values
        private float curWaveHeight;
        private float curWaveSpeed;
        private Color curShallowColor;
        private Color curDeepColor;
        private Color curSkyColor;
        private Color curFoamColor;
        private float curRippleHeight;

        private Vector3 curSunRotation;
        private Color curSunColor;
        private float curSunIntensity;
        private Color curCameraBg;

        private Material waterMat;

        private void Start()
        {
            if (waterRenderer != null)
            {
                waterMat = waterRenderer.material;
                // Khởi tạo các giá trị ban đầu từ material
                curWaveHeight = waterMat.GetFloat("_WaveHeight");
                curWaveSpeed = waterMat.GetFloat("_WaveSpeed");
                curShallowColor = waterMat.GetColor("_ShallowColor");
                curDeepColor = waterMat.GetColor("_DeepColor");
                curSkyColor = waterMat.GetColor("_SkyColor");
                curFoamColor = waterMat.GetColor("_FoamColor");
                curRippleHeight = waterMat.GetFloat("_RippleHeight");
            }

            if (sunLight != null)
            {
                curSunRotation = sunLight.transform.rotation.eulerAngles;
                curSunColor = sunLight.color;
                curSunIntensity = sunLight.intensity;
            }

            if (mainCamera != null)
            {
                curCameraBg = mainCamera.backgroundColor;
            }

            // Mặc định khởi đầu ở Calm
            ApplyPreset(WeatherPreset.Calm, true);
        }

        private void Update()
        {
            // Đọc bàn phím số 1, 2, 3 để đổi thời tiết
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ApplyPreset(WeatherPreset.Calm, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ApplyPreset(WeatherPreset.Sunset, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ApplyPreset(WeatherPreset.Storm, false);
            }

            // Lerp các giá trị mượt mà theo thời gian
            float t = Time.deltaTime * transitionSpeed;

            curWaveHeight = Mathf.Lerp(curWaveHeight, targetWaveHeight, t);
            curWaveSpeed = Mathf.Lerp(curWaveSpeed, targetWaveSpeed, t);
            curShallowColor = Color.Lerp(curShallowColor, targetShallowColor, t);
            curDeepColor = Color.Lerp(curDeepColor, targetDeepColor, t);
            curSkyColor = Color.Lerp(curSkyColor, targetSkyColor, t);
            curFoamColor = Color.Lerp(curFoamColor, targetFoamColor, t);
            curRippleHeight = Mathf.Lerp(curRippleHeight, targetRippleHeight, t);

            curSunColor = Color.Lerp(curSunColor, targetSunColor, t);
            curSunIntensity = Mathf.Lerp(curSunIntensity, targetSunIntensity, t);
            curCameraBg = Color.Lerp(curCameraBg, targetCameraBg, t);

            // Nội suy góc quay mặt trời tránh gimbal lock
            Quaternion curSunRot = Quaternion.Euler(curSunRotation);
            Quaternion tarSunRot = Quaternion.Euler(targetSunRotation);
            curSunRot = Quaternion.Slerp(curSunRot, tarSunRot, t);
            curSunRotation = curSunRot.eulerAngles;

            // Áp dụng lên Shader
            if (waterMat != null)
            {
                waterMat.SetFloat("_WaveHeight", curWaveHeight);
                waterMat.SetFloat("_WaveSpeed", curWaveSpeed);
                waterMat.SetColor("_ShallowColor", curShallowColor);
                waterMat.SetColor("_DeepColor", curDeepColor);
                waterMat.SetColor("_SkyColor", curSkyColor);
                waterMat.SetColor("_FoamColor", curFoamColor);
                waterMat.SetFloat("_RippleHeight", curRippleHeight);
            }

            // Áp dụng lên C# Buoyancy của thuyền
            if (boatFloating != null)
            {
                boatFloating.waveHeight = curWaveHeight;
                boatFloating.waveSpeed = curWaveSpeed;
                boatFloating.rippleHeight = curRippleHeight;
            }

            // Áp dụng lên Ánh sáng và Camera
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Euler(curSunRotation);
                sunLight.color = curSunColor;
                sunLight.intensity = curSunIntensity;
            }

            if (mainCamera != null)
            {
                mainCamera.backgroundColor = curCameraBg;
            }
        }

        public void ApplyPreset(WeatherPreset preset, bool instant)
        {
            currentPreset = preset;

            switch (preset)
            {
                case WeatherPreset.Calm:
                    targetWaveHeight = 0.15f;
                    targetWaveSpeed = 1.2f;
                    targetShallowColor = new Color(0.08f, 0.89f, 0.77f, 1.0f); // Xanh ngọc lam dịu mát
                    targetDeepColor = new Color(0.0f, 0.28f, 0.49f, 1.0f);      // Xanh dương sâu lắng
                    targetSkyColor = new Color(0.65f, 0.85f, 0.95f, 1.0f);
                    targetFoamColor = new Color(0.95f, 0.98f, 1.0f, 0.88f);
                    targetRippleHeight = 0.05f;

                    targetSunRotation = new Vector3(52f, -34f, 0f);
                    targetSunColor = new Color(1.0f, 0.96f, 0.88f, 1.0f); // Nắng vàng ấm áp
                    targetSunIntensity = 1.3f;
                    targetCameraBg = new Color(0.72f, 0.86f, 0.94f, 1.0f);
                    break;

                case WeatherPreset.Sunset:
                    targetWaveHeight = 0.28f;
                    targetWaveSpeed = 1.8f;
                    targetShallowColor = new Color(1.0f, 0.44f, 0.35f, 1.0f); // San hô cam đỏ hoàng hôn
                    targetDeepColor = new Color(0.24f, 0.04f, 0.42f, 1.0f);     // Tím hồng hoàng hôn thẳm
                    targetSkyColor = new Color(0.85f, 0.25f, 0.35f, 1.0f);      // Bầu trời đỏ hồng rực rỡ
                    targetFoamColor = new Color(1.0f, 0.90f, 0.85f, 0.92f);     // Bọt ánh cam đào
                    targetRippleHeight = 0.09f;

                    targetSunRotation = new Vector3(15f, -85f, 0f); // Mặt trời hạ cực thấp sát chân trời
                    targetSunColor = new Color(1.0f, 0.40f, 0.10f, 1.0f); // Ánh nắng cam cháy
                    targetSunIntensity = 1.6f;
                    targetCameraBg = new Color(0.40f, 0.12f, 0.28f, 1.0f);
                    break;

                case WeatherPreset.Storm:
                    targetWaveHeight = 0.65f; // Sóng cuộn dâng cao
                    targetWaveSpeed = 3.2f;   // Sóng đẩy nhanh cuồn cuộn
                    targetShallowColor = new Color(0.18f, 0.28f, 0.32f, 1.0f); // Xám xanh giông bão
                    targetDeepColor = new Color(0.06f, 0.10f, 0.12f, 1.0f);     // Đen thẳm trùng khơi
                    targetSkyColor = new Color(0.20f, 0.24f, 0.26f, 1.0f);      // Mây đen mù mịt
                    targetFoamColor = new Color(0.80f, 0.85f, 0.88f, 0.95f);    // Bọt xám đục ngầu
                    targetRippleHeight = 0.18f; // Thuyền rẽ sóng dữ dội hơn

                    targetSunRotation = new Vector3(65f, 20f, 0f);
                    targetSunColor = new Color(0.55f, 0.60f, 0.65f, 1.0f); // Ánh sáng xám âm u
                    targetSunIntensity = 0.35f;
                    targetCameraBg = new Color(0.18f, 0.20f, 0.22f, 1.0f);
                    break;
            }

            if (instant)
            {
                curWaveHeight = targetWaveHeight;
                curWaveSpeed = targetWaveSpeed;
                curShallowColor = targetShallowColor;
                curDeepColor = targetDeepColor;
                curSkyColor = targetSkyColor;
                curFoamColor = targetFoamColor;
                curRippleHeight = targetRippleHeight;

                curSunRotation = targetSunRotation;
                curSunColor = targetSunColor;
                curSunIntensity = targetSunIntensity;
                curCameraBg = targetCameraBg;

                // Áp dụng ngay lập tức lên shader/buoyancy
                if (waterMat != null)
                {
                    waterMat.SetFloat("_WaveHeight", curWaveHeight);
                    waterMat.SetFloat("_WaveSpeed", curWaveSpeed);
                    waterMat.SetColor("_ShallowColor", curShallowColor);
                    waterMat.SetColor("_DeepColor", curDeepColor);
                    waterMat.SetColor("_SkyColor", curSkyColor);
                    waterMat.SetColor("_FoamColor", curFoamColor);
                    waterMat.SetFloat("_RippleHeight", curRippleHeight);
                }

                if (boatFloating != null)
                {
                    boatFloating.waveHeight = curWaveHeight;
                    boatFloating.waveSpeed = curWaveSpeed;
                    boatFloating.rippleHeight = curRippleHeight;
                }

                if (sunLight != null)
                {
                    sunLight.transform.rotation = Quaternion.Euler(curSunRotation);
                    sunLight.color = curSunColor;
                    sunLight.intensity = curSunIntensity;
                }

                if (mainCamera != null)
                {
                    mainCamera.backgroundColor = curCameraBg;
                }
            }
        }

        private void OnGUI()
        {
            // Thiết kế bảng HUD điều khiển thời tiết cao cấp góc trên màn hình để hướng dẫn người dùng
            GUIStyle styleBox = new GUIStyle(GUI.skin.box);
            styleBox.normal.background = Texture2D.whiteTexture;
            
            Color oldColor = GUI.color;
            GUI.color = new Color(0.08f, 0.12f, 0.16f, 0.85f); // Background màu xanh thẫm tối giản
            
            GUILayout.BeginArea(new Rect(20, 20, 320, 180), styleBox);
            GUI.color = oldColor;

            GUILayout.BeginVertical();
            
            GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
            styleTitle.alignment = TextAnchor.MiddleCenter;
            styleTitle.fontStyle = FontStyle.Bold;
            styleTitle.fontSize = 15;
            styleTitle.normal.textColor = Color.cyan;
            GUILayout.Label("MMO WATER SANDBOX PREVIEW", styleTitle);
            
            GUILayout.Space(5);
            
            GUIStyle styleText = new GUIStyle(GUI.skin.label);
            styleText.normal.textColor = Color.white;
            styleText.fontSize = 12;
            GUILayout.Label("▸ Điều khiển thuyền: WASD / Phím mũi tên", styleText);
            GUILayout.Label("▸ Bấm phím số để chuyển đổi thời tiết:", styleText);

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            
            GUIStyle styleBtnActive = new GUIStyle(GUI.skin.button);
            styleBtnActive.fontStyle = FontStyle.Bold;
            styleBtnActive.normal.textColor = Color.green;

            GUIStyle styleBtnNormal = new GUIStyle(GUI.skin.button);
            styleBtnNormal.normal.textColor = Color.white;

            if (GUILayout.Button(" [1] Calm Lagoon ", currentPreset == WeatherPreset.Calm ? styleBtnActive : styleBtnNormal))
            {
                ApplyPreset(WeatherPreset.Calm, false);
            }
            if (GUILayout.Button(" [2] Sunset ", currentPreset == WeatherPreset.Sunset ? styleBtnActive : styleBtnNormal))
            {
                ApplyPreset(WeatherPreset.Sunset, false);
            }
            if (GUILayout.Button(" [3] Stormy ", currentPreset == WeatherPreset.Storm ? styleBtnActive : styleBtnNormal))
            {
                ApplyPreset(WeatherPreset.Storm, false);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            GUIStyle styleStatus = new GUIStyle(GUI.skin.label);
            styleStatus.alignment = TextAnchor.MiddleCenter;
            styleStatus.fontStyle = FontStyle.Italic;
            styleStatus.normal.textColor = new Color(0.8f, 0.9f, 1.0f, 0.7f);
            
            string presetName = currentPreset == WeatherPreset.Calm ? "Bình yên Vịnh Hạ Long" :
                                currentPreset == WeatherPreset.Sunset ? "Chiều hoàng hôn vàng" : "Giông bão cuồn cuộn";
            GUILayout.Label("Trạng thái: " + presetName, styleStatus);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
