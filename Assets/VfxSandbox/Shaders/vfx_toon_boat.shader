Shader "VFX/ToonBoat"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.35, 0.20, 0.10, 1.0)
        _ShadowColor("Shadow Tint Color", Color) = (0.12, 0.06, 0.08, 1.0)
        
        [Header(Toon Shading)]
        _ToonStep1("Toon Step 1 (Shadow-Mid)", Range(-1.0, 1.0)) = -0.15
        _ToonStep2("Toon Step 2 (Mid-Light)", Range(-1.0, 1.0)) = 0.15
        _ToonFeather("Toon Edge Smoothness", Range(0.001, 0.2)) = 0.03
        
        [Header(Comic Hatching)]
        _HatchDensity("UV Hatch Density", Range(0.5, 5.0)) = 1.8
        _HatchStrength("Hatch Line Intensity", Range(0.0, 1.0)) = 0.45
        
        [Header(Procedural Wood Grain)]
        _WoodGrainIntensity("Wood Grain Intensity", Range(0.0, 1.0)) = 0.75
        _WoodGrainTiling("Wood Grain Tiling", Range(5.0, 80.0)) = 32.0
        _WoodGrainWiggle("Wood Grain Wavy distortion", Range(0.5, 8.0)) = 3.0
        
        [Header(Vertical Ambient Gradient)]
        _TopColorTint("Top Color Tint (Bright)", Color) = (1.15, 1.15, 1.1, 1.0)
        _BottomColorTint("Bottom Color Tint (Dark)", Color) = (0.55, 0.6, 0.68, 1.0)
        _GradientMinY("Gradient Min Relative Y", Float) = -0.3
        _GradientMaxY("Gradient Max Relative Y", Float) = 0.8
        
        [Header(Stylized Specular)]
        _SpecularColor("Specular Color", Color) = (1.0, 0.95, 0.85, 1.0)
        _SpecularSize("Specular Size", Range(0.001, 0.5)) = 0.04
        _SpecularGloss("Specular Glossiness", Range(2.0, 128.0)) = 32.0
        
        [Header(Painterly Rim Light)]
        _RimColor("Rim Light Color", Color) = (1.0, 0.98, 0.92, 1.0)
        _RimPower("Rim Width", Range(0.5, 8.0)) = 3.5
        _RimThreshold("Rim Threshold", Range(0.0, 1.0)) = 0.45
        
        [Header(Waterline Ambient Tint)]
        _WaterlineColor("Waterline Tint Color", Color) = (0.05, 0.18, 0.22, 1.0)
        _WaterlineOffset("Waterline Relative Y", Range(-1.0, 1.0)) = -0.15
        _WaterlineGrad("Waterline Fade Range", Range(0.01, 1.0)) = 0.15
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

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
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            // Uniform parameters
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half _ToonStep1;
                half _ToonStep2;
                half _ToonFeather;
                half _HatchDensity;
                half _HatchStrength;
                half _WoodGrainIntensity;
                half _WoodGrainTiling;
                half _WoodGrainWiggle;
                half4 _TopColorTint;
                half4 _BottomColorTint;
                float _GradientMinY;
                float _GradientMaxY;
                half4 _SpecularColor;
                half _SpecularSize;
                half _SpecularGloss;
                half4 _RimColor;
                half _RimPower;
                half _RimThreshold;
                half4 _WaterlineColor;
                half _WaterlineOffset;
                half _WaterlineGrad;
            CBUFFER_END

            // Global shader variable populated by C# BoatFloating.cs script
            float _BoatWorldY;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                Light mainLight = GetMainLight();
                float3 lightDirWS = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;

                half NdotL = dot(normalWS, lightDirWS);

                // 1. Phân chia 3 vùng bóng rẽ (Tri-tone Step Shading) mượt mà bằng smoothstep
                half toneShadow = smoothstep(_ToonStep1, _ToonStep1 + _ToonFeather, NdotL);
                half toneMid = smoothstep(_ToonStep2, _ToonStep2 + _ToonFeather, NdotL);
                half toonDiffuse = saturate((toneShadow + toneMid) * 0.5);

                // 2. Tạo vân gỗ vẽ tay nghệ thuật dạng thủ tục (Procedural Stylized Wood Grain)
                float woodWiggle = sin(input.uv.x * 6.0) * _WoodGrainWiggle;
                float woodLine = sin(input.uv.y * _WoodGrainTiling + woodWiggle) * 0.5 + 0.5;
                float woodGrainMask = smoothstep(0.72, 0.82, woodLine) * _WoodGrainIntensity;
                half3 baseColorWithGrain = lerp(_BaseColor.rgb, _BaseColor.rgb * 0.60, woodGrainMask);

                // Nội suy màu sắc thân gỗ/buồm giữa màu sáng và màu tối
                half3 diffuseColor = lerp(_ShadowColor.rgb, baseColorWithGrain, toonDiffuse);

                // Tính toán độ cao thực tế của đỉnh so với trọng tâm thế giới của tàu
                // Giải quyết 100% lỗi lệch trục Blender (Z-up) sang Unity (Y-up) ở Object Space
                float relativeY = input.positionWS.y - _BoatWorldY;

                // 3. Gradient sáng tối dọc thân (Vertical Height Ambient Gradient)
                float vGradient = saturate((relativeY - _GradientMinY) / max(0.001, _GradientMaxY - _GradientMinY));
                half3 verticalAmbient = lerp(_BottomColorTint.rgb, _TopColorTint.rgb, vGradient);
                diffuseColor *= verticalAmbient;

                // 4. Tích hợp hiệu ứng vẽ tranh Manga chéo (Cross-Hatching) trong vùng tối theo tọa độ UV
                float2 hatchUv = input.uv * _HatchDensity * 12.0;
                float hatchPattern1 = saturate(sin((hatchUv.x - hatchUv.y) * 1.5) * 6.0 - 4.5);
                float hatchPattern2 = saturate(sin((hatchUv.x + hatchUv.y) * 1.5) * 6.0 - 4.5);
                
                float singleHatch = hatchPattern1 * (1.0 - toneShadow);
                float crossHatch = max(hatchPattern1, hatchPattern2) * saturate(1.0 - toneShadow * 2.0);
                float finalHatch = max(singleHatch, crossHatch) * _HatchStrength;

                diffuseColor = lerp(diffuseColor, diffuseColor * 0.55, finalHatch);

                // 5. Hiệu ứng tô bóng nổi khối viền cạnh (Backlit Toon Rim Light)
                half fresnel = saturate(1.0 - dot(normalWS, viewDirWS));
                half rimWeight = pow(fresnel, _RimPower);
                half rimToon = smoothstep(_RimThreshold, _RimThreshold + _ToonFeather * 1.5, rimWeight);
                half rimLightMask = rimToon * saturate(0.2 - NdotL * 0.8);
                half3 rimLightColor = rimLightMask * _RimColor.rgb * _RimColor.a * 0.65;

                // 6. Phản chiếu Toon Specular Highlight
                float3 halfDir = normalize(lightDirWS + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specPower = pow(NdotH, _SpecularGloss);
                half specToon = smoothstep(1.0 - _SpecularSize, 1.0 - _SpecularSize + _ToonFeather, specPower);
                half3 specularHighlight = specToon * _SpecularColor.rgb * _SpecularColor.a;

                // 7. Gradient nhuộm ẩm chân thực (Height-based Waterline Tint với viền sóng gợn nhẹ)
                float waterlineNoise = sin(input.uv.x * 16.0) * 0.03;
                float heightFactor = saturate(1.0 - (relativeY - _WaterlineOffset + waterlineNoise) / _WaterlineGrad);
                diffuseColor = lerp(diffuseColor, _WaterlineColor.rgb, heightFactor * _WaterlineColor.a);

                // 8. Tổng hợp màu sắc đầu ra hoàn chỉnh
                half3 finalColor = diffuseColor * lightColor; // Chiếu sáng
                finalColor += specularHighlight;              // Highlight
                finalColor += rimLightColor;                  // Rim light viền ngoài

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
