Shader "Hidden/Shader/SpaceFogShader"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch


    //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"
/*
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
	*/
    // List of properties to control your post process effect
    float _Intensity;
	float _RayMarchStepSize;
	TEXTURE2D_SAMPLER2D(_InputTexture, sampler_InputTexture);
	TEXTURE2D_SAMPLER2D(_NoiseTexture, sampler_NoiseTexture);

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
		return 0;
		//float d = Density(pos);
	}



#define DITHERING
#define BACKGROUND

	//-------------------
#define pi 3.14159265
#define R(p, a) p=cos(a)*p+sin(a)*vec2(p.y, -p.x)


	float rand(float2 co)
	{
		return frac(sin(dot(co*0.123, float2(12.9898, 78.233))) * 43758.5453);
	}


	float singleChannelNoise(float2 co)
	{
		return float2(rand(co), rand(float2(3567-co.y*23.0, co.x*13.0+co.y)));
	}



	// iq's noise
	float noise(in float3 x)
	{
		float3 p = floor(x);
		float3 f = frac(x);
		f = f * f*(3.0 - 2.0*f);
		float2 uv = (p.xy + float2(37.0, 17.0)*p.z) + f.xy;

		int3 loadUv = 0;
		
		loadUv.xy = (uv + 0.5) / 256.0;

		float2 rg = _NoiseTexture.Load(loadUv).yx;//singleChannelNoise((uv + 0.5) / 256.0);//SAMPLE_TEXTURE2D_LOD(_NoiseTexture, (uv + 0.5) / 256.0, 0.0).yx;
		return 1. - 0.82*lerp(rg.x, rg.y, f.z);
	} 

	float fbm(float3 p)
	{
		return noise(p*.06125)*.5 + noise(p*.125)*.25 + noise(p*.25)*.125 + noise(p*.4)*.2;
	}


	//=====================================
	// otaviogood's noise from https://www.shadertoy.com/view/ld2SzK
	//--------------------------------------------------------------
	// This spiral noise works by successively adding and rotating sin waves while increasing frequency.
	// It should work the same on all computers since it's not based on a hash function like some other noises.
	// It can be much faster than other noise functions if you're ok with some repetition.
	const float nudge = 0.739513;	// size of perpendicular floattor
	#define normalizer  1.0 / sqrt(1.0 + nudge * nudge);	// pythagorean theorem on that perpendicular to maintain scale
	float SpiralNoiseC(float3 p)
	{
		float n = 0.0;	// noise amount
		float iter = 1.0;
		for (int i = 0; i < 8; i++)
		{
			// add sin and cos scaled inverse with the frequency
			n += -abs(sin(p.y*iter) + cos(p.x*iter)) / iter;	// abs for a ridged look
																// rotate by adding perpendicular and scaling down
			p.xy += float2(p.y, -p.x) * nudge;
			p.xy *= normalizer;
			// rotate on other axis
			p.xz += float2(p.z, -p.x) * nudge;
			p.xz *= normalizer;
			// increase the frequency
			iter *= 1.733733;
		}
		return n;
	}

	float SpiralNoise3D(float3 p)
	{
		float n = 0.0;
		float iter = 1.0;
		for (int i = 0; i < 5; i++)
		{
			n += (sin(p.y*iter) + cos(p.x*iter)) / iter;
			p.xz += float2(p.z, -p.x) * nudge;
			p.xz *= normalizer;
			iter *= 1.33733;
		}
		return n;
	}

	float Nebulae(float3 p)
	{
		float final = p.y + 4.5;
		final += SpiralNoiseC(p.zxy*0.123 + 100.0)*3.0;	// large scale features
		final -= SpiralNoise3D(p);	// more large scale features, but 3d

		return final;
	}

	float map(float3 p)
	{
		p.y += 4.1;
		return Nebulae(p) + fbm(p*50. + _Time.y);
	}

	// assign color to the media
	float3 computeColor(float density, float radius)
	{
		// color based on density alone, gives impression of occlusion within
		// the media
		float3 result = lerp(float3(1.0, 0.9, 0.8), float3(0.4, 0.15, 0.1), density);

		// color added to the media
		float3 colCenter = 7.*float3(0.8, 1.0, 1.0);
		float3 colEdge = 1.5*float3(0.48, 0.53, 0.5);
		result *= lerp(colCenter, colEdge, min((radius + .05) / 1.30, 1.15));

		return result;
	}

	bool RaySphereIntersect(float3 org, float3 dir, out float near, out float far)
	{
		float b = dot(dir, org);
		float c = dot(org, org) - 20.;
		float delta = b * b - c;
		if (delta < 0.0)
			return false;
		float deltasqrt = sqrt(delta);
		near = -b - deltasqrt;
		far = -b + deltasqrt;
		return far > 0.0;
	}



	float3 DoRayMarch(float sceneDepth, float2 uv, float3 sceneColor)
	{
		float3 wsDir = GetPixelRayDirectionWS(uv);
		float3 wsStartPoint = _WorldSpaceCameraPos.xyz;

		float4 colToRet = 0;

#ifdef DITHERING
		float2 dpos = uv;
		float2 seed = dpos + frac(_Time.y);
		// randomizing the length 
		//rd *= (1. + frac(sin(dot(float3(7, 157, 113), rd.zyx))*43758.5453)*0.1-0.03);
#endif 

		// ld, td: local, total density 
		// w: weighting factor
		float ld = 0., td = 0., w = 0.;

		// t: length of the ray
		// d: distance function
		float d = 1., t = 0.;

		const float h = 0.1;

		float4 sum = (0.0);

		float min_dist = 0.0, max_dist = 0.0;

		if (RaySphereIntersect(wsStartPoint, wsDir, min_dist, max_dist))
		{
			t = min_dist * step(t, min_dist);

			// raymarch loop
			for (int i = 0; i<128; i++)
			{
				float3 pos = wsStartPoint + t * wsDir;

				// Loop break conditions.
				if (td>0.9 || d<0.1*t || t>10. || sum.a > 0.99 || t>max_dist) break;

				// evaluate distance function
				float d = map(pos);

				// change this string to control density 
				d = max(d, 0.08);

				if (d<h)
				{
					// compute local density 
					ld = h - d;

					// compute weighting factor 
					w = (1. - td) * ld;

					// accumulate density
					td += w + 1. / 200.;

					float radiusFromCenter = length(pos - (0.0));

					float computedColor = computeColor(td, radiusFromCenter);

					float4 col = float4(computedColor, computedColor, computedColor, td);

					// uniform scale density
					col.a *= 0.185;
					// colour by alpha
					col.rgb *= col.a;
					// alpha blend in contribution
					sum = sum + col * (1.0 - sum.a);

				}

				td += 1. / 70.;

				// point light calculations
				float3 ldst = (0.0) - pos;
				float lDist = max(length(ldst), 0.001);

				// star in center
				float3 lightColor = float3(1.0, 0.5, 0.25);
				sum.rgb += lightColor / (lDist*lDist*6.); //add a bloom around the light

				sum.a *= 0.8;

				// enforce minimum stepsize
				d = max(d, 0.1);

#ifdef DITHERING
				// add in noise to reduce banding and create fuzz
				d = abs(d)*(1. + 0.2*rand(seed*(i)));
#endif 

				//t += max(d * 0.25, 0.02);
				t += max(d * 0.1 * max(length(ldst), 2.0), 0.02);

			}

			// simple scattering
			sum *= 1. / exp(ld * 0.2) * 0.6;

			sum = clamp(sum, 0.0, 1.0);

			sum.xyz = sum.xyz*sum.xyz*(3.0 - 2.0*sum.xyz);

		}

		/*
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

		}*/

		colToRet = sum;

		return colToRet.rgb;//lerp(sceneColor.rgb, colToRet.rgb, colToRet.a);
	}

	 

    float4 CustomPostProcess(VaryingsDefault input) : SV_Target
    {
        //UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        //uint2 positionSS = input.texcoord * _ScreenSize.xy;
		float depth = 100;//LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).x;
		depth = 1.0/(_ZBufferParams.z*depth + _ZBufferParams.w);//LinearEyeDepth(depth);
		 
		float3 outColor = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, UnityStereoTransformScreenSpaceTex(input.texcoord)).rgb;// LOAD_TEXTURE2D_X(_InputTexture, positionSS%255).xyz; 
		outColor = DoRayMarch(depth, input.texcoord, outColor);
		outColor = 0;

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
                #pragma vertex VertDefault
            ENDHLSL
        }
    }
    Fallback Off
}
