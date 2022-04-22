Shader "CustomRenderTexture/SpaceFog_DV"
{
    Properties
    {
        _DensityFunctionMultiplier("Density Function Multiplier", Float) = 100.0
        //_Dimension("Dimension", Float) = 256.0
        _Color("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
       Lighting Off
       Blend One Zero
       Pass
       {
           CGPROGRAM
           #include "UnityCustomRenderTexture.cginc"
           #pragma vertex CustomRenderTextureVertexShader
           #pragma fragment frag
           #pragma target 3.0
           float4      _Color;
           //float _Dimension;
           float _DensityFunctionMultiplier;



           float DensityFunction(float3 position)
           {
               float distFromCenter = length(position.xz - 0.5);

               return (0.7 - distFromCenter * distFromCenter) - position.y;

           }




           float4 frag(v2f_customrendertexture IN) : COLOR
           {
               //float3 distFromCent = IN.globalTexcoord.xyz - 0.5;
               //float rad = length(distFromCent);
               //rad = saturate(1.0 - floor(rad*3.0));

               float output = DensityFunction(IN.globalTexcoord.xyz);

               return float4(_Color.rgb, _DensityFunctionMultiplier * output);
           }
           ENDCG
       }
    }
} 