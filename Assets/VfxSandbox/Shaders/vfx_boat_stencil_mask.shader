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
            
            // Khóa hoàn toàn kênh màu bằng cả ColorMask 0 và cơ chế Blend Zero One
            ColorMask 0
            Blend Zero One
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
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
