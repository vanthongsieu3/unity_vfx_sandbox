Shader "VFX/ToonBoat"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.35, 0.20, 0.10, 1.0)
        _ShadowColor("Shadow Tint Color", Color) = (0.12, 0.06, 0.08, 1.0)
        
        [Header(Toon Shading)]
        _ToonStep1("Toon Step 1 (Shadow-Mid)", Range(-1.0, 1.0)) = -0.15
        _ToonStep2("Toon Step 2 (Mid-Light)", Range(-1.0, 1.0)) = 0.2
        _ToonFeather("Toon Edge Smoothness", Range(0.001, 0.2)) = 0.02
        
        [Header(Comic Hatching)]
        _HatchDensity("Screen Hatch Density", Range(0.1, 2.0)) = 0.65
        _HatchStrength("Hatch Line Intensity", Range(0.0, 1.0)) = 0.55
        
        [Header(Stylized Specular)]
        _SpecularColor("Specular Color", Color) = (1.0, 0.95, 0.85, 1.0)
        _SpecularSize("Specular Size", Range(0.001, 0.5)) = 0.05
        _SpecularGloss("Specular Glossiness", Range(2.0, 128.0)) = 32.0
        
        [Header(Painterly Rim Light)]
        _RimColor("Rim Light Color", Color) = (1.0, 0.98, 0.92, 1.0)
        _RimPower("Rim Width", Range(0.5, 8.0)) = 3.0
        _RimThreshold("Rim Threshold", Range(0.0, 1.0)) = 0.4
        
        [Header(Waterline Ambient Tint)]
        _WaterlineColor("Waterline Tint Color", Color) = (0.05, 0.18, 0.22, 1.0)
        _WaterlineOffset("Waterline Local Y", Range(-1.0, 1.0)) = -0.05
        _WaterlineGrad("Waterline Fade Range", Range(0.01, 1.0)) = 0.35
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
                float localY        : TEXCOORD3;
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Chuyển đổi tọa độ đỉnh sang các không gian
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                // Chuyển đổi vector pháp tuyến sang không gian thế giới
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.localY = input.positionOS.y;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Chuẩn hóa vector pháp tuyến và vector nhìn
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // Lấy thông tin nguồn sáng chính từ URP
                Light mainLight = GetMainLight();
                float3 lightDirWS = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;

                // Tính toán thành phần khuếch tán Lambertian cơ bản
                half NdotL = dot(normalWS, lightDirWS);

                // 1. Phân chia 3 vùng bóng rẽ (Tri-tone Step Shading) mượt mà bằng smoothstep
                half toneShadow = smoothstep(_ToonStep1, _ToonStep1 + _ToonFeather, NdotL);
                half toneMid = smoothstep(_ToonStep2, _ToonStep2 + _ToonFeather, NdotL);
                
                // Tổng hợp hệ số chiếu sáng khuếch tán toon
                half toonDiffuse = saturate((toneShadow + toneMid) * 0.5);

                // Nội suy màu sắc thân gỗ/buồm giữa màu sáng và màu tối
                half3 diffuseColor = lerp(_ShadowColor.rgb, _BaseColor.rgb, toonDiffuse);

                // 2. Tích hợp hiệu ứng vẽ tranh Manga chéo màn hình (Cross-Hatching) trong vùng tối
                // Hatching tạo ra chất cổ điển hoàn toàn khác biệt với nét gradient phẳng của Genshin
                float2 screenPos = input.positionCS.xy * _HatchDensity * 0.08;
                
                // Vẽ các nét kẻ chéo góc xiên
                float hatchPattern1 = saturate(sin((screenPos.x - screenPos.y) * 2.0) * 12.0 - 10.0);
                float hatchPattern2 = saturate(sin((screenPos.x + screenPos.y) * 2.0) * 12.0 - 10.0);
                
                // Trộn 2 hướng kẻ: bóng nhạt vẽ nét đơn, bóng đậm vẽ nét đôi chéo nhau (cross-hatch)
                float singleHatch = hatchPattern1 * (1.0 - toneShadow);
                float crossHatch = max(hatchPattern1, hatchPattern2) * saturate(1.0 - toneShadow * 2.0);
                float finalHatch = max(singleHatch, crossHatch) * _HatchStrength;

                // Khắc vạch tối vào màu khuếch tán vùng tối
                diffuseColor = lerp(diffuseColor, diffuseColor * 0.45, finalHatch);

                // 3. Hiệu ứng tô bóng nổi khối viền cạnh (Fresnel Toon Rim Light)
                half fresnel = saturate(1.0 - dot(normalWS, viewDirWS));
                half rimWeight = pow(fresnel, _RimPower);
                // Rim light cũng được cắt sắc nét theo phong cách vẽ tay
                half rimToon = smoothstep(_RimThreshold, _RimThreshold + _ToonFeather, rimWeight);
                half3 rimLightColor = rimToon * _RimColor.rgb * _RimColor.a;

                // 4. Phản chiếu Toon Specular Highlight
                float3 halfDir = normalize(lightDirWS + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specPower = pow(NdotH, _SpecularGloss);
                // Cắt nhọn specular thành vệt sáng nhỏ óng ánh nghệ thuật
                half specToon = smoothstep(1.0 - _SpecularSize, 1.0 - _SpecularSize + _ToonFeather, specPower);
                half3 specularHighlight = specToon * _SpecularColor.rgb * _SpecularColor.a;

                // 5. Gradient nhuộm ẩm chân thực (Height-based Waterline Tint)
                // Tạo cảm giác chân thuyền tiếp giáp nước có lớp rêu mốc/nước ngấm đậm đà
                float heightFactor = saturate(1.0 - (input.localY - _WaterlineOffset) / _WaterlineGrad);
                diffuseColor = lerp(diffuseColor, diffuseColor * _WaterlineColor.rgb, heightFactor * _WaterlineColor.a * 0.9);

                // 6. Tổng hợp màu sắc đầu ra hoàn chỉnh
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
