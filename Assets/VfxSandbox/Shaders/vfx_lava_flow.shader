Shader "VFX/LavaFlow"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Rock Texture (RGBA)", 2D) = "white" {}
        _NoiseMap("Seamless Noise Map (Grayscale)", 2D) = "gray" {}
        _RampMap("Color Ramp (Color)", 2D) = "white" {}
        _FlowSpeed("Lava Flow Speed (X, Y)", Vector) = (0.25, 0.1, 0, 0)
        _LavaIntensity("Lava Glow Intensity", Float) = 2.0
        _DisplacementStrength("Vertex Displacement Strength", Float) = 0.15
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
            };

            Texture2D _BaseMap;
            Texture2D _NoiseMap;
            Texture2D _RampMap;
            SamplerState sampler_BaseMap;
            SamplerState sampler_NoiseMap;
            SamplerState sampler_RampMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _NoiseMap_ST;
                float2 _FlowSpeed;
                float _LavaIntensity;
                float _DisplacementStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Sample noise at vertex stage for displacement (simple fallback approximation using UV)
                float2 noiseUV = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw;
                float noiseVal = _NoiseMap.SampleLevel(sampler_NoiseMap, noiseUV, 0).r;
                
                // Displace vertex position along its normal
                float3 displacedPos = input.positionOS.xyz + input.normalOS * noiseVal * _DisplacementStrength;
                
                output.positionCS = TransformObjectToHClip(displacedPos);
                output.uv = input.uv;
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Base rock color
                float2 baseUV = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                float4 rockCol = _BaseMap.Sample(sampler_BaseMap, baseUV);

                // Lava flow noise panning
                float2 flowOffset = _FlowSpeed * _Time.y;
                float2 noiseUV = input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw + flowOffset;
                float noiseVal = _NoiseMap.Sample(sampler_NoiseMap, noiseUV).r;

                // Alternate overlapping layer of noise to avoid repetition pattern
                float2 noiseUV2 = input.uv * _NoiseMap_ST.xy * 1.5 + _NoiseMap_ST.zw - flowOffset * 0.7;
                float noiseVal2 = _NoiseMap.Sample(sampler_NoiseMap, noiseUV2).r;
                
                float combinedNoise = saturate(noiseVal * noiseVal2 * 2.0);

                // Sample Color Ramp based on noise value
                float4 lavaCol = _RampMap.Sample(sampler_RampMap, float2(combinedNoise, 0.5));
                
                // Mix base rock with glowing lava
                float4 finalColor = lerp(rockCol, lavaCol * _LavaIntensity, combinedNoise);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
