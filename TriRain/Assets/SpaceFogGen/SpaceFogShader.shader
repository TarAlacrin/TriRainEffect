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


	float4 SampleRayMarch(float3 wsPos, float4 curColor, float stepSize)
	{
		float d = Density(pos);
	}



	float3 DoRayMarch(float sceneDepth, float2 uv, float3 sceneColor)
	{
		float3 wsDir = GetPixelRayDirectionWS(uv);
		float3 wsStartPoint = _WorldSpaceCameraPos.xyz;

		float4 colToRet = 0;



		[unroll]
		for (int i = 0; i < 64; i++)
		{
			float curDepth = _ProjectionParams.y + ((float)i) * _RayMarchStepSize;

			if (curDepth > sceneDepth)
			{
				colToRet = float4(lerp(sceneColor.rgb, colToRet.rgb, colToRet.a), saturate(colToRet.a+sceneColor.a));
			}
			if (colToRet.a == 1)
			{
				return colToRet.rgb;
			}

			float3 wpos = wsStartPoint + curDepth * wsDir;
			
			colToRet = SampleRayMarch(wpos, colToRet, _RayMarchStepSize);

		}

		return lerp(sceneColor.rgb, colToRet.rgb, colToRet.a);
	}



    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
		float depth = LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).x;
		depth = 1.0/(_ZBufferParams.z*depth + _ZBufferParams.w);//LinearEyeDepth(depth);
		 
		float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
		outColor = DoRayMarch(depth, input.texcoord, outColor);
		

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
