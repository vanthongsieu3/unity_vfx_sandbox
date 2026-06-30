Shader "VFX/BoatStencilMask"
{
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry-1" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "StencilMask"
            
            ColorMask 0 // Tắt hoàn toàn việc ghi màu sắc ra màn hình
            ZWrite Off  // Tắt ghi Depth Buffer để không chặn các hình vẽ khác
            Cull Off    // Vẽ cả mặt trước và sau của khối mặt nạ
            
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
                // Trả về màu trống, thực tế ColorMask 0 sẽ chặn toàn bộ giá trị này ghi ra màn hình
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
