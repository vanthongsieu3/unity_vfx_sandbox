Shader "VFX/FireRing"
{
    Properties
    {
        _NoiseMap("Noise Map", 2D) = "white" {}
        _RampMap("Color Ramp", 2D) = "white" {}
        _ScrollSpeed("Scroll Speed (X, Y)", Vector) = (0.8, 0, 0, 0)
        _Intensity("Glow Intensity", Float) = 3.0
        _Opacity("Opacity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        Blend SrcAlpha One // Additive
        ZWrite Off
        Cull Off

        Pass
        {
            Name "FireRingPass"
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
                float4 _NoiseMap_ST;
                float2 _ScrollSpeed;
                float _Intensity;
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
                // Scroll UVs
                float2 offset = _ScrollSpeed * _Time.y;
                float2 uv = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw + offset;

                // Sample noise
                float noise = _NoiseMap.Sample(sampler_NoiseMap, uv).r;

                // Sample ramp color using noise
                float4 fireColor = _RampMap.Sample(sampler_RampMap, float2(noise, 0.5));

                // Fade out at the inner/outer edges of the UV.v (v goes from 0 to 1 across the ring width)
                float edgeFade = smoothstep(0.0, 0.15, input.uv.y) * smoothstep(1.0, 0.85, input.uv.y);

                float alpha = noise * _Opacity * edgeFade;
                float3 finalColor = fireColor.rgb * _Intensity;

                return float4(finalColor * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
