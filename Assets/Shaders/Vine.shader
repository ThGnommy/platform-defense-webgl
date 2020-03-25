﻿Shader "Custom/Vine" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_P0 ("Point 0", Vector) = (0, 0, 0, 0)
		_P1 ("Point 1", Vector) = (0, 0, 0, 0)
		_P2 ("Point 2", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		 Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            // "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float2 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
				return o;
			}

			// quadratic bezier distance field calc taken from https://www.shadertoy.com/view/lts3Df

			float det(float2 a, float2 b) {
				return a.x*b.y-b.x*a.y;
			}

			// float2 closestPointInSegment(float2 a, float2 b ) {
			// 	float2 ba = b - a;
			// 	return a + ba*clamp( -dot(a,ba)/dot(ba,ba), 0.0, 1.0 );
			// }

			// From: http://research.microsoft.com/en-us/um/people/hoppe/ravg.pdf
			float2 get_distance_vector(float2 b0, float2 b1, float2 b2, out float t) {
				
				float a=det(b0,b2), b=2.0*det(b1,b0), d=2.0*det(b2,b1);


				float f=b*d-a*a;
				float2 d10=b1-b0, d20=b2-b0;
				float2 gf=2.0*(b*(b2-b1)+d*d10+a*d20);
				gf=float2(gf.y,-gf.x);
				float2 d0p=b0+f*gf/dot(gf,gf);

				t=clamp((det(d0p,d20)+2.0*det(d10,d0p))/(2.0*a+b+d), 0.0 ,1.0);
				return lerp(lerp(b0,b1,t),lerp(b1,b2,t),t);

			}

			float approx_distance(float2 p, float2 b0, float2 b1, float2 b2, out float t) {
				return length(get_distance_vector(b0-p, b1-p, b2-p, t));
			}

			float2 _P0, _P1, _P2;
			float _Offset;
			
			fixed4 frag (v2f i) : SV_Target {
				float t;
				if(approx_distance(floor(i.worldPos * 8) * 0.125, _P0, _P1 + float2(0.3 * sin(_Time.y * 1.5 + _P1.x), 0), _P2, t) < 0.0625) {
					return tex2D(_MainTex, float2(t, _Offset));
				} else {
					return fixed4(0, 0, 0, 0);
				}
			}
			ENDCG
		}
	}
}
