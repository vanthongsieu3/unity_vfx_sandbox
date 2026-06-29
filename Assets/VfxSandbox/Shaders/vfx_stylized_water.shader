Shader "VFX/StylizedWater"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor("Mau Nuoc Nong (Shallow Color)", Color) = (0.0, 0.8, 0.75, 0.6)  // Màu xanh lam ngọc nông
        _DeepColor("Mau Nuoc Sau (Deep Color)", Color) = (0.02, 0.12, 0.35, 0.95)       // Màu xanh đại dương sâu
        _WaterOpaqueness("Do Duc Nuoc Nong (0: Trong Suot - 1: Duc)", Range(0.0, 1.0)) = 0.45 // Tùy chỉnh độ trong suốt/đục của nước
        _DepthMaxDistance("Khoang Cach Tron Mau Nong/Sau (Met)", Float) = 4.0   // Khoảng cách chuyển màu nông/sâu theo chiều dọc

        [Header(Subsurface Scattering)]
        _SssColor("Mau Thau Quang Dinh Song (Emerald)", Color) = (0.0, 1.0, 0.65, 1.0) // Màu phát sáng của đỉnh sóng khi ngược nắng
        _SssStrength("Cuong Do Phat Sang Thau Quang (SSS)", Float) = 1.5                   // Độ sáng rực của đỉnh sóng
        _SssPower("Do Thu Hep Vien Phat Sang (Mu Pow)", Float) = 4.0                       // Độ tập trung góc nhìn ngược sáng

        [Header(Shoreline and Wave Crest Foam)]
        _FoamColor("Mau Bot Nuoc (Foam Color)", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamDistance("Do Rong Vien Bot Xo Bo (Met)", Float) = 0.55                    // Chiều rộng dải bọt xô vào bờ cát
        _FoamLappingSpeed("Toc Do Thuy Trieu Co Gian Bot Bo", Float) = 1.3               // Tần số co giãn nhịp thở dâng rút của bọt bờ
        _FoamLappingAmplitude("Bien Do Thuy Trieu Co Gian (Met)", Float) = 0.16          // Khoảng cách xô lên/rút xuống của bọt bờ
        _FoamNoiseScale("Kich Thuoc Hat Bot Song (Scale)", Float) = 3.5                  // Độ to nhỏ của nhiễu bọt
        _FoamNoiseWeight("Do Meo Vien Bot (Noise Weight)", Range(0.1, 0.8)) = 0.45     // Độ lồi lõm zích zắc ở viền bọt
        _WaveCrestThreshold("Nguong Chieu Cao Sinh Bot Dinh Song", Float) = 0.12         // Độ cao sóng bắt đầu xuất hiện bọt trắng
        _WaveCrestRange("Do Phuc Vien Bot Tren Dinh Song", Float) = 0.15                 // Dải chuyển tiếp mềm của bọt đỉnh
        _OutlineDistance("Do Rong Bot Vien Om Coc/Thuyen (Met)", Float) = 1.35            // Độ dày dải bọt sủi tăm ôm sát vật thể

        [Header(Normal Map Ripples)]
        _NormalMap("Ban Do Phap Tuyen (Normal Map)", 2D) = "bump" {}
        _NormalScale1("Ty Le Kich Thuoc Gon Song Lop 1", Float) = 0.05
        _NormalScale2("Ty Le Kich Thuoc Gon Song Lop 2", Float) = 0.08
        _NormalSpeed1("Toc Do Cuon Song Lop 1 (Vector2)", Vector) = (0.05, 0.02, 0, 0)
        _NormalSpeed2("Toc Do Cuon Song Lop 2 (Vector2)", Vector) = (-0.03, 0.04, 0, 0)
        _RefractionStrength("Cuong Do Khuc Xa Vien Bien Dang Day", Float) = 0.12         // Độ méo khúc xạ nhìn xuyên đáy nước
        _PlanarReflectionTexture("Anh Phan Chieu Guong (Tu Script Truyen)", 2D) = "black" {}

        [Header(Procedural Gerstner Waves)]
        _WaveDirection("Huong Song Chay (X, Z)", Vector) = (0.0, -1.0, 0, 0)             // Hướng sóng truyền từ khơi vào bờ
        _WaveHeight("Chieu Cao Song Nhap Nho (Met)", Float) = 0.22                        // Độ cao nhấp nhô của đỉnh sóng
        _WaveScale("Tan So Song (Do Day Giua Cac Song)", Float) = 0.85                   // Số lượng ngọn sóng trên một khoảng cách
        _WaveSpeed("Toc Do Song Di Chuyen", Float) = 1.6                                 // Tốc độ lướt sóng

        [Header(Concentric Obstacle Ripples)]
        _Pillar1Pos("Toa Do Coc Da 1 (X, Z)", Vector) = (1.2, 1.5, 0, 0)
        _Pillar2Pos("Toa Do Coc Da 2 (X, Z)", Vector) = (-1.8, 3.2, 0, 0)
        _BoatPos("Toa Do Con Thuyen (X, Z)", Vector) = (-0.5, -1.0, 0, 0)
        _BoatDir("Huong Mui Thuyen (X, Z)", Vector) = (0.0, 0.0, 1.0, 0.0)
        _BoatLength("Chieu Dai Song Thuyen (Tao Song Capsule)", Float) = 1.5              // Kích thước keel tạo sóng kén thon dài
        _BoatSpeed("Toc Do Chay Cua Thuyen (m/s)", Float) = 0.0                          // Tốc độ hiện tại của thuyền
        _RippleHeight("Chieu Cao Song Phan Chan Va Dap", Float) = 0.07                     // Cường độ sóng phản xạ
        _RippleScale("Tan So Song Phan Chan (Met)", Float) = 5.5                         // Số lượng vòng sóng phản chấn
        _RippleSpeed("Toc Do Lan Toa Song Phan Chan", Float) = 4.2                         // Tốc độ loang ra ngoài của vòng sóng
        _RippleDecay("Do Tat Dan Theo Khoang Cach (Decay)", Float) = 0.75                // Độ cản làm tắt sóng khi đi xa

        [Header(Shimmering Caustics)]
        _NoiseMap("Ban Do Nhieu Seamless Caustics", 2D) = "gray" {}
        _CausticsMap("Ban Do Voronoi Luoi Nang Lap Lanh", 2D) = "black" {}
        _NoiseScale("Kich Thuoc Vien Caustics", Float) = 6.0
        _CausticsColor("Mau Luoi Nang Lap Lanh", Color) = (0.7, 1.0, 0.95, 1.0)
        _CausticsPower("Do Sac Net Duong Vien Nang (Mu Pow)", Range(1.0, 15.0)) = 5.0    // Lũy thừa làm mỏng đường gợn nắng thành lưới mảnh
        _CausticsIntensity("Cuong Do Phat Sang Luoi Nang", Float) = 2.0

        [Header(Sky Specular and Reflections)]
        _SkyColor("Mau Bau Troi Phan Chieu (Goc Nghieng)", Color) = (0.45, 0.68, 0.9, 1.0)  // Màu hòa trộn bầu trời ở góc nhìn Fresnel nghiêng
        _ReflectionStrength("Cuong Do Phan Chieu Guong & Troi", Range(0, 1)) = 0.75      // Hệ số pha trộn ảnh phản chiếu thật/sky
        _Glossiness("Do Bong Be Mat Nuoc (Bong Specular)", Float) = 200.0                   // Độ thu hẹp điểm lóa nắng
        _SpecularIntensity("Cuong Do Diem Loa Specular Mat Troi", Float) = 3.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 300
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "StylizedWaterPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldNormal  : TEXCOORD1;
                float3 viewDirWS    : TEXCOORD3;
                float3 worldPos     : TEXCOORD4;
                float waveHeight    : TEXCOORD5;
                float waveSlope     : TEXCOORD6;
            };

            Texture2D _NoiseMap;
            Texture2D _NormalMap;
            Texture2D _CausticsMap;
            sampler2D _PlanarReflectionTexture;

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _WaterOpaqueness;
                float _DepthMaxDistance;
                float4 _SssColor;
                float _SssStrength;
                float _SssPower;
                float4 _FoamColor;
                float _FoamDistance;
                float _FoamLappingSpeed;
                float _FoamLappingAmplitude;
                float _FoamNoiseScale;
                float _FoamNoiseWeight;
                float _WaveCrestThreshold;
                float _WaveCrestRange;
                float _OutlineDistance;
                float _NormalScale1;
                float _NormalScale2;
                float4 _NormalSpeed1;
                float4 _NormalSpeed2;
                float _RefractionStrength;
                float4 _WaveDirection;
                float _WaveHeight;
                float _WaveScale;
                float _WaveSpeed;
                float4 _Pillar1Pos;
                float4 _Pillar2Pos;
                float4 _PillarPositions[10];
                float4 _BoatPos;
                float4 _BoatDir;
                float _BoatLength;
                float _BoatSpeed;
                float _RippleHeight;
                float _RippleScale;
                float _RippleSpeed;
                float _RippleDecay;
                float4 _NoiseMap_ST;
                float _NoiseScale;
                float4 _CausticsColor;
                float _CausticsPower;
                float _CausticsIntensity;
                float4 _SkyColor;
                float _ReflectionStrength;
                float _Glossiness;
                float _SpecularIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Vị trí thế giới của đỉnh nước (World Position)
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 1. Chuẩn hóa hướng truyền sóng waveDir dựa trên XY của Vector để tránh lỗi chia 0 (NaN) làm biến mất Mesh
                float2 waveDir = _WaveDirection.xy;
                if (dot(waveDir, waveDir) < 0.001)
                {
                    waveDir = float2(0.0, -1.0); // Hướng mặc định dọc bờ (sau ra trước)
                }
                else
                {
                    waveDir = normalize(waveDir);
                }

                // Thực hiện biến dạng đa tầng (Multi-frequency organic wiggle) uốn lượn sóng ngẫu nhiên cực đẹp theo nét vẽ tay
                float2 waveTangent = float2(-waveDir.y, waveDir.x);
                float tangentPos = dot(positionWS.xz, waveTangent);
                
                float phasePerp1 = tangentPos * (_WaveScale * 0.35) - _Time.y * 0.7;
                float phasePerp2 = tangentPos * (_WaveScale * 0.85) + _Time.y * 1.1;
                float phasePerp3 = tangentPos * (_WaveScale * 1.75) - _Time.y * 1.6;

                float wiggle = sin(phasePerp1) * 1.8 + cos(phasePerp2) * 0.65 + sin(phasePerp3) * 0.22;
                float wavePos = dot(positionWS.xz, waveDir) + wiggle;

                // Đạo hàm wiggle theo tangentPos để tính Pháp tuyến chuẩn
                float d_wiggle = cos(phasePerp1) * (_WaveScale * 0.35) * 1.8 
                               - sin(phasePerp2) * (_WaveScale * 0.85) * 0.65 
                               + cos(phasePerp3) * (_WaveScale * 1.75) * 0.22;

                float d_wavePos_dx = waveDir.x + d_wiggle * waveTangent.x;
                float d_wavePos_dz = waveDir.y + d_wiggle * waveTangent.y;

                // Phép dựng sóng Gerstner giải tích hướng tâm
                float k1 = _WaveScale;
                float w1 = _Time.y * _WaveSpeed;
                float wave1 = sin(wavePos * k1 - w1) * _WaveHeight;

                // Tạo sóng phụ chéo góc 37 độ uốn cong đa tầng để dập dềnh tự nhiên
                float2 waveDir2 = float2(waveDir.x * 0.8 - waveDir.y * 0.6, waveDir.y * 0.8 + waveDir.x * 0.6);
                float2 waveTangent2 = float2(-waveDir2.y, waveDir2.x);
                float tangentPos2 = dot(positionWS.xz, waveTangent2);
                
                float phasePerp2_1 = tangentPos2 * (_WaveScale * 0.4) - _Time.y * 0.55;
                float phasePerp2_2 = tangentPos2 * (_WaveScale * 0.9) + _Time.y * 0.85;

                float wiggle2 = sin(phasePerp2_1) * 1.3 + cos(phasePerp2_2) * 0.45;
                float wavePos2 = dot(positionWS.xz, waveDir2) + wiggle2;

                // Tính toán đạo hàm của wavePos2 theo X và Z
                float d_wiggle2 = cos(phasePerp2_1) * (_WaveScale * 0.4) * 1.3 
                                - sin(phasePerp2_2) * (_WaveScale * 0.9) * 0.45;

                float d_wavePos2_dx = waveDir2.x + d_wiggle2 * waveTangent2.x;
                float d_wavePos2_dz = waveDir2.y + d_wiggle2 * waveTangent2.y;

                float k2 = _WaveScale * 1.35;
                float w2 = _Time.y * _WaveSpeed * 1.15;
                float wave2 = cos(wavePos2 * k2 - w2) * (_WaveHeight * 0.55);

                float baseWaveHeight = wave1 + wave2;

                // 2. Thêm sóng phản xạ từ 10 cọc đá vôi Vịnh Hạ Long (Majestic Archipelago)
                float totalPillarRipple = 0.0;
                float totalPillarDry_dx = 0.0;
                float totalPillarDry_dz = 0.0;

                for (int i = 0; i < 10; i++)
                {
                    float2 pPos = _PillarPositions[i].xy;
                    if (dot(pPos, pPos) < 0.001) continue; // Bỏ qua nếu cọc trống

                    float2 toP = positionWS.xz - pPos;
                    float pAlong = dot(toP, waveDir);
                    float pPerp = dot(toP, waveTangent);
                    float pAlongScale = pAlong > 0.0 ? 0.65 : 2.5;
                    float defDist = sqrt(pPerp * pPerp * 1.3 + pAlong * pAlong * pAlongScale);
                    
                    float decay = pAlong > 0.0 ? _RippleDecay : _RippleDecay * 2.8;
                    float ripple = sin(defDist * _RippleScale - _Time.y * _RippleSpeed) * _RippleHeight * exp(-defDist * decay);
                    float weight = smoothstep(3.5, 0.0, defDist);
                    
                    totalPillarRipple += ripple * weight;

                    // Tính đạo hàm pháp tuyến cho từng cọc đá vôi
                    float2 rDir = waveTangent * pPerp * 1.3 + waveDir * pAlong * pAlongScale;
                    if (dot(rDir, rDir) > 0.0001)
                    {
                        rDir = normalize(rDir);
                    }
                    float dry_dx = cos(defDist * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir.x * _RippleHeight * exp(-defDist * decay);
                    float dry_dz = cos(defDist * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir.y * _RippleHeight * exp(-defDist * decay);
                    
                    totalPillarDry_dx += dry_dx * weight;
                    totalPillarDry_dz += dry_dz * weight;
                }

                // Khoảng cách hình capsule tới sống thuyền (Boat Keel Segment Projection)
                float2 boatForward = normalize(_BoatDir.xy);
                float2 boatA = _BoatPos.xy - boatForward * (_BoatLength * 0.5);
                float2 boatB = _BoatPos.xy + boatForward * (_BoatLength * 0.5);
                float2 segAB = boatB - boatA;
                float2 vecAP = positionWS.xz - boatA;
                float tSeg = saturate(dot(vecAP, segAB) / max(0.001, dot(segAB, segAB)));
                float2 closestPtBoat = boatA + tSeg * segAB;
                float distBoat = distance(positionWS.xz, closestPtBoat);
                
                // Phân tách Sóng rẽ nước chữ V (Kelvin Wake) khi di chuyển và Sóng nhấp nhô đứng yên (Stationary Bobbing)
                float speedFactor = saturate(_BoatSpeed * 1.5);
                float2 boatRight = float2(-boatForward.y, boatForward.x);
                float2 toBoat = positionWS.xz - _BoatPos.xy;
                
                // 1. Sóng rẽ nước chữ V (Wake) chạy dọc hai bên mạn và kéo dài về phía sau
                float along = dot(toBoat, boatForward);
                float perp = abs(dot(toBoat, boatRight));
                
                // Cải tiến uốn cong phi tuyến (quadratic curve) và tạo nếp sóng uốn lượn hình chữ S mềm mại (Kelvin Wake bending)
                float curvedAlong = along + perp * perp * 0.14;
                float vWiggle = sin(along * 0.45 + perp * 0.22 + _Time.y * 1.5) * 1.35;
                float vPhase = (perp * 0.8 + curvedAlong * 1.8 + vWiggle) * _RippleScale - _Time.y * _RippleSpeed;
                float vDecay = exp(-(perp * 0.8 - along * 0.4) * _RippleDecay);
                
                // Giới hạn vùng ảnh hưởng ở phía sau mũi thuyền và tỏa rộng dần (hoàn toàn triệt tiêu sóng ở trước mũi tàu)
                float vWeight = smoothstep(0.2, -0.6, along) * smoothstep(6.0, 0.0, perp);
                float vWake = sin(vPhase) * (_RippleHeight * 1.8) * vDecay * vWeight * speedFactor;
                
                // Đạo hàm cho Sóng chữ V (kèm đạo hàm curvedAlong và vWiggle tần số thấp)
                float perpSign = sign(dot(toBoat, boatRight));
                float d_along_dx = boatForward.x;
                float d_along_dz = boatForward.y;
                float d_perp_dx = boatRight.x * perpSign;
                float d_perp_dz = boatRight.y * perpSign;
                
                float d_curvedAlong_dx = d_along_dx + 0.28 * perp * d_perp_dx;
                float d_curvedAlong_dz = d_along_dz + 0.28 * perp * d_perp_dz;
                
                float wCos = cos(along * 0.45 + perp * 0.22 + _Time.y * 1.5) * 1.35;
                float d_vWiggle_dx = wCos * (0.45 * d_along_dx + 0.22 * d_perp_dx);
                float d_vWiggle_dz = wCos * (0.45 * d_along_dz + 0.22 * d_perp_dz);
                
                float d_vPhase_dx = (d_perp_dx * 0.8 + d_curvedAlong_dx * 1.8 + d_vWiggle_dx) * _RippleScale;
                float d_vPhase_dz = (d_perp_dz * 0.8 + d_curvedAlong_dz * 1.8 + d_vWiggle_dz) * _RippleScale;
                float dvWake_dx = cos(vPhase) * d_vPhase_dx * (_RippleHeight * 1.8) * vDecay * vWeight * speedFactor;
                float dvWake_dz = cos(vPhase) * d_vPhase_dz * (_RippleHeight * 1.8) * vDecay * vWeight * speedFactor;

                // 2. Sóng dập dềnh đồng tâm (Bobbing) khi thuyền đứng yên hoặc di chuyển rất chậm
                float bobbingWake = sin(distBoat * _RippleScale - _Time.y * _RippleSpeed) * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2)) * (1.0 - speedFactor);
                float weightBoat = smoothstep(3.5, 0.0, distBoat);
                
                float2 rDirBoat = (positionWS.xz - closestPtBoat) / max(0.001, distBoat);
                float dbobbing_dx = cos(distBoat * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDirBoat.x * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2)) * (1.0 - speedFactor);
                float dbobbing_dz = cos(distBoat * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDirBoat.y * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2)) * (1.0 - speedFactor);

                float rippleBoat = vWake + bobbingWake * weightBoat;
                float dryBoat_dx = dvWake_dx + dbobbing_dx * weightBoat;
                float dryBoat_dz = dvWake_dz + dbobbing_dz * weightBoat;

                positionWS.y += baseWaveHeight + totalPillarRipple + rippleBoat;
                output.waveHeight = baseWaveHeight + totalPillarRipple + rippleBoat;

                // 3. Tính toán Vector Pháp tuyến (Normal Vector) chính xác dựa trên đạo hàm sóng chiếu theo hướng và sóng phản xạ
                float dy1_dx = k1 * d_wavePos_dx * cos(wavePos * k1 - w1) * _WaveHeight;
                float dy1_dz = k1 * d_wavePos_dz * cos(wavePos * k1 - w1) * _WaveHeight;

                float dy2_dx = -k2 * d_wavePos2_dx * sin(wavePos2 * k2 - w2) * (_WaveHeight * 0.55);
                float dy2_dz = -k2 * d_wavePos2_dz * sin(wavePos2 * k2 - w2) * (_WaveHeight * 0.55);

                float dy_dx = dy1_dx + dy2_dx + totalPillarDry_dx + dryBoat_dx;
                float dy_dz = dy1_dz + dy2_dz + totalPillarDry_dz + dryBoat_dz;
                float3 waveNormal = normalize(float3(-dy_dx, 1.0, -dy_dz));

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.worldNormal = waveNormal;
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.worldPos = positionWS;
                output.waveSlope = cos(wavePos * k1 - w1);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Tọa độ màn hình (Screen UV)
                float2 screenUv = input.positionCS.xy / _ScreenParams.xy;

                // Tính hướng sóng đồng bộ để cuộn cấu trúc gợn sóng và bọt trôi theo chiều sóng
                float2 waveDir = _WaveDirection.xy;
                if (dot(waveDir, waveDir) < 0.001)
                {
                    waveDir = float2(0.0, -1.0);
                }
                else
                {
                    waveDir = normalize(waveDir);
                }
                float2 waveTangent = float2(-waveDir.y, waveDir.x);

                // 1. Đọc và hòa trộn 2 lớp Map Pháp tuyến (Normal Map) cuộn xuôi dòng hướng bờ (shoreward)
                float2 normalUv1 = input.worldPos.xz * _NormalScale1 + (waveDir * 0.045 + waveTangent * 0.025) * _Time.y;
                float2 normalUv2 = input.worldPos.xz * _NormalScale2 + (waveDir * 0.035 - waveTangent * 0.035) * _Time.y;
                
                float3 normal1 = UnpackNormal(_NormalMap.Sample(sampler_LinearRepeat, normalUv1));
                float3 normal2 = UnpackNormal(_NormalMap.Sample(sampler_LinearRepeat, normalUv2));
                
                // Hòa trộn vector pháp tuyến chi tiết
                float3 blendedNormalMap = normalize(float3(normal1.xy + normal2.xy, normal1.z * normal2.z));
                
                // Kết hợp pháp tuyến sóng thô (vertex) với pháp tuyến gợn nước mịn (normal map)
                float3 worldNormal = normalize(input.worldNormal + blendedNormalMap * 0.4);

                // 2. Tính toán chênh lệch độ sâu (Depth Blending)
                float rawDepth = SampleSceneDepth(screenUv);
                float sceneZ = LinearEyeDepth(rawDepth, _ZBufferParams);
                float screenZ = input.positionCS.w;
                float depthDiff = sceneZ - screenZ;

                // KHÔI PHỤC CHIỀU SÂU GIẢI TÍCH (Nếu Depth Texture bị tắt trên máy người dùng hoặc Web/Mobile)
                if (sceneZ > 500.0 || sceneZ < 0.001)
                {
                    // Cát nghiêng 9 độ: Y_cat = -1.0 - (Z - 1.5) * sin(9 độ) (sin(9 độ) ≈ 0.1564)
                    // Mặt nước ở Y = 0, nên độ sâu nước bằng -Y_cat (cho phép giá trị âm để ẩn nước trên bờ cát khô)
                    float sandY = -1.0 - (input.worldPos.z - 1.5) * 0.1564;
                    depthDiff = -sandY;
                }
                // Tính chu kỳ triều dâng/rút (Tide Cycle) để tăng bọt khi sóng vỗ bờ, giảm bọt khi rút ra xa
                float tide = sin(_Time.y * _FoamLappingSpeed) * 0.5 + 0.5;

                // Tạo gợn sóng uốn lượn hữu cơ dọc theo bờ biển (Organic Sloping Beach Wiggle)
                float shoreWiggle = sin(input.worldPos.x * 0.075 + _Time.y * _WaveSpeed * 0.25) * 0.65 
                                  + cos(input.worldPos.x * 0.032 - _Time.y * _WaveSpeed * 0.12) * 1.1;
                float dynamicDepthDiff = depthDiff + shoreWiggle * (0.22 + tide * 0.18);
                float depthFactor = saturate(dynamicDepthDiff / _DepthMaxDistance);

                // 3. Khúc xạ đáy nước biến dạng (Refraction Distortion)
                float2 distort = blendedNormalMap.xy * _RefractionStrength * saturate(dynamicDepthDiff * 1.5);
                float2 refractUv = screenUv + distort;

                // Kiểm tra an toàn
                float rawDepthDist = SampleSceneDepth(refractUv);
                float sceneZDist = LinearEyeDepth(rawDepthDist, _ZBufferParams);
                if (sceneZDist < screenZ)
                {
                    refractUv = screenUv;
                }
                float3 sceneColor = SampleSceneColor(refractUv);

                // 4. Hòa trộn màu nước nông/sâu dốc
                float3 shallowColor = _ShallowColor.rgb;
                float3 deepColor = _DeepColor.rgb;
                float3 waterBaseColor = lerp(shallowColor, deepColor, depthFactor);

                // 5. Phản chiếu thực tế & Bầu trời dựa trên góc nhìn Fresnel (Planar & Fresnel Sky Reflection)
                float3 viewDir = SafeNormalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(viewDir, worldNormal)), 4.0);
                
                // Đọc ảnh phản chiếu phẳng với biến dạng từ Normal Map để tạo gợn sóng phản chiếu lăn tăn
                float2 reflectUv = screenUv + blendedNormalMap.xy * 0.035;
                reflectUv = clamp(reflectUv, 0.002, 0.998); // Tránh tràn biên Render Texture
                float4 planarReflectionSample = tex2D(_PlanarReflectionTexture, reflectUv);
                
                // Nếu chưa có ảnh phản chiếu (ví dụ ở Edit Mode hoặc tắt script), tự động hòa trộn về màu bầu trời làm mặc định
                float3 reflectionColor = lerp(_SkyColor.rgb, planarReflectionSample.rgb, planarReflectionSample.a);

                // Hòa trộn phản chiếu thực tế (Planar) vào màu gốc của nước
                float3 reflectedColor = lerp(waterBaseColor, reflectionColor, _ReflectionStrength);
                // Hòa trộn thêm một chút màu trời ở góc nhìn Fresnel cực nghiêng để tạo độ sâu
                reflectedColor = lerp(reflectedColor, _SkyColor.rgb, fresnel * 0.45);

                float opacity = lerp(_WaterOpaqueness, _DeepColor.a, depthFactor);

                // 6. Tạo bọt nước xô bờ gợn sóng cách điệu (Stylized Shoreline Foam)
                float2 foamUv = input.worldPos.xz * (_FoamNoiseScale * 0.06) + blendedNormalMap.xy * 0.15 + (waveDir * 0.09 + waveTangent * 0.03) * _Time.y;
                float foamNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv).r;
                
                // Khoảng cách bọt xô bờ tự động co giãn mạnh theo triều dâng
                float dynamicFoamDistance = max(0.05, _FoamDistance * (0.5 + tide * 0.9));
                
                // Độ rộng viền bọt quanh cọc/thuyền tự động dày lên khi sóng vỗ bờ, thu nhỏ mảnh dẻ khi rút ra
                float dynamicOutlineDistance = max(0.05, _OutlineDistance * (0.65 + tide * 0.7));

                // Độ đục lỗ bong bóng nước (độ rách bọt): khi nước rút thì bọt rách/tan cực mạnh (noise weight cao hơn)
                float dynamicNoiseWeight = _FoamNoiseWeight * (1.35 - tide * 0.55);

                // Hệ số tăng bọt khi sóng vỗ bờ dốc tiến tới (crashing slope) và giảm bọt khi rút ngược ra biển (retreating slope)
                float waveFrontFactor = saturate(-input.waveSlope * 0.7 + 0.65);

                // A. Bọt xô bờ (Shoreline Foam) - Luôn chạy dựa trên dynamicDepthDiff để vẽ mép cát tiếp xúc
                float shoreFoamFactor = saturate(1.0 - max(0.0, dynamicDepthDiff) / dynamicFoamDistance);
                float shoreFoamVal = shoreFoamFactor - (1.0 - shoreFoamFactor) * foamNoise * dynamicNoiseWeight * 1.5;
                float shoreFoamMask = smoothstep(0.12, 0.22, shoreFoamVal) * waveFrontFactor;

                // B. Bọt đỉnh sóng (Wave Crest foam) - Phá vỡ các đường bọt song song dẹt thành các mảng bọt trôi nổi bằng phép nhân nhiễu
                float waveCrestFactor = saturate((input.waveHeight - _WaveCrestThreshold) / _WaveCrestRange);
                float waveCrestNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv * 1.6).r;
                float waveCrestMask = smoothstep(0.48, 0.55, waveCrestFactor * waveCrestNoise * 2.3) * waveFrontFactor;

                // C. Bọt sóng phản xạ uốn theo dòng sóng chạy lan tỏa từ các cọc và thuyền (Pulsing Wake-shaped Foam Rings)
                
                // Loop qua 10 cọc đá vôi Vịnh Hạ Long để tính sóng phản xạ và viền bọt giải tích
                float maxPillarFoam = 0.0;
                float maxPillarOutline = 0.0;
                float closestPillarDist = 9999.0;

                for (int i = 0; i < 10; i++)
                {
                    float2 pPos = _PillarPositions[i].xy;
                    if (dot(pPos, pPos) < 0.001) continue;

                    // A. Khoảng cách lệch tâm của sóng phản xạ
                    float2 toP = input.worldPos.xz - pPos;
                    float pAlong = dot(toP, waveDir);
                    float pPerp = dot(toP, waveTangent);
                    float pAlongScale = pAlong > 0.0 ? 0.65 : 2.5;
                    float defDist = sqrt(pPerp * pPerp * 1.3 + pAlong * pAlong * pAlongScale);
                    closestPillarDist = min(closestPillarDist, defDist);

                    // Tính sóng phản xạ đồng tâm
                    float phase = defDist * _RippleScale - _Time.y * _RippleSpeed;
                    float decay = pAlong > 0.0 ? _RippleDecay : _RippleDecay * 2.8;
                    float ring = pow(saturate(sin(phase)), 6.0) * exp(-defDist * decay);
                    
                    float rippleWeight = smoothstep(4.0, 0.0, defDist);
                    maxPillarFoam = max(maxPillarFoam, ring * 0.85 * rippleWeight);

                    // B. Viền giải tích lệch tâm theo dòng chảy (Leeward Shifted Outline)
                    float2 offsetP = pPos + waveDir * 0.45;
                    float2 toPOutline = input.worldPos.xz - offsetP;
                    float pAlongOutline = dot(toPOutline, waveDir);
                    float pPerpOutline = dot(toPOutline, waveTangent);
                    float pScaleOutline = pAlongOutline > 0.0 ? 0.75 : 3.2;
                    float defDistOutline = sqrt(pPerpOutline * pPerpOutline * 1.3 + pAlongOutline * pAlongOutline * pScaleOutline);
                    
                    // Lấy bán kính thực tế từ thành phần .z của _PillarPositions (nếu không có thì mặc định 0.5)
                    float radius = _PillarPositions[i].z > 0.001 ? _PillarPositions[i].z : 0.5;
                    float distToSurf = max(0.0, defDistOutline - radius);
                    float pOutline = saturate(1.0 - distToSurf / dynamicOutlineDistance);
                    maxPillarOutline = max(maxPillarOutline, pOutline);
                }

                // Khoảng cách hình capsule tới sống thuyền (Boat Keel Segment Projection)
                float2 boatForward = normalize(_BoatDir.xy);
                float2 boatA = _BoatPos.xy - boatForward * (_BoatLength * 0.5);
                float2 boatB = _BoatPos.xy + boatForward * (_BoatLength * 0.5);
                float2 segAB = boatB - boatA;
                float2 vecAP = input.worldPos.xz - boatA;
                float tSeg = saturate(dot(vecAP, segAB) / max(0.001, dot(segAB, segAB)));
                float2 closestPtBoat = boatA + tSeg * segAB;
                float distBoat = distance(input.worldPos.xz, closestPtBoat);

                // Phân tách Sóng rẽ nước chữ V (Kelvin Wake) khi di chuyển và Sóng nhấp nhô đứng yên (Stationary Bobbing)
                float speedFactor = saturate(_BoatSpeed * 1.5);
                float2 boatRight = float2(-boatForward.y, boatForward.x);
                float2 toBoat = input.worldPos.xz - _BoatPos.xy;
                
                // 1. Sóng rẽ nước chữ V (Wake) chạy dọc hai bên mạn và kéo dài về phía sau
                float along = dot(toBoat, boatForward);
                float perp = abs(dot(toBoat, boatRight));
                float curvedAlong = along + perp * perp * 0.14;
                float vWiggle = sin(along * 0.45 + perp * 0.22 + _Time.y * 1.5) * 1.35;
                float vPhase = (perp * 0.8 + curvedAlong * 1.8 + vWiggle) * _RippleScale - _Time.y * _RippleSpeed;
                float vDecay = exp(-(perp * 0.8 - along * 0.4) * _RippleDecay);
                
                // Giới hạn vùng ảnh hưởng ở phía sau mũi thuyền và tỏa rộng dần
                float vWeight = smoothstep(0.2, -0.6, along) * smoothstep(6.0, 0.0, perp);
                float vWakeRaw = pow(saturate(sin(vPhase)), 4.0) * vDecay * vWeight * speedFactor;

                // 2. Sóng dập dềnh đồng tâm (Bobbing) khi thuyền đứng yên hoặc di chuyển rất chậm
                float phaseBoat = distBoat * _RippleScale - _Time.y * _RippleSpeed;
                float ringBoat = pow(saturate(sin(phaseBoat)), 6.0) * exp(-distBoat * (_RippleDecay * 1.2));
                float boatRippleWeight = smoothstep(4.0, 0.0, distBoat);
                
                float pillarFoam = maxPillarFoam;

                // Viền giải tích sát thân thuyền (Hull Outline)
                float2 offsetBoat = _BoatPos.xy - boatForward * 0.55; 
                float2 boatA_dyn = offsetBoat - boatForward * (_BoatLength * 0.5);
                float2 boatB_dyn = offsetBoat + boatForward * (_BoatLength * 0.5);
                float2 segAB_dyn = boatB_dyn - boatA_dyn;
                
                float2 vecAPOutline = input.worldPos.xz - boatA_dyn;
                float tSegOutline = saturate(dot(vecAPOutline, segAB_dyn) / max(0.001, dot(segAB_dyn, segAB_dyn)));
                float2 closestPtBoatOutline = boatA_dyn + tSegOutline * segAB_dyn;
                float distBoatOutline = distance(input.worldPos.xz, closestPtBoatOutline);
                float distToBoatSurf = max(0.0, distBoatOutline - 0.45);
                float boatOutline = saturate(1.0 - distToBoatSurf / dynamicOutlineDistance);
                
                // Tạo vệt bọt khí sủi bọt/rẽ sóng hình chữ V nở rộng (Turbulent Expanding Wake Cone) sau đuôi thuyền
                // Tính khoảng cách lùi dần về phía sau từ đuôi thuyền (stern)
                float distBehind = max(0.0, -along - 0.7);
                // Vệt bọt khí nở rộng phi tuyến tính tỏa rộng thành hình quạt phân rã ra hai bên (quadratic expansion)
                float wakeWidth = 0.45 + distBehind * 0.45 + distBehind * distBehind * 0.06;
                // Biên hình nón V-shaped
                float coneFactor = saturate(1.0 - perp / wakeWidth);
                
                // Suy giảm độ dày bọt khi đi xa dần (kéo dài theo tốc độ thuyền)
                float wakeDecay = exp(-distBehind * (0.35 / (speedFactor + 0.01)));
                // Hoàn toàn triệt tiêu bọt khí rẽ sóng ở trước đuôi tàu (along > -0.7)
                float wakeConeWeight = smoothstep(-0.7, -1.2, along);
                float wakeFoamBase = coneFactor * wakeDecay * speedFactor * wakeConeWeight;
                
                // Trộn thêm 2 lớp nhiễu trôi ngược dòng để đục lỗ rách bong bóng khí sủi bọt chân thực (cuộn theo hướng đuôi tàu)
                float2 wakeNoiseUv1 = input.worldPos.xz * (_FoamNoiseScale * 0.09) - boatForward * _Time.y * 0.18;
                float2 wakeNoiseUv2 = input.worldPos.xz * (_FoamNoiseScale * 0.045) - boatForward * _Time.y * 0.09;
                float wakeNoise1 = _NoiseMap.Sample(sampler_LinearRepeat, wakeNoiseUv1).r;
                float wakeNoise2 = _NoiseMap.Sample(sampler_LinearRepeat, wakeNoiseUv2).r;
                float combinedWakeNoise = lerp(wakeNoise1, wakeNoise2, 0.45);
                
                // Trừ nhiễu mạnh trên toàn bộ vệt bọt để tạo lỗ rách bong bóng sủi bọt ngẫu nhiên (không bị đặc ở tâm)
                float wakeFoamVal = wakeFoamBase * 2.0 - combinedWakeNoise * dynamicNoiseWeight * 1.95;
                float wakeFoamMask = smoothstep(0.12, 0.25, wakeFoamVal);
                
                // Gộp chung viền cứng sát thân và vệt bọt khí sủi kéo dài đằng sau
                float totalBoatOutline = max(boatOutline, wakeFoamMask);
                
                float analyticalOutline = max(maxPillarOutline, totalBoatOutline);

                // 2. Viền dựa trên chiều sâu thực tế (nếu có bật Depth)
                float depthOutline = 0.0;
                if (rawDepth > 0.0001 && sceneZ < 500.0)
                {
                    depthOutline = saturate(1.0 - max(0.0, depthDiff) / dynamicOutlineDistance);
                }

                // Gộp cả 2 nguồn viền để đảm bảo luôn hiện viền rõ nét
                float outlineFactor = max(depthOutline, analyticalOutline);
                
                // Sử dụng vân Voronoi (Caustics Map) cuộn xuôi dòng hướng bờ để đục lỗ bong bóng nước tròn trịa
                float2 outlineFoamUv = input.worldPos.xz * (_FoamNoiseScale * 0.045) + (waveDir * 0.04 + waveTangent * 0.025) * _Time.y;
                float outlineNoise = _CausticsMap.Sample(sampler_LinearRepeat, outlineFoamUv).r;
                
                // Lực bọt rách tạo bong bóng: ở gần vật thể bọt đặc trắng tinh, đi xa dần bị đục lỗ và rã thành các mảng trôi nổi nhỏ
                float outlineFoamVal = outlineFactor - (1.0 - outlineFactor) * outlineNoise * dynamicNoiseWeight * 1.7;
                float outlineMask = smoothstep(0.12, 0.22, outlineFoamVal) * waveFrontFactor;

                // Đồng bộ bọt sóng phản chấn cọc theo sườn sóng dốc đón bờ
                float finalPillarFoam = pillarFoam * waveFrontFactor;

                // Áp nhiễu và làm rách bọt sóng chữ V cho đồng bộ style nghệ thuật vẽ tay
                float boatWakeFoamRaw = smoothstep(0.18, 0.28, vWakeRaw * (0.45 + combinedWakeNoise * 1.15)) * 1.5;
                
                // Sóng dập dềnh đồng tâm (Bobbing) khi thuyền đứng yên hoặc di chuyển rất chậm
                float ringBoat = pow(saturate(sin(phaseBoat)), 5.0) * exp(-distBoat * (_RippleDecay * 1.2));
                float bobbingFoam = ringBoat * 0.85 * boatRippleWeight * (1.0 - speedFactor) * (0.6 + combinedWakeNoise * 0.8);
                
                float boatWakeFoam = max(boatWakeFoamRaw, bobbingFoam);

                // Gộp chung năm loại bọt nước
                float foamCutout = max(max(max(max(shoreFoamMask, waveCrestMask), finalPillarFoam), outlineMask), boatWakeFoam);

                // Xử lý ẩn hoàn toàn nước và bọt trên bãi cát khô (khi dynamicDepthDiff < 0.0)
                if (dynamicDepthDiff < 0.0)
                {
                    opacity = 0.0;
                    foamCutout = 0.0;
                }

                float3 finalWaterColor = lerp(sceneColor, reflectedColor, opacity);
                finalWaterColor = lerp(finalWaterColor, _FoamColor.rgb, foamCutout * _FoamColor.a);

                // 7. Tạo vân nắng lung linh khúc xạ (Distorted Caustics) cuộn xuôi dòng hướng bờ
                float2 causticsUv1 = input.worldPos.xz * _NoiseScale * 0.06 + blendedNormalMap.xy * 0.12 + (waveDir * 0.045 + waveTangent * 0.02) * _Time.y;
                float2 causticsUv2 = input.worldPos.xz * _NoiseScale * 0.082 - blendedNormalMap.xy * 0.1 + (waveDir * 0.03 - waveTangent * 0.045) * _Time.y;
                float noiseVal1 = _CausticsMap.Sample(sampler_LinearRepeat, causticsUv1).r;
                float noiseVal2 = _CausticsMap.Sample(sampler_LinearRepeat, causticsUv2).r;
                float caustics = noiseVal1 * noiseVal2;
                
                // Lũy thừa pow như Shader Graph để tạo viền vân nắng siêu mảnh, sắc và long lanh
                float causticsMask = pow(caustics, _CausticsPower) * _CausticsIntensity;
                float causticsFade = smoothstep(0.08, 0.35, dynamicDepthDiff);
                finalWaterColor += causticsMask * _CausticsColor.rgb * causticsFade * (1.0 - foamCutout);

                // 8. Hiệu ứng Thấu quang Đỉnh Sóng (Subsurface Scattering / Wave Translucency)
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                
                float waveCrestHeight = saturate(input.waveHeight / max(0.001, _WaveHeight * 1.5));
                float sssFactor = pow(saturate(dot(viewDir, -lightDir)), _SssPower) * waveCrestHeight * _SssStrength;
                finalWaterColor += _SssColor.rgb * sssFactor * mainLight.color * (1.0 - foamCutout);

                // 9. Tính toán phản xạ mặt trời chói mắt (Stylized Specular Highlight)
                float3 halfDir = SafeNormalize(lightDir + viewDir);
                float NdotH = saturate(dot(worldNormal, halfDir));
                float specular = pow(NdotH, _Glossiness) * _SpecularIntensity;
                float3 specColor = mainLight.color * specular;

                finalWaterColor += specColor * (1.0 - foamCutout);

                return float4(finalWaterColor, 1.0);
            }
            ENDHLSL
        }
    }
}
