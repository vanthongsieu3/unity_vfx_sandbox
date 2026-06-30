Shader "VFX/BoatStencilMask"
{
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent-1" // Vẽ ngay trước nước (Transparent) để ghi đè Stencil
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "StencilMask"
            Tags { "LightMode"="UniversalForward" }
            
            // Sử dụng Alpha Blending chuẩn và tắt ghi depth để làm trong suốt 100% trên mọi phiên bản URP
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off  // Tắt ghi Depth Buffer
            Cull Off    // Vẽ hai mặt
            
            Stencil
            {
                Ref 1
                Pass Replace // Ghi giá trị 1 vào Stencil Buffer
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Trả về màu hoàn toàn trong suốt (Alpha = 0)
                return half4(0.0, 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}
