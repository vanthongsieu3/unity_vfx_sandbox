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
        Blend SrcAlpha One // Cộng sáng (Additive)
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
                // 1. Tính toán mặt nạ quét chém trăng khuyết (Crescent swipe mask)
                // Hạn chế chiều rộng vệt chém chỉ nằm từ đầu chém đến đuôi chém
                float leadMask = smoothstep(_Swipe, _Swipe - 0.05, input.uv.x);
                float trailMask = smoothstep(_Swipe - _TailLength, _Swipe - _TailLength + 0.15, input.uv.x);
                float swipeMask = leadMask * trailMask;

                // 2. Tính toán độ co hẹp của đuôi chém (Tapering tail)
                // Càng xa đầu chém (U gần _Swipe), độ dày kiếm càng nhỏ lại
                float distFromHead = clamp(_Swipe - input.uv.x, 0.0, 1.0);
                float thickness = saturate(1.0 - distFromHead / _TailLength);
                
                // Mép biên bo tròn co hẹp dần về phía đuôi chém
                float borderOffset = 0.12 * (2.0 - thickness);
                float edgeFade = smoothstep(0.0, borderOffset, input.uv.y) * smoothstep(1.0, 1.0 - borderOffset, input.uv.y);
                float startFade = smoothstep(0.0, 0.08, input.uv.x);

                // 3. Tính toán dịch chuyển UV và mẫu Noise
                float2 offset = _ScrollSpeed * _Time.y;
                float2 noiseUv = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw + offset;
                float noise = _NoiseMap.Sample(sampler_NoiseMap, noiseUv).r;

                // 4. Mẫu màu từ Color Ramp sử dụng Noise
                float4 rampColor = _RampMap.Sample(sampler_RampMap, float2(noise, 0.5));

                // Tính toán màu và alpha cuối cùng
                float3 finalColor = rampColor.rgb * _ColorTint.rgb * _Intensity;
                float alpha = noise * swipeMask * edgeFade * startFade * _Opacity;

                return float4(finalColor * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
