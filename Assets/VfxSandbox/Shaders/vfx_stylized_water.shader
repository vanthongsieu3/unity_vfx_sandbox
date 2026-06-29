Shader "VFX/StylizedWater"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor("Shallow Color", Color) = (0.0, 0.8, 0.75, 0.6)  // Màu xanh lam ngọc nông
        _DeepColor("Deep Color", Color) = (0.02, 0.12, 0.35, 0.95)       // Màu xanh đại dương sâu
        _WaterOpacity("Base Opacity", Range(0, 1)) = 0.5                  // Độ trong suốt cơ bản của nước
        _DepthMaxDistance("Depth Color Blending Distance", Float) = 4.0   // Khoảng cách chuyển màu nông/sâu

        [Header(Shoreline Foam)]
        _FoamColor("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamDistance("Foam Width", Float) = 0.35                         // Độ rộng bọt sóng xô bờ
        _FoamNoiseScale("Foam Noise Scale", Float) = 3.0                  // Tỉ lệ nhiễu tạo hình bọt sóng

        [Header(Procedural Gerstner Waves)]
        _WaveHeight("Wave Height", Float) = 0.18                          // Chiều cao nhấp nhô của sóng
        _WaveScale("Wave Scale/Frequency", Float) = 1.2                   // Tần số sóng
        _WaveSpeed("Wave Speed", Float) = 2.0                             // Tốc độ sóng

        [Header(Shimmering Caustics)]
        _NoiseMap("Seamless Noise Map", 2D) = "gray" {}
        _NoiseScale("Caustics Scale", Float) = 4.0
        _FlowSpeed("Flow Speed (X, Y)", Vector) = (0.05, 0.03, 0, 0)
        _CausticsColor("Caustics Color", Color) = (0.7, 1.0, 0.95, 1.0)   // Màu vân sóng nắng
        _CausticsCutoff("Caustics Cutoff", Range(0.1, 0.6)) = 0.28        // Độ sắc nét vân nắng
        _CausticsIntensity("Caustics Intensity", Float) = 1.5

        [Header(Sun Specular Reflection)]
        _Glossiness("Specular Glossiness", Float) = 120.0
        _SpecularIntensity("Specular Intensity", Float) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 300
        Blend SrcAlpha OneMinusSrcAlpha // Hòa trộn trong suốt tiêu chuẩn
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
            };

            Texture2D _NoiseMap;
            SamplerState sampler_NoiseMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _WaterOpacity;
                float _DepthMaxDistance;
                float4 _FoamColor;
                float _FoamDistance;
                float _FoamNoiseScale;
                float _WaveHeight;
                float _WaveScale;
                float _WaveSpeed;
                float4 _NoiseMap_ST;
                float _NoiseScale;
                float2 _FlowSpeed;
                float4 _CausticsColor;
                float _CausticsCutoff;
                float _CausticsIntensity;
                float _Glossiness;
                float _SpecularIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Tính toán vị trí thế giới của đỉnh nước (World Position)
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 1. Phép dựng sóng Gerstner giải tích cực nhanh trên Mobile (không đọc texture)
                float k1 = _WaveScale;
                float w1 = _Time.y * _WaveSpeed;
                float wave1 = sin(positionWS.x * k1 + w1) * _WaveHeight;

                float k2 = _WaveScale * 1.35;
                float w2 = _Time.y * _WaveSpeed * 1.15;
                float wave2 = cos(positionWS.z * k2 + w2) * (_WaveHeight * 0.55);

                positionWS.y += wave1 + wave2;

                // 2. Tính toán Vector Pháp tuyến (Normal Vector) chính xác dựa trên đạo hàm sóng
                // dy/dx của wave1 = k1 * cos(x * k1 + w1) * _WaveHeight
                // dy/dz of wave2 = -k2 * sin(z * k2 + w2) * _WaveHeight * 0.55
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
                // Tính toán tọa độ màn hình (Screen UV) để đọc Depth & Opaque Color
                float2 screenUv = input.positionCS.xy / _ScreenParams.xy;

                // 1. Tính toán chênh lệch độ sâu (Depth Blending)
                float rawDepth = SampleSceneDepth(screenUv);
                float sceneZ = LinearEyeDepth(rawDepth, _ZBufferParams);
                float screenZ = input.positionCS.w;
                float depthDiff = sceneZ - screenZ;
                float depthFactor = saturate(depthDiff / _DepthMaxDistance);

                // 2. Hòa trộn màu nước nông/sâu và chụp ảnh nền (Refraction)
                float3 shallowColor = _ShallowColor.rgb;
                float3 deepColor = _DeepColor.rgb;
                float3 waterBaseColor = lerp(shallowColor, deepColor, depthFactor);

                // Đọc ảnh nền phía sau để tạo độ trong suốt tự nhiên của nước
                float3 sceneColor = SampleSceneColor(screenUv);
                float opacity = lerp(_WaterOpacity, _DeepColor.a, depthFactor);
                float3 finalWaterColor = lerp(sceneColor, waterBaseColor, opacity);

                // 3. Hiệu ứng bọt nước xô bờ (Shoreline Foam)
                // Phối hợp nhiễu sóng cuộn để bọt xô bờ nhấp nhô cách điệu
                float2 foamUv = input.worldPos.xz * _FoamNoiseScale + float2(_Time.y * 0.3, _Time.y * 0.15);
                float foamNoise = _NoiseMap.Sample(sampler_NoiseMap, foamUv).r;
                float foamFactor = saturate(1.0 - depthDiff / _FoamDistance);
                float foamMask = step(foamNoise * 0.6, foamFactor); // Cắt biên bọt sóng sắc nét
                
                finalWaterColor = lerp(finalWaterColor, _FoamColor.rgb, foamMask * _FoamColor.a);

                // 4. Hiệu ứng phản chiếu vân sóng nắng (Shimmering Caustics)
                // Lấy chéo 2 mẫu nhiễu cuộn ngược hướng để tạo vân nước chuyển động ngẫu nhiên
                float2 uv1 = input.worldPos.xz * _NoiseScale * 0.05 + _FlowSpeed * _Time.y;
                float2 uv2 = input.worldPos.xz * _NoiseScale * 0.065 - _FlowSpeed * _Time.y * 0.75;
                float noise1 = _NoiseMap.Sample(sampler_NoiseMap, uv1).r;
                float noise2 = _NoiseMap.Sample(sampler_NoiseMap, uv2).r;
                float caustics = noise1 * noise2;
                float causticsMask = smoothstep(_CausticsCutoff, _CausticsCutoff + 0.12, caustics);

                // Không hiển thị vân caustics sát mép nước nông
                float causticsFade = smoothstep(0.1, 0.4, depthDiff);
                finalWaterColor += causticsMask * _CausticsColor.rgb * _CausticsIntensity * causticsFade;

                // 5. Tính toán phản xạ mặt trời chói mắt (Stylized Specular Reflection)
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 viewDir = SafeNormalize(input.viewDirWS);
                float3 halfDir = SafeNormalize(lightDir + viewDir);
                
                float NdotH = saturate(dot(input.worldNormal, halfDir));
                float specular = pow(NdotH, _Glossiness) * _SpecularIntensity;
                float3 specColor = mainLight.color * specular;

                finalWaterColor += specColor * (1.0 - foamMask); // Phản chiếu xuất hiện trên diện tích nước không có bọt

                return float4(finalWaterColor, 1.0);
            }
            ENDHLSL
        }
    }
}
