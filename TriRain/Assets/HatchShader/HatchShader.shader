Shader "Unlit/HatchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


			float2 hash(float2 p) // replace this by something better
			{
				p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
				return -1.0 + 2.0*frac(sin(p)*43758.5453123);
			}


			float noise(in float2 p)
			{
				const float K1 = 0.366025404; // (sqrt(3)-1)/2;
				const float K2 = 0.211324865; // (3-sqrt(3))/6;

				float2  i = floor(p + (p.x + p.y)*K1);
				float2  a = p - i + (i.x + i.y)*K2;
				float m = step(a.y, a.x);
				float2  o = float2(m, 1.0 - m);
				float2  b = a - o + K2;
				float2  c = a - 1.0 + 2.0*K2;
				float3  h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
				float3  n = h * h*h*h*float3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
				return dot(n, float3(70.0,70,70));
			}

			float remap(float value, float low1, float high1, float low2, float high2)
			{
				return clamp(low2 + (value - low1) * (high2 - low2) / (high1 - low1), low2, high2);
			}


			float hatch(float2 uv, float2 scrUV, float inpTime)
			{
				float inp = (noise((floor(scrUV) + inpTime) * 0.3333) + 1.0) * 0.5;
				float inp2 = (noise((floor(scrUV * 0.1) + (inpTime * 0.5 + 150.0)) * 0.3333) + 1.0) * 0.5;
				inp2 = remap(inp2, 0.3, 1.0, 0.0, 1.0);
				inp = lerp(inp, 0.5, pow(inp2 * 18.0, 0.6));

				if (inp > 0.9)
				{
					if (abs(uv.x - uv.y) < 0.1)
					{
						if (length(uv - 0.5) < 0.707 * (inp - 0.9) * 10.0) return 1.0;
						else return 0.0;
					}
					else return 0.0;
				}
				else if (inp < 0.1)
				{
					if (abs((1.0 - uv.x) - uv.y) < 0.1)
					{
						if (length(uv - 0.5) < 0.707 * (1.0 - inp * 10.0)) return 1.0;
						else return 0.0;
					}
					else return 0.0;
				}
				else
					return 0.0;
			}


			float hatch2(float2 uv, float2 scrUV)
			{
				return hatch(uv, scrUV, _Time.y + 200.0) +
					hatch(uv + float2(0.5, -0.5), scrUV, _Time.y + 300.0) +
					hatch(uv + float2(-0.5, 0.5), scrUV, _Time.y + 400.0) +
					hatch(uv + float2(-0.5, -0.5), scrUV, _Time.y + 500.0) +
					hatch(uv + float2(0.5, 0.5), scrUV, _Time.y + 600.0);
			}


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = 1;
				
				float2 hatchUV = i.uv * 200;

				float noiseVal = clamp(hatch2(frac(hatchUV), hatchUV), 0.0, 1.0);
				float colorVal = (noise(hatchUV / 75.0) + 1.0) * 0.5;
				float3 outCol = lerp(float3(1.0, 0.5, 0.5), float3(1.0, 0.5, 1.0), colorVal) * noiseVal;
				col.rgb = outCol.rgb*20;

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
