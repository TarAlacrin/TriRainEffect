Shader "Hidden/Shader/SpaceFogShader"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
	float _RayMarchStepSize;
    TEXTURE2D_X(_InputTexture);
	uniform float3 _WSScreenVerticalDirection;
	uniform float3 _WSScreenHorizontDirection;
	uniform float3 _WSCameraForward;



	float3 GetPixelRayDirectionWS(float2 uv)
	{
		float2 screenspace;
		screenspace = uv * 2.0 - 1.0;
		//screenspace.y = 1.0 - uv.y*2.0;
		float3 dir = _WSCameraForward + _WSScreenHorizontDirection* screenspace.x + _WSScreenVerticalDirection* screenspace.y;
		dir = normalize(dir);

		return dir;
	}

	float3 DoRayMarch(float sceneDepth, float2 uv, float3 sceneColor)
	{
		float3 wsDir = GetPixelRayDirectionWS(uv);
		float4 colToRet = 0;
		for (int i = 0; i < 64; i++)
		{
			float curDepth = _ProjectionParams.y + ((float)i) * _RayMarchStepSize;

			if (curDepth > sceneDepth)
			{
				return sceneColor;
			}
			
			float3 wpos = _WorldSpaceCameraPos.xyz + curDepth*wsDir;

			if (wpos.y < 0)
			{
				colToRet += float4(.01*saturate(abs(wpos.y*0.1)), 0, 0, 0.1);
			}
		}

		return colToRet.rgb;
	}

	 

















    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
		float depth = (1-LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).x)*(_ProjectionParams.z - _ProjectionParams.y)+ _ProjectionParams.y;
		float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
		outColor = frac(depth);//DoRayMarch(depth, input.texcoord, outColor);
		

		return float4(outColor,  1); 
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "SpaceFogShader"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
