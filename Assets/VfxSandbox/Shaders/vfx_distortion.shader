Shader "VFX/ScreenDistortion"
{
    Properties
    {
        _DistortionMap("Distortion Map (Normal)", 2D) = "bump" {}
        _DistortionStrength("Distortion Strength", Float) = 0.08
        _DistortionSpeed("Distortion Speed (X, Y)", Vector) = (0, 0, 0, 0)
        _ColorTint("Color Tint (Emissive)", Color) = (1, 0.4, 0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "DistortionPass"
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
                float4 screenPos    : TEXCOORD1;
            };

            Texture2D _DistortionMap;
            SamplerState sampler_DistortionMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _DistortionMap_ST;
                float _DistortionStrength;
                float2 _DistortionSpeed;
                float4 _ColorTint;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Normalize screen coordinates
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // Sample distortion normal map with flow speed
                float2 offsetUV = _DistortionSpeed * _Time.y;
                float2 distUV = input.uv * _DistortionMap_ST.xy + _DistortionMap_ST.zw + offsetUV;
                float4 normalSample = _DistortionMap.Sample(sampler_DistortionMap, distUV);

                // Convert normal map sample [0, 1] to [-1, 1] offsets
                float2 offset = (normalSample.rg * 2.0 - 1.0) * _DistortionStrength;

                // Grab screen color with offset
                float3 screenColor = SampleSceneColor(screenUV + offset);

                // Add subtle color tint based on distortion mask alpha channel
                float alpha = normalSample.a * _ColorTint.a;
                float3 finalColor = lerp(screenColor, _ColorTint.rgb * 1.5, alpha);

                // Chỉ hiện khung hình ở những nơi có hoa văn bẻ cong, nền còn lại trong suốt hoàn toàn
                float finalAlpha = step(0.01, alpha);
                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
