Shader "VFX/StylizedWater"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor("Shallow Color", Color) = (0.0, 0.8, 0.75, 0.6)  // Màu xanh lam ngọc nông
        _DeepColor("Deep Color", Color) = (0.02, 0.12, 0.35, 0.95)       // Màu xanh đại dương sâu
        _WaterOpacity("Base Opacity", Range(0, 1)) = 0.5                  // Độ trong suốt cơ bản của nước
        _DepthMaxDistance("Depth Color Blending Distance", Float) = 4.0   // Khoảng cách chuyển màu nông/sâu

        [Header(Shoreline and Wave Crest Foam)]
        _FoamColor("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamDistance("Shore Foam Width", Float) = 0.55                   // Độ rộng bọt sóng xô bờ
        _FoamNoiseScale("Foam Noise Scale", Float) = 3.5                  // Tỉ lệ nhiễu bọt sóng
        _FoamNoiseWeight("Foam Edge Noise Distortion", Range(0.1, 0.8)) = 0.45 // Độ lồi lõm của mép bọt sóng
        _WaveCrestThreshold("Wave Crest Foam Threshold", Float) = 0.12    // Điểm cao của sóng bắt đầu sinh bọt đỉnh
        _WaveCrestRange("Wave Crest Foam Range", Float) = 0.15            // Dải chuyển tiếp bọt đỉnh sóng

        [Header(Normal Map Ripples)]
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale1("Normal Scale 1", Float) = 0.05
        _NormalScale2("Normal Scale 2", Float) = 0.08
        _NormalSpeed1("Normal Speed 1 (X, Y)", Vector) = (0.05, 0.02, 0, 0)
        _NormalSpeed2("Normal Speed 2 (X, Y)", Vector) = (-0.03, 0.04, 0, 0)
        _RefractionStrength("Refraction Distortion Strength", Float) = 0.12 // Độ khúc xạ biến dạng đáy nước

        [Header(Procedural Gerstner Waves)]
        _WaveHeight("Wave Height", Float) = 0.22                          // Chiều cao nhấp nhô của sóng
        _WaveScale("Wave Scale/Frequency", Float) = 0.85                  // Tần số sóng
        _WaveSpeed("Wave Speed", Float) = 1.6                             // Tốc độ sóng

        [Header(Shimmering Caustics)]
        _NoiseMap("Seamless Noise Map", 2D) = "gray" {}
        _NoiseScale("Caustics Scale", Float) = 6.0
        _CausticsColor("Caustics Color", Color) = (0.7, 1.0, 0.95, 1.0)   // Màu vân sóng nắng
        _CausticsCutoff("Caustics Cutoff", Range(0.1, 0.6)) = 0.3         // Độ sắc nét vân nắng
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

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _WaterOpacity;
                float _DepthMaxDistance;
                float4 _FoamColor;
                float _FoamDistance;
                float _FoamNoiseScale;
                float _FoamNoiseWeight;
                float _WaveCrestThreshold;
                float _WaveCrestRange;
                float _NormalScale1;
                float _NormalScale2;
                float4 _NormalSpeed1;
                float4 _NormalSpeed2;
                float _RefractionStrength;
                float _WaveHeight;
                float _WaveScale;
                float _WaveSpeed;
                float4 _NoiseMap_ST;
                float _NoiseScale;
                float4 _CausticsColor;
                float _CausticsCutoff;
                float _CausticsIntensity;
                float4 _SkyColor;
                float _ReflectionStrength;
                float _Glossiness;
                float _SpecularIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Tính toán vị trí thế giới của đỉnh nước (World Position)
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 1. Phép dựng sóng Gerstner giải tích
                float k1 = _WaveScale;
                float w1 = _Time.y * _WaveSpeed;
                float wave1 = sin(positionWS.x * k1 + w1) * _WaveHeight;

                float k2 = _WaveScale * 1.35;
                float w2 = _Time.y * _WaveSpeed * 1.15;
                float wave2 = cos(positionWS.z * k2 + w2) * (_WaveHeight * 0.55);

                positionWS.y += wave1 + wave2;
                output.waveHeight = wave1 + wave2; // Truyền chiều cao sóng sang fragment

                // 2. Tính toán Vector Pháp tuyến (Normal Vector) dựa trên đạo hàm sóng
                float dy_dx = k1 * cos(positionWS.x * k1 + w1) * _WaveHeight;
                float dy_dz = -k2 * sin(positionWS.z * k2 + w2) * (_WaveHeight * 0.55);
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
                // Tính toán tọa độ màn hình (Screen UV)
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
                // Dùng thông số pháp tuyến chi tiết dịch chuyển tọa độ đọc ảnh nền
                float2 distort = blendedNormalMap.xy * _RefractionStrength * saturate(depthDiff * 1.5);
                float2 refractUv = screenUv + distort;

                // Kiểm tra an toàn để không vẽ vật cản phía trước đè lên nước
                float rawDepthDist = SampleSceneDepth(refractUv);
                float sceneZDist = LinearEyeDepth(rawDepthDist, _ZBufferParams);
                if (sceneZDist < screenZ)
                {
                    refractUv = screenUv; // Fallback nếu tọa độ khúc xạ lồi ra ngoài vật cản trước nước
                }
                float3 sceneColor = SampleSceneColor(refractUv);

                // 4. Hòa trộn màu nước nông/sâu dốc
                float3 shallowColor = _ShallowColor.rgb;
                float3 deepColor = _DeepColor.rgb;
                float3 waterBaseColor = lerp(shallowColor, deepColor, depthFactor);

                // 5. Phản chiếu Bầu Trời dựa trên góc nhìn Fresnel (Fresnel Reflection)
                // Góc nhìn càng nghiêng, nước càng phản chiếu bầu trời nhiều; góc vuông nhìn xuyên thấu xuống cát
                float3 viewDir = SafeNormalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(viewDir, worldNormal)), 4.0);
                float3 reflectedColor = lerp(waterBaseColor, _SkyColor.rgb, fresnel * _ReflectionStrength);

                float opacity = lerp(_WaterOpacity, _DeepColor.a, depthFactor);
                float3 finalWaterColor = lerp(sceneColor, reflectedColor, opacity);

                // 6. Tạo bọt nước xô bờ gợn sóng cách điệu (Stylized Shoreline Foam)
                float2 foamUv = input.worldPos.xz * _FoamNoiseScale + blendedNormalMap.xy * 0.15 + float2(_Time.y * 0.12, _Time.y * 0.06);
                float foamNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv).r;
                
                // A. Bọt xô bờ (Shoreline foam) - Sửa lỗi viền bọt răng cưa rách nát ở rìa không có đáy nước (Skybox)
                float shoreFoamMask = 0.0;
                if (sceneZ > screenZ) // Chỉ sinh bọt nếu có thực thể nằm DƯỚI mặt nước (tránh lỗi mép bầu trời)
                {
                    float shoreFoamFactor = saturate(1.0 - depthDiff / _FoamDistance);
                    shoreFoamMask = smoothstep(0.42, 0.48, shoreFoamFactor + foamNoise * _FoamNoiseWeight);
                }

                // B. Bọt đỉnh sóng (Wave Crest foam) - Phá vỡ các đường bọt song song dẹt thành các mảng bọt trôi nổi bằng phép nhân nhiễu
                float waveCrestFactor = saturate((input.waveHeight - _WaveCrestThreshold) / _WaveCrestRange);
                float waveCrestNoise = _NoiseMap.Sample(sampler_LinearRepeat, foamUv * 1.6).r;
                float waveCrestMask = smoothstep(0.48, 0.55, waveCrestFactor * waveCrestNoise * 2.3); // Nhân tỉ lệ xé nhỏ

                // Gộp chung hai loại bọt nước
                float foamCutout = max(shoreFoamMask, waveCrestMask);
                finalWaterColor = lerp(finalWaterColor, _FoamColor.rgb, foamCutout * _FoamColor.a);

                // 7. Tạo vân nắng lung linh khúc xạ (Distorted Caustics)
                // Dùng chính véc-tơ pháp tuyến bẻ cong UV vân nắng tạo hình rực sáng hữu cơ uốn lượn
                float2 causticsUv1 = input.worldPos.xz * _NoiseScale * 0.06 + blendedNormalMap.xy * 0.12 + float2(_Time.y * 0.04, _Time.y * 0.02);
                float2 causticsUv2 = input.worldPos.xz * _NoiseScale * 0.082 - blendedNormalMap.xy * 0.1 + float2(_Time.y * -0.03, _Time.y * 0.05);
                float noiseVal1 = _NoiseMap.Sample(sampler_LinearRepeat, causticsUv1).r;
                float noiseVal2 = _NoiseMap.Sample(sampler_LinearRepeat, causticsUv2).r;
                float caustics = noiseVal1 * noiseVal2;
                
                float causticsMask = smoothstep(_CausticsCutoff, _CausticsCutoff + 0.1, caustics);
                float causticsFade = smoothstep(0.08, 0.35, depthDiff); // Tắt vân nắng ở sát mép nước cực nông
                finalWaterColor += causticsMask * _CausticsColor.rgb * _CausticsIntensity * causticsFade * (1.0 - foamCutout);

                // 8. Tính toán phản xạ mặt trời chói mắt (Stylized Specular Highlight)
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
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
