using UnityEngine;

namespace VfxSandbox
{
    public class WeatherController : MonoBehaviour
    {
        public enum WeatherPreset 
        { 
            Calm,       // [1] Calm Lagoon (Aquarium / Vịnh bình yên)
            Sunset,     // [2] Sunset Romance (Hoàng hôn lãng mạn)
            Storm,      // [3] Stormy Tempest (Bão tố trùng khơi)
            Lava,       // [4] Boiling Lava (Dung nham nóng chảy)
            Muddy,      // [5] Toxic Muddy (Lũ lụt đục ngầu / Sông Bạch Đằng)
            RedSea,     // [6] Crimson Red Sea (Biển Đỏ)
            BlackSea,   // [7] Ink Black Sea (Biển Đen)
            DeadSea,    // [8] Salty Dead Sea (Biển Chết)
            Frozen      // [9] Frozen Ice Lake (Hồ băng đóng giá)
        }

        [Header("References")]
        public Renderer waterRenderer;
        public BoatFloating boatFloating;
        public Light sunLight;
        public Camera mainCamera;

        [Header("Transition Speed")]
        public float transitionSpeed = 2.0f;

        private WeatherPreset currentPreset = WeatherPreset.Calm;

        // Target values for Lerp
        private float targetWaveHeight;
        private float targetWaveSpeed;
        private Color targetShallowColor;
        private Color targetDeepColor;
        private Color targetSkyColor;
        private Color targetFoamColor;
        private float targetRippleHeight;
        private float targetFloatOffset;

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
        private float curFloatOffset;

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
                curWaveHeight = waterMat.GetFloat("_WaveHeight");
                curWaveSpeed = waterMat.GetFloat("_WaveSpeed");
                curShallowColor = waterMat.GetColor("_ShallowColor");
                curDeepColor = waterMat.GetColor("_DeepColor");
                curSkyColor = waterMat.GetColor("_SkyColor");
                curFoamColor = waterMat.GetColor("_FoamColor");
                curRippleHeight = waterMat.GetFloat("_RippleHeight");
            }

