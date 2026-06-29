Shader "VFX/StylizedWater"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor("Shallow Color", Color) = (0.0, 0.8, 0.75, 0.6)  // Màu xanh lam ngọc nông
        _DeepColor("Deep Color", Color) = (0.02, 0.12, 0.35, 0.95)       // Màu xanh đại dương sâu
        _WaterOpaqueness("Water Opaqueness (Trong/Duc)", Range(0.0, 1.0)) = 0.45 // Độ đục/trong suốt của nước biển nông cơ bản của nước
        _DepthMaxDistance("Depth Color Blending Distance", Float) = 4.0   // Khoảng cách chuyển màu nông/sâu

        [Header(Subsurface Scattering Translucency)]
        _SssColor("SSS Color (Translucency)", Color) = (0.0, 1.0, 0.65, 1.0) // Màu thấu quang xanh lục bảo rực rỡ
        _SssStrength("SSS Strength", Float) = 1.5                         // Cường độ phát quang đỉnh sóng
        _SssPower("SSS Power Angle", Float) = 4.0                         // Độ tụ hướng nhìn ngược sáng

        [Header(Shoreline and Wave Crest Foam)]
        _FoamColor("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamDistance("Shore Foam Width", Float) = 0.55                   // Độ rộng bọt sóng xô bờ trung bình
        _FoamLappingSpeed("Foam Lapping Speed", Float) = 1.3              // Tốc độ dâng/rút thủy triều xô bờ
        _FoamLappingAmplitude("Foam Lapping Amplitude", Float) = 0.16     // Biên độ co giãn dâng rút của bọt bờ
        _FoamNoiseScale("Foam Noise Scale", Float) = 3.5                  // Tỉ lệ nhiễu bọt sóng
        _FoamNoiseWeight("Foam Edge Noise Distortion", Range(0.1, 0.8)) = 0.45 // Độ lồi lõm của mép bọt sóng
        _WaveCrestThreshold("Wave Crest Foam Threshold", Float) = 0.12    // Điểm cao của sóng bắt đầu sinh bọt đỉnh
        _WaveCrestRange("Wave Crest Foam Range", Float) = 0.15            // Dải chuyển tiếp bọt đỉnh sóng
        _OutlineDistance("Hugging Outline Width", Float) = 1.35           // Độ dày của viền bọt ôm sát cọc và thuyền

        [Header(Normal Map Ripples)]
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale1("Normal Scale 1", Float) = 0.05
        _NormalScale2("Normal Scale 2", Float) = 0.08
        _NormalSpeed1("Normal Speed 1 (X, Y)", Vector) = (0.05, 0.02, 0, 0)
        _NormalSpeed2("Normal Speed 2 (X, Y)", Vector) = (-0.03, 0.04, 0, 0)
        _RefractionStrength("Refraction Distortion Strength", Float) = 0.12 // Độ khúc xạ biến dạng đáy nước
        _PlanarReflectionTexture("Planar Reflection Texture", 2D) = "black" {} // Kết cấu phản chiếu thực tế từ camera phản chiếu

        [Header(Procedural Gerstner Waves)]
        _WaveDirection("Wave Propagation Direction (X, Y)", Vector) = (0.0, -1.0, 0, 0) // Hướng truyền sóng (mặc định từ sau ra trước hướng về bờ cát cạn)
        _WaveHeight("Wave Height", Float) = 0.22                          // Chiều cao nhấp nhô của sóng
        _WaveScale("Wave Scale/Frequency", Float) = 0.85                  // Tần số sóng
        _WaveSpeed("Wave Speed", Float) = 1.6                             // Tốc độ sóng

        [Header(Concentric Obstacle Ripples)]
        _Pillar1Pos("Pillar 1 Position (X, Z)", Vector) = (1.2, 1.5, 0, 0)
        _Pillar2Pos("Pillar 2 Position (X, Z)", Vector) = (-1.8, 3.2, 0, 0)
        _BoatPos("Boat Position (X, Z)", Vector) = (-0.5, -1.0, 0, 0)     // Vị trí thuyền để tính sóng phản xạ
        _BoatDir("Boat Forward Direction (X, Z)", Vector) = (0.0, 0.0, 1.0, 0.0) // Hướng mũi thuyền
        _BoatLength("Boat Keel Length", Float) = 1.5                      // Chiều dài sống thuyền để tạo sóng hình capsule
        _RippleHeight("Obstacle Ripple Height", Float) = 0.07             // Độ cao của sóng phản xạ từ cọc
        _RippleScale("Obstacle Ripple Frequency", Float) = 5.5            // Tần số sóng phản xạ từ cọc
        _RippleSpeed("Obstacle Ripple Speed", Float) = 4.2                // Tốc độ lan tỏa sóng phản xạ từ cọc
        _RippleDecay("Obstacle Ripple Decay", Float) = 0.75               // Độ tắt dần của sóng phản xạ theo khoảng cách

        [Header(Shimmering Caustics)]
        _NoiseMap("Seamless Noise Map", 2D) = "gray" {}
        _CausticsMap("Caustics Map (Voronoi)", 2D) = "black" {}
        _NoiseScale("Caustics Scale", Float) = 6.0
        _CausticsColor("Caustics Color", Color) = (0.7, 1.0, 0.95, 1.0)   // Màu vân sóng nắng
        _CausticsPower("Caustics Power (Sharpness)", Range(1.0, 15.0)) = 5.0 // Cường độ lũy thừa tạo vân nét sắc sảo
        _CausticsIntensity("Caustics Intensity", Float) = 2.0

        [Header(Sky Specular and Reflections)]
        _SkyColor("Sky Color (Reflection)", Color) = (0.45, 0.68, 0.9, 1.0) // Màu phản chiếu bầu trời
        _ReflectionStrength("Fresnel Reflection Strength", Range(0, 1)) = 0.75
        _Glossiness("Specular Glossiness", Float) = 200.0
        _SpecularIntensity("Specular Intensity", Float) = 3.5
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
                float4 _BoatPos;
                float4 _BoatDir;
                float _BoatLength;
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
                float wavePos = dot(positionWS.xz, waveDir);

                // Phép dựng sóng Gerstner giải tích hướng tâm
                float k1 = _WaveScale;
                float w1 = _Time.y * _WaveSpeed;
                float wave1 = sin(wavePos * k1 + w1) * _WaveHeight;

                // Tạo sóng phụ chéo góc 37 độ để dập dềnh tự nhiên
                float2 waveDir2 = float2(waveDir.x * 0.8 - waveDir.y * 0.6, waveDir.y * 0.8 + waveDir.x * 0.6);
                float wavePos2 = dot(positionWS.xz, waveDir2);
                float k2 = _WaveScale * 1.35;
                float w2 = _Time.y * _WaveSpeed * 1.15;
                float wave2 = cos(wavePos2 * k2 + w2) * (_WaveHeight * 0.55);

                float baseWaveHeight = wave1 + wave2;

                // 2. Thêm sóng phản xạ từ các cọc và thuyền (cọc tròn, thuyền hình capsule sống thuyền)
                float dist1 = distance(positionWS.xz, _Pillar1Pos.xy);
                float dist2 = distance(positionWS.xz, _Pillar2Pos.xy);
                
                // Khoảng cách hình capsule tới sống thuyền (Boat Keel Segment Projection)
                float2 boatForward = normalize(_BoatDir.xy);
                float2 boatA = _BoatPos.xy - boatForward * (_BoatLength * 0.5);
                float2 boatB = _BoatPos.xy + boatForward * (_BoatLength * 0.5);
                float2 segAB = boatB - boatA;
                float2 vecAP = positionWS.xz - boatA;
                float tSeg = saturate(dot(vecAP, segAB) / max(0.001, dot(segAB, segAB)));
                float2 closestPtBoat = boatA + tSeg * segAB;
                float distBoat = distance(positionWS.xz, closestPtBoat);

                float ripple1 = sin(dist1 * _RippleScale - _Time.y * _RippleSpeed) * _RippleHeight * exp(-dist1 * _RippleDecay);
                float ripple2 = sin(dist2 * _RippleScale - _Time.y * _RippleSpeed) * _RippleHeight * exp(-dist2 * _RippleDecay);
                float rippleBoat = sin(distBoat * _RippleScale - _Time.y * _RippleSpeed) * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2));

                positionWS.y += baseWaveHeight + ripple1 + ripple2 + rippleBoat;
                output.waveHeight = baseWaveHeight + ripple1 + ripple2 + rippleBoat;

                // 3. Tính toán Vector Pháp tuyến (Normal Vector) chính xác dựa trên đạo hàm sóng chiếu theo hướng và sóng phản xạ
                float dy1_dx = k1 * waveDir.x * cos(wavePos * k1 + w1) * _WaveHeight;
                float dy1_dz = k1 * waveDir.y * cos(wavePos * k1 + w1) * _WaveHeight;

                float dy2_dx = -k2 * waveDir2.x * sin(wavePos2 * k2 + w2) * (_WaveHeight * 0.55);
                float dy2_dz = -k2 * waveDir2.y * sin(wavePos2 * k2 + w2) * (_WaveHeight * 0.55);

                // Đạo hàm cho sóng phản xạ cọc 1
                float2 rDir1 = (positionWS.xz - _Pillar1Pos.xy) / max(0.001, dist1);
                float dry1_dx = cos(dist1 * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir1.x * _RippleHeight * exp(-dist1 * _RippleDecay);
                float dry1_dz = cos(dist1 * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir1.y * _RippleHeight * exp(-dist1 * _RippleDecay);

                // Đạo hàm cho sóng phản xạ cọc 2
                float2 rDir2 = (positionWS.xz - _Pillar2Pos.xy) / max(0.001, dist2);
                float dry2_dx = cos(dist2 * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir2.x * _RippleHeight * exp(-dist2 * _RippleDecay);
                float dry2_dz = cos(dist2 * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDir2.y * _RippleHeight * exp(-dist2 * _RippleDecay);

                // Đạo hàm cho sóng phản xạ hình capsule của thuyền (dựa trên điểm gần nhất trên sống thuyền)
                float2 rDirBoat = (positionWS.xz - closestPtBoat) / max(0.001, distBoat);
                float dryBoat_dx = cos(distBoat * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDirBoat.x * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2));
                float dryBoat_dz = cos(distBoat * _RippleScale - _Time.y * _RippleSpeed) * _RippleScale * rDirBoat.y * (_RippleHeight * 0.8) * exp(-distBoat * (_RippleDecay * 1.2));

                float dy_dx = dy1_dx + dy2_dx + dry1_dx + dry2_dx + dryBoat_dx;
                float dy_dz = dy1_dz + dy2_dz + dry1_dz + dry2_dz + dryBoat_dz;
                float3 waveNormal = normalize(float3(-dy_dx, 1.0, -dy_dz));

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.worldNormal = waveNormal;
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.worldPos = positionWS;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Tọa độ màn hình (Screen UV)
                float2 screenUv = input.positionCS.xy / _ScreenParams.xy;

                // 1. Đọc và hòa trộn 2 lớp Map Pháp tuyến (Normal Map) chuyển động chéo nhau (Ép Sampler Repeat)
                float2 normalUv1 = input.worldPos.xz * _NormalScale1 + _NormalSpeed1.xy * _Time.y;
                float2 normalUv2 = input.worldPos.xz * _NormalScale2 + _NormalSpeed2.xy * _Time.y;
                
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
                float depthFactor = saturate(depthDiff / _DepthMaxDistance);

                // 3. Khúc xạ đáy nước biến dạng (Refraction Distortion)
                float2 distort = blendedNormalMap.xy * _RefractionStrength * saturate(depthDiff * 1.5);
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
                float3 finalWaterColor = lerp(sceneColor, reflectedColor, opacity);

                // 6. Tạo bọt nước xô bờ gợn sóng cách điệu (Stylized Shoreline Foam)
                float2 foamUv = input.worldPos.xz * _FoamNoiseScale + blendedNormalMap.xy * 0.15 + float2(_Time.y * 0.12, _Time.y * 0.06);
                float foamNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv).r;
                
                // A. Bọt xô bờ & cọc (Shoreline & Pillar Intersection foam) với hiệu ứng thủy triều nhấp nhô (Lapping)
                float shoreFoamMask = 0.0;
                if (rawDepth > 0.0001)
                {
                    // Lapping animation: bọt co giãn dâng rút tuần hoàn theo thời gian
                    float lapping = sin(_Time.y * _FoamLappingSpeed) * _FoamLappingAmplitude;
                    float dynamicFoamDistance = max(0.05, _FoamDistance + lapping);

                    float shoreFoamFactor = saturate(1.0 - max(0.0, depthDiff) / dynamicFoamDistance);
                    // Thuật toán trừ nhiễu làm bọt rách tạo lỗ bong bóng / viền rách ngẫu nhiên ở mép ngoài bờ
                    float shoreFoamVal = shoreFoamFactor - (1.0 - shoreFoamFactor) * foamNoise * _FoamNoiseWeight * 1.5;
                    shoreFoamMask = smoothstep(0.12, 0.22, shoreFoamVal);
                }

                // B. Bọt đỉnh sóng (Wave Crest foam) - Phá vỡ các đường bọt song song dẹt thành các mảng bọt trôi nổi bằng phép nhân nhiễu
                float waveCrestFactor = saturate((input.waveHeight - _WaveCrestThreshold) / _WaveCrestRange);
                float waveCrestNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv * 1.6).r;
                float waveCrestMask = smoothstep(0.48, 0.55, waveCrestFactor * waveCrestNoise * 2.3);

                // C. Bọt sóng phản xạ đồng tâm lan tỏa từ các cọc và thuyền (Pulsing Concentric Foam Rings)
                float dist1 = distance(input.worldPos.xz, _Pillar1Pos.xy);
                float dist2 = distance(input.worldPos.xz, _Pillar2Pos.xy);
                
                // Khoảng cách hình capsule tới sống thuyền (Boat Keel Segment Projection)
                float2 boatForward = normalize(_BoatDir.xy);
                float2 boatA = _BoatPos.xy - boatForward * (_BoatLength * 0.5);
                float2 boatB = _BoatPos.xy + boatForward * (_BoatLength * 0.5);
                float2 segAB = boatB - boatA;
                float2 vecAP = input.worldPos.xz - boatA;
                float tSeg = saturate(dot(vecAP, segAB) / max(0.001, dot(segAB, segAB)));
                float2 closestPtBoat = boatA + tSeg * segAB;
                float distBoat = distance(input.worldPos.xz, closestPtBoat);
                
                // Đọc pha sóng phản xạ từ cọc 1, cọc 2 và thuyền
                float phase1 = dist1 * _RippleScale - _Time.y * _RippleSpeed;
                float phase2 = dist2 * _RippleScale - _Time.y * _RippleSpeed;
                float phaseBoat = distBoat * _RippleScale - _Time.y * _RippleSpeed;

                // Tạo nhiều vòng tròn đồng tâm sắc nét ở các đỉnh sóng (crest) và dập tắt dần theo khoảng cách exp
                float ring1 = pow(saturate(sin(phase1)), 6.0) * exp(-dist1 * _RippleDecay);
                float ring2 = pow(saturate(sin(phase2)), 6.0) * exp(-dist2 * _RippleDecay);
                float ringBoat = pow(saturate(sin(phaseBoat)), 6.0) * exp(-distBoat * (_RippleDecay * 1.2));

                // Gộp chung các vòng bọt phản chấn, nhân với hệ số cường độ và giới hạn bán kính ảnh hưởng
                float pillarFoam = max(max(ring1, ring2), ringBoat) * 0.85 * smoothstep(4.0, 0.0, min(min(dist1, dist2), distBoat));

                // D. Bọt viền ôm khít sát sạt vật thể (Solid outline foam hugging the boat and pillars) - Độc lập góc dốc
                float outlineMask = 0.0;
                if (rawDepth > 0.0001)
                {
                    float outlineFactor = saturate(1.0 - max(0.0, depthDiff) / _OutlineDistance);
                    // Sử dụng vân Voronoi (Caustics Map) cuộn chậm để đục lỗ bong bóng nước tròn trịa giống xà phòng sủi bọt chuẩn Zelda/Genshin
                    float2 outlineFoamUv = input.worldPos.xz * 0.38 + float2(_Time.y * 0.04, _Time.y * 0.02);
                    float outlineNoise = _CausticsMap.Sample(sampler_LinearRepeat, outlineFoamUv).r;
                    
                    // Lực bọt rách tạo bong bóng: ở gần vật thể bọt đặc trắng tinh, đi xa dần bị đục lỗ và rã thành các mảng trôi nổi nhỏ
                    float outlineFoamVal = outlineFactor - (1.0 - outlineFactor) * outlineNoise * _FoamNoiseWeight * 1.7;
                    outlineMask = smoothstep(0.12, 0.22, outlineFoamVal);
                }

                // Gộp chung bốn loại bọt nước
                float foamCutout = max(max(max(shoreFoamMask, waveCrestMask), pillarFoam), outlineMask);
                finalWaterColor = lerp(finalWaterColor, _FoamColor.rgb, foamCutout * _FoamColor.a);

                // 7. Tạo vân nắng lung linh khúc xạ (Distorted Caustics) bằng lưới vân Voronoi sắc sảo
                float2 causticsUv1 = input.worldPos.xz * _NoiseScale * 0.06 + blendedNormalMap.xy * 0.12 + float2(_Time.y * 0.04, _Time.y * 0.02);
                float2 causticsUv2 = input.worldPos.xz * _NoiseScale * 0.082 - blendedNormalMap.xy * 0.1 + float2(_Time.y * -0.03, _Time.y * 0.05);
                float noiseVal1 = _CausticsMap.Sample(sampler_LinearRepeat, causticsUv1).r;
                float noiseVal2 = _CausticsMap.Sample(sampler_LinearRepeat, causticsUv2).r;
                float caustics = noiseVal1 * noiseVal2;
                
                // Lũy thừa pow như Shader Graph để tạo viền vân nắng siêu mảnh, sắc và long lanh
                float causticsMask = pow(caustics, _CausticsPower) * _CausticsIntensity;
                float causticsFade = smoothstep(0.08, 0.35, depthDiff);
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
