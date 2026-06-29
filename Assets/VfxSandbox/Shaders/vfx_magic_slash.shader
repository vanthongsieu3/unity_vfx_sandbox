Shader "VFX/MagicSlash"
{
    Properties
    {
        [MainColor] _ColorTint("Color Tint (Edges)", Color) = (0.75, 0.0, 1.0, 1) // Màu tím hồng rìa ngoài
        _VoidColor("Void Color (Core)", Color) = (0.05, 0.0, 0.15, 1)        // Màu lõi đen hư vô
        _CoreColor("Core Color (Blade)", Color) = (0.0, 0.95, 1.0, 1)         // Màu đường cắt chớp lam/cyan
        _NoiseMap("Noise Map", 2D) = "white" {}
        _RampMap("Color Ramp", 2D) = "white" {}
        _ScrollSpeed("Scroll Speed (X, Y)", Vector) = (-1.5, 0.5, 0, 0)
        _ScrollSpeed2("Scroll Speed 2 (Distortion)", Vector) = (1.2, -0.6, 0, 0)
        _DistortionStrength("Distortion Strength", Range(0, 0.5)) = 0.16
        _Intensity("Glow Intensity", Float) = 4.5
        _Swipe("Swipe Progress", Range(0, 1.5)) = 0.0
        _TailLength("Tail Length", Range(0.1, 0.8)) = 0.45
        _Opacity("Opacity", Range(0, 1)) = 1.0

        // Thông số cấu hình sóng răng cưa ma thuật (Kassadin saw-tooth waves)
        _WaveCount("Wave Count", Float) = 4.0
        _WaveSpeed("Wave Speed", Float) = -8.0
        _WaveAmplitude("Wave Amplitude", Range(0, 0.4)) = 0.14
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        Blend One Zero // Ghi đè trực tiếp vì đã tự hòa trộn màu nền qua camera opaque texture
        ZWrite Off
        Cull Off

        Pass
        {
            Name "MagicSlashPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            Texture2D _NoiseMap;
            SamplerState sampler_NoiseMap;
            Texture2D _RampMap;
            SamplerState sampler_RampMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorTint;
                float4 _VoidColor;
                float4 _CoreColor;
                float4 _NoiseMap_ST;
                float2 _ScrollSpeed;
                float2 _ScrollSpeed2;
                float _DistortionStrength;
                float _Intensity;
                float _Swipe;
                float _TailLength;
                float _Opacity;
                float _WaveCount;
                float _WaveSpeed;
                float _WaveAmplitude;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Tính toán tọa độ màn hình (Screen UV) để mẫu màu nền phía sau
                float2 screenUv = input.positionCS.xy / _ScreenParams.xy;

                // 1. Tính toán mặt nạ quét chém trăng khuyết (Crescent swipe mask)
                float leadMask = smoothstep(_Swipe, _Swipe - 0.03, input.uv.x);
                float trailMask = smoothstep(_Swipe - _TailLength, _Swipe - _TailLength + 0.12, input.uv.x);
                float swipeMask = leadMask * trailMask;

                // 2. Tính toán độ co hẹp của đuôi chém (Tapering tail)
                float distFromHead = clamp(_Swipe - input.uv.x, 0.0, 1.0);
                float thickness = saturate(1.0 - distFromHead / _TailLength);
                
                float borderOffset = 0.12 * (2.0 - thickness);
                float edgeFade = smoothstep(0.0, borderOffset, input.uv.y) * smoothstep(1.0, 1.0 - borderOffset, input.uv.y);
                float startFade = smoothstep(0.0, 0.06, input.uv.x);

                float tearMask = swipeMask * edgeFade * startFade * _Opacity;

                // 3. Nhiễu biến dạng UV (Noise-on-Noise UV Distortion)
                float2 offset2 = _ScrollSpeed2 * _Time.y;
                float2 distUv = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw + offset2;
                float distNoise = _NoiseMap.Sample(sampler_NoiseMap, distUv).r;

                // Bẻ cong không gian cục bộ xung quanh vệt kiếm
                float2 distOffset = float2(distNoise - 0.5, (1.0 - distNoise) - 0.5) * 0.09 * tearMask;
                float3 sceneColor = SampleSceneColor(screenUv + distOffset);

                // 4. Sinh Sóng Răng Cưa Ma Thuật (Procedural Zig-Zag / Sawtooth Wave)
                // Lập trình sóng tam giác chuyển động dọc theo chiều dài cung chém (UV.x)
                float wave1 = abs(frac(input.uv.x * _WaveCount + _Time.y * _WaveSpeed) - 0.5) * 2.0;
                float wave2 = abs(frac(input.uv.x * (_WaveCount * 1.65) + _Time.y * (_WaveSpeed * 1.35) + 0.35) - 0.5) * 2.0;

                // Dùng sóng để xê dịch trọng tâm chiều rộng cung chém (UV.y) tạo nét răng cưa sắc nhọn
                float yOffset1 = (wave1 - 0.5) * _WaveAmplitude;
                float yOffset2 = (wave2 - 0.5) * (_WaveAmplitude * 0.55);

                float distCenter1 = abs(input.uv.y - 0.5 + yOffset1);
                float distCenter2 = abs(input.uv.y - 0.5 + yOffset2);

                // Tạo các đường chớp năng lượng răng cưa sắc nét
                float energyWave = smoothstep(0.18, 0.0, distCenter1);
                float coreWave = smoothstep(0.06, 0.0, distCenter2);

                // 5. Đường lưỡi kiếm sắc bén rực sáng ở mũi chém (Sharp Glowing Blade Edge)
                float edgeLine = smoothstep(_Swipe - 0.06, _Swipe - 0.01, input.uv.x) * leadMask;
                edgeLine = pow(edgeLine, 2.0);

                // 6. Lõi hố đen không gian (Cosmic Void Rift Center)
                float riftCenter = smoothstep(borderOffset * 1.5, borderOffset * 2.8, input.uv.y) * 
                                   smoothstep(1.0 - borderOffset * 1.5, 1.0 - borderOffset * 2.8, input.uv.y);
                riftCenter *= swipeMask * startFade * _Opacity;
                float3 voidColor = _VoidColor.rgb * _Intensity; 

                // 7. Nhiễu lửa nền lấy từ Color Ramp cuộn xoáy
                float2 noiseUv = (input.uv + float2(distNoise - 0.5, 0.0) * 0.08) * _NoiseMap_ST.xy + _ScrollSpeed * _Time.y;
                float noiseVal = _NoiseMap.Sample(sampler_NoiseMap, noiseUv).r;
                float4 rampColor = _RampMap.Sample(sampler_RampMap, float2(noiseVal, 0.5));
                float3 energyBase = rampColor.rgb * _Intensity;

                // 8. Hòa trộn tổng hợp màu sắc (Refraction background + Void core + Stylized Zig-zag + Glowing Core)
                float3 finalColor = sceneColor; // Lớp nền biến dạng khúc xạ
                finalColor = lerp(finalColor, voidColor, riftCenter * 0.95); // Đè lõi tím đen vũ trụ tối lên center

                // Màu chớp răng cưa chuyển đổi từ tím hồng sang xanh lam rực rỡ
                float3 waveColor = lerp(_ColorTint.rgb, _CoreColor.rgb, coreWave);
                float3 stylizedEnergy = (energyWave * 0.85 + coreWave * 2.6) * waveColor * _Intensity;
                
                // Viền trắng nóng rực chém cắt ở mũi chém
                float3 edgeGlow = float3(1.0, 1.0, 1.0) * _Intensity * 3.0;
                stylizedEnergy = lerp(stylizedEnergy, edgeGlow, edgeLine * 0.85);

                // Hòa vào năng lượng
                finalColor += (energyBase * 0.35 + stylizedEnergy) * tearMask;

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
