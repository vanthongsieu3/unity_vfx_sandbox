Shader "VFX/BoatStencilMask"
{
    Properties
    {
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry-1" 
        }
        
        ColorMask 0 
        ZWrite Off  
        
        Pass
        {
            Stencil
            {
                Ref 1
                Pass Replace 
            }
        }
    }
}
