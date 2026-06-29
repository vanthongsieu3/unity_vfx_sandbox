Shader "VFX/MagicSlash"
{
    Properties
    {
        [MainColor] _ColorTint("Color Tint", Color) = (1, 0.5, 0.2, 1)
        _NoiseMap("Noise Map", 2D) = "white" {}
        _RampMap("Color Ramp", 2D) = "white" {}
        _ScrollSpeed("Scroll Speed (X, Y)", Vector) = (-1.5, 0.5, 0, 0)
        _Intensity("Glow Intensity", Float) = 4.0
        _Swipe("Swipe Progress", Range(0, 1.5)) = 0.0
        _TailLength("Tail Length", Range(0.1, 0.8)) = 0.45
        _Opacity("Opacity", Range(0, 1)) = 1.0
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
                float4 _NoiseMap_ST;
                float2 _ScrollSpeed;
                float _Intensity;
                float _Swipe;
                float _TailLength;
                float _Opacity;
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

                // 3. Tính toán dịch chuyển UV và mẫu Noise
                float2 offset = _ScrollSpeed * _Time.y;
                float2 noiseUv = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw + offset;
                float noise = _NoiseMap.Sample(sampler_NoiseMap, noiseUv).r;

                // 4. Biến dạng không gian (Spatial Tear Refraction)
                // Dùng nhiễu kết hợp mặt nạ vệt chém để bẻ cong không gian xung quanh vết rách
                float2 distOffset = float2(noise - 0.5, (1.0 - noise) - 0.5) * 0.09 * tearMask;
                float3 sceneColor = SampleSceneColor(screenUv + distOffset);

                // 5. Đường lưỡi kiếm sắc bén rực sáng ở mũi chém (Sharp Glowing Blade Edge)
                float edgeLine = smoothstep(_Swipe - 0.06, _Swipe - 0.01, input.uv.x) * leadMask;
                edgeLine = pow(edgeLine, 2.0);

                // 6. Lõi hố đen không gian (Cosmic Void Rift Center)
                // Tạo một khe nứt màu tím tối ở trung tâm vệt chém, thể hiện không gian bị rách toác
                float riftCenter = smoothstep(borderOffset * 1.5, borderOffset * 2.8, input.uv.y) * 
                                   smoothstep(1.0 - borderOffset * 1.5, 1.0 - borderOffset * 2.8, input.uv.y);
                riftCenter *= swipeMask * startFade * _Opacity;
                float3 voidColor = float3(0.02, 0.0, 0.08) * _Intensity; // Màu tím đen vũ trụ tối tăm

                // 7. Hòa trộn màu: Nền biến dạng + Lõi hố đen rách + Lửa ma thuật cuộn và Lưỡi sáng
                float4 rampColor = _RampMap.Sample(sampler_RampMap, float2(noise, 0.5));
                float3 energyColor = rampColor.rgb * _ColorTint.rgb * _Intensity;
                float3 coreColor = float3(1.0, 1.0, 1.0) * _Intensity * 2.8;

                // Hòa trộn từng lớp
                float3 finalColor = sceneColor; // Bắt đầu bằng ảnh nền đã biến dạng
                finalColor = lerp(finalColor, voidColor, riftCenter * 0.9); // Đè lõi hư vô đen tối lên trung tâm vết rách
                
                float3 glowColor = lerp(energyColor, coreColor, edgeLine * 0.75);
                finalColor += glowColor * tearMask; // Cộng sáng viền lửa ma thuật xung quanh vết rách

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