            if (boatFloating != null)
            {
                curFloatOffset = boatFloating.floatOffset;
            }
            else
            {
                curFloatOffset = 0.18f;
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
            // Phím số 1 -> 9 để chuyển đổi nhanh các Preset
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) ApplyPreset(WeatherPreset.Calm, false);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) ApplyPreset(WeatherPreset.Sunset, false);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) ApplyPreset(WeatherPreset.Storm, false);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) ApplyPreset(WeatherPreset.Lava, false);
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) ApplyPreset(WeatherPreset.Muddy, false);
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) ApplyPreset(WeatherPreset.RedSea, false);
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) ApplyPreset(WeatherPreset.BlackSea, false);
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) ApplyPreset(WeatherPreset.DeadSea, false);
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) ApplyPreset(WeatherPreset.Frozen, false);

            float t = Time.deltaTime * transitionSpeed;

            // Lerp các thông số Shader nước
            curWaveHeight = Mathf.Lerp(curWaveHeight, targetWaveHeight, t);
            curWaveSpeed = Mathf.Lerp(curWaveSpeed, targetWaveSpeed, t);
            curShallowColor = Color.Lerp(curShallowColor, targetShallowColor, t);
            curDeepColor = Color.Lerp(curDeepColor, targetDeepColor, t);
            curSkyColor = Color.Lerp(curSkyColor, targetSkyColor, t);
            curFoamColor = Color.Lerp(curFoamColor, targetFoamColor, t);
            curRippleHeight = Mathf.Lerp(curRippleHeight, targetRippleHeight, t);
            curFloatOffset = Mathf.Lerp(curFloatOffset, targetFloatOffset, t);

            // Lerp thông số môi trường
            curSunColor = Color.Lerp(curSunColor, targetSunColor, t);
            curSunIntensity = Mathf.Lerp(curSunIntensity, targetSunIntensity, t);
            curCameraBg = Color.Lerp(curCameraBg, targetCameraBg, t);

            // Slerp góc xoay mặt trời
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

            // Áp dụng lên Buoyancy C# của thuyền
            if (boatFloating != null)
            {
                boatFloating.waveHeight = curWaveHeight;
                boatFloating.waveSpeed = curWaveSpeed;
                boatFloating.rippleHeight = curRippleHeight;
                boatFloating.floatOffset = curFloatOffset;
            }

            // Áp dụng lên Light và Camera
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
                    targetShallowColor = new Color(0.08f, 0.89f, 0.77f, 1.0f); // Xanh ngọc lam trong vắt
                    targetDeepColor = new Color(0.0f, 0.28f, 0.49f, 1.0f);      // Xanh dương sâu lắng
                    targetSkyColor = new Color(0.65f, 0.85f, 0.95f, 1.0f);
                    targetFoamColor = new Color(0.95f, 0.98f, 1.0f, 0.88f);
                    targetRippleHeight = 0.05f;
                    targetFloatOffset = 0.18f;

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
                    targetSkyColor = new Color(0.85f, 0.25f, 0.35f, 1.0f);
                    targetFoamColor = new Color(1.0f, 0.90f, 0.85f, 0.92f);
                    targetRippleHeight = 0.09f;
                    targetFloatOffset = 0.18f;

                    targetSunRotation = new Vector3(15f, -85f, 0f);
                    targetSunColor = new Color(1.0f, 0.40f, 0.10f, 1.0f); // Ánh nắng cam cháy
                    targetSunIntensity = 1.6f;
                    targetCameraBg = new Color(0.40f, 0.12f, 0.28f, 1.0f);
                    break;

                case WeatherPreset.Storm:
                    targetWaveHeight = 0.65f;
                    targetWaveSpeed = 3.2f;
                    targetShallowColor = new Color(0.18f, 0.28f, 0.32f, 1.0f); // Xám xanh giông bão
                    targetDeepColor = new Color(0.06f, 0.10f, 0.12f, 1.0f);     // Đen thẳm trùng khơi
                    targetSkyColor = new Color(0.20f, 0.24f, 0.26f, 1.0f);
                    targetFoamColor = new Color(0.80f, 0.85f, 0.88f, 0.95f);
                    targetRippleHeight = 0.18f;
                    targetFloatOffset = 0.14f; // Sụt nhẹ trong bão tố

                    targetSunRotation = new Vector3(65f, 20f, 0f);
                    targetSunColor = new Color(0.55f, 0.60f, 0.65f, 1.0f); // Ánh sáng xám âm u
                    targetSunIntensity = 0.35f;
                    targetCameraBg = new Color(0.18f, 0.20f, 0.22f, 1.0f);
                    break;

                case WeatherPreset.Lava:
                    targetWaveHeight = 0.32f;
                    targetWaveSpeed = 0.6f; // Chất lưu dung nham quánh đặc, di chuyển chậm
                    targetShallowColor = new Color(1.0f, 0.22f, 0.0f, 1.0f); // Đỏ dung nham rực cháy
                    targetDeepColor = new Color(0.18f, 0.01f, 0.0f, 1.0f);     // Đen đá magma đang nguội
                    targetSkyColor = new Color(0.12f, 0.03f, 0.01f, 1.0f);      // Khói bụi núi lửa u tối
                    targetFoamColor = new Color(1.0f, 0.85f, 0.1f, 0.98f);      // Vệt nứt dung nham vàng phát sáng
                    targetRippleHeight = 0.08f;
                    targetFloatOffset = 0.38f; // Trọng lượng dung nham lớn đẩy thuyền nổi rất cao!

                    targetSunRotation = new Vector3(30f, 60f, 0f);
                    targetSunColor = new Color(1.0f, 0.35f, 0.1f, 1.0f); // Nắng cam hỏa ngục
                    targetSunIntensity = 0.8f;
                    targetCameraBg = new Color(0.08f, 0.02f, 0.01f, 1.0f);
                    break;

                case WeatherPreset.Muddy:
                    targetWaveHeight = 0.25f;
                    targetWaveSpeed = 2.4f; // Nước sông Bạch Đằng chảy xiết cuồn cuộn
                    targetShallowColor = new Color(0.55f, 0.42f, 0.28f, 1.0f); // Phù sa nâu nhạt đục ngầu
                    targetDeepColor = new Color(0.32f, 0.20f, 0.10f, 1.0f);     // Bùn sông sậm tối
                    targetSkyColor = new Color(0.58f, 0.55f, 0.52f, 1.0f);      // Bầu trời nhiều mây u ám
                    targetFoamColor = new Color(0.85f, 0.80f, 0.75f, 0.90f);    // Bọt phù sa màu bùn đất nhạt
                    targetRippleHeight = 0.07f;
                    targetFloatOffset = 0.18f;

                    targetSunRotation = new Vector3(45f, -120f, 0f);
                    targetSunColor = new Color(0.90f, 0.88f, 0.82f, 1.0f); // Nắng chiều hanh khô
                    targetSunIntensity = 1.0f;
                    targetCameraBg = new Color(0.50f, 0.48f, 0.45f, 1.0f);
                    break;

                case WeatherPreset.RedSea:
                    targetWaveHeight = 0.20f;
                    targetWaveSpeed = 1.4f;
                    targetShallowColor = new Color(0.85f, 0.05f, 0.10f, 1.0f); // Đỏ thẫm tảo biển (Biển Đỏ)
                    targetDeepColor = new Color(0.35f, 0.0f, 0.05f, 1.0f);      // Đỏ huyết sậm sâu
                    targetSkyColor = new Color(0.70f, 0.60f, 0.60f, 1.0f);
                    targetFoamColor = new Color(0.98f, 0.88f, 0.88f, 0.90f);    // Bọt hồng đào nhạt
                    targetRippleHeight = 0.06f;
                    targetFloatOffset = 0.20f; // Nước mặn đẩy thuyền cao hơn chút

                    targetSunRotation = new Vector3(50f, 90f, 0f);
                    targetSunColor = new Color(0.95f, 0.92f, 0.88f, 1.0f);
                    targetSunIntensity = 1.2f;
                    targetCameraBg = new Color(0.60f, 0.55f, 0.55f, 1.0f);
                    break;

                case WeatherPreset.BlackSea:
                    targetWaveHeight = 0.35f;
                    targetWaveSpeed = 1.8f;
                    targetShallowColor = new Color(0.04f, 0.05f, 0.08f, 1.0f); // Biển Đen xanh mực đêm tối
                    targetDeepColor = new Color(0.0f, 0.005f, 0.015f, 1.0f);    // Hố đen vực thẳm
                    targetSkyColor = new Color(0.12f, 0.14f, 0.18f, 1.0f);      // Bầu trời đêm tĩnh mịch
                    targetFoamColor = new Color(0.75f, 0.82f, 0.90f, 0.85f);    // Bọt lấp lánh ánh trăng
                    targetRippleHeight = 0.09f;
                    targetFloatOffset = 0.18f;

                    targetSunRotation = new Vector3(80f, 0f, 0f);
                    targetSunColor = new Color(0.65f, 0.75f, 0.90f, 1.0f); // Ánh trăng dịu mát huyền ảo
                    targetSunIntensity = 0.25f;
                    targetCameraBg = new Color(0.04f, 0.05f, 0.07f, 1.0f);
                    break;

                case WeatherPreset.DeadSea:
                    targetWaveHeight = 0.04f; // Biển Chết hàm lượng muối cực cao triệt tiêu hầu hết sóng
                    targetWaveSpeed = 0.4f;
                    targetShallowColor = new Color(0.15f, 0.78f, 0.72f, 1.0f); // Ngọc lục bảo cực đậm đặc
                    targetDeepColor = new Color(0.02f, 0.35f, 0.38f, 1.0f);     // Xanh ngọc lam đậm bão hòa
                    targetSkyColor = new Color(0.78f, 0.85f, 0.88f, 1.0f);
                    targetFoamColor = new Color(0.98f, 1.0f, 0.98f, 0.92f);     // Vệt muối kết tinh trắng toát
                    targetRippleHeight = 0.02f;
                    targetFloatOffset = 0.32f; // Tỷ trọng muối cực cao đẩy thuyền nổi cao vượt trội!

                    targetSunRotation = new Vector3(60f, -45f, 0f);
                    targetSunColor = new Color(1.0f, 0.98f, 0.90f, 1.0f); // Nắng chói chang sa mạc
                    targetSunIntensity = 1.5f;
                    targetCameraBg = new Color(0.70f, 0.78f, 0.80f, 1.0f);
                    break;

                case WeatherPreset.Frozen:
                    targetWaveHeight = 0.06f; // Hồ băng đóng giá tĩnh lặng
                    targetWaveSpeed = 0.3f;
                    targetShallowColor = new Color(0.50f, 0.92f, 1.0f, 1.0f);  // Cyan băng tuyết phát sáng
                    targetDeepColor = new Color(0.05f, 0.45f, 0.65f, 1.0f);     // Glacial xanh lam thẳm sâu
                    targetSkyColor = new Color(0.85f, 0.92f, 0.98f, 1.0f);      // Bầu trời Bắc Cực băng giá
                    targetFoamColor = new Color(1.0f, 1.0f, 1.0f, 0.98f);       // Bọt tuyết trắng buốt đông cứng
                    targetRippleHeight = 0.03f;
                    targetFloatOffset = 0.18f;

                    targetSunRotation = new Vector3(35f, 150f, 0f);
                    targetSunColor = new Color(0.90f, 0.95f, 1.0f, 1.0f); // Ánh nắng mùa đông nhạt màu
                    targetSunIntensity = 1.1f;
                    targetCameraBg = new Color(0.78f, 0.85f, 0.90f, 1.0f);
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
                curFloatOffset = targetFloatOffset;

                curSunRotation = targetSunRotation;
                curSunColor = targetSunColor;
                curSunIntensity = targetSunIntensity;
                curCameraBg = targetCameraBg;

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
                    boatFloating.floatOffset = curFloatOffset;
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
            // Bảng HUD điều khiển 9 Preset nước cao cấp thế hệ mới 3x3 Grid
            GUIStyle styleBox = new GUIStyle(GUI.skin.box);
            styleBox.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.88f); // Màu nền Dark Navy tinh tế
            
            GUILayout.BeginArea(new Rect(20, 20, 360, 260), styleBox);
            GUI.color = oldColor;

            GUILayout.BeginVertical();
            
            GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
            styleTitle.alignment = TextAnchor.MiddleCenter;
            styleTitle.fontStyle = FontStyle.Bold;
            styleTitle.fontSize = 14;
            styleTitle.normal.textColor = Color.cyan;
            GUILayout.Label("MMO WATER SANDBOX PRO PRESETS", styleTitle);

            GUILayout.Space(3);
            
            GUIStyle styleText = new GUIStyle(GUI.skin.label);
            styleText.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            styleText.fontSize = 11;
            GUILayout.Label("▸ Lái thuyền: WASD / Phím mũi tên", styleText);
            GUILayout.Label("▸ Bấm phím số [1-9] để đổi nhanh chất liệu nước:", styleText);

            GUILayout.Space(6);

            // Xây dựng Layout 3x3 Grid cho 9 Presets
            GUIStyle styleBtnActive = new GUIStyle(GUI.skin.button);
            styleBtnActive.fontStyle = FontStyle.Bold;
            styleBtnActive.normal.textColor = Color.green;
            styleBtnActive.fontSize = 11;

            GUIStyle styleBtnNormal = new GUIStyle(GUI.skin.button);
            styleBtnNormal.normal.textColor = Color.white;
            styleBtnNormal.fontSize = 11;

            // Hàng 1: Calm, Sunset, Storm
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(" [1] Calm Lagoon ", currentPreset == WeatherPreset.Calm ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Calm, false);
            if (GUILayout.Button(" [2] Sunset ", currentPreset == WeatherPreset.Sunset ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Sunset, false);
            if (GUILayout.Button(" [3] Stormy ", currentPreset == WeatherPreset.Storm ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Storm, false);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Hàng 2: Lava, Muddy, RedSea
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(" [4] Boiling Lava ", currentPreset == WeatherPreset.Lava ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Lava, false);
            if (GUILayout.Button(" [5] Muddy River ", currentPreset == WeatherPreset.Muddy ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Muddy, false);
            if (GUILayout.Button(" [6] Red Sea ", currentPreset == WeatherPreset.RedSea ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.RedSea, false);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Hàng 3: BlackSea, DeadSea, Frozen
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(" [7] Black Sea ", currentPreset == WeatherPreset.BlackSea ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.BlackSea, false);
            if (GUILayout.Button(" [8] Dead Sea ", currentPreset == WeatherPreset.DeadSea ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.DeadSea, false);
            if (GUILayout.Button(" [9] Frozen Ice ", currentPreset == WeatherPreset.Frozen ? styleBtnActive : styleBtnNormal))
                ApplyPreset(WeatherPreset.Frozen, false);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            
            // Dòng thông tin trạng thái mô tả chi tiết của Preset hiện tại
            GUIStyle styleStatus = new GUIStyle(GUI.skin.label);
            styleStatus.alignment = TextAnchor.MiddleCenter;
            styleStatus.fontStyle = FontStyle.Italic;
            styleStatus.normal.textColor = Color.yellow;
            styleStatus.fontSize = 11;
            
            string description = "";
            switch (currentPreset)
            {
                case WeatherPreset.Calm: description = "Vịnh ngọc lam phẳng lặng, nước trong vắt"; break;
                case WeatherPreset.Sunset: description = "Hoàng hôn nhuộm tím hồng lãng mạn, nắng lóa cam"; break;
                case WeatherPreset.Storm: description = "Bão tố trùng khơi xanh xám, sóng dâng cuồn cuộn"; break;
                case WeatherPreset.Lava: description = "Dung nham nóng chảy đỏ rực, nứt vỡ vàng phát sáng"; break;
                case WeatherPreset.Muddy: description = "Lũ lụt cuồn cuộn / Sông Bạch Đằng phù sa đục ngầu"; break;
                case WeatherPreset.RedSea: description = "Biển Đỏ Crimson rực rỡ, độ mặn cao"; break;
                case WeatherPreset.BlackSea: description = "Biển Đen sâu thẳm huyền bí, óng ánh trăng khuya"; break;
                case WeatherPreset.DeadSea: description = "Biển Chết ngọc bích, phẳng lặng, sức nổi thuyền cực cao"; break;
                case WeatherPreset.Frozen: description = "Hồ băng xanh ngắt lạnh giá, đóng tuyết trắng buốt"; break;
            }
            GUILayout.Label(description, styleStatus);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
