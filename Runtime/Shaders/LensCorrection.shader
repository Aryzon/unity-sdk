Shader "Aryzon/LensCorrection" {
	Properties {
		_MainTex ("RGB", 2D) = "" {}
	}
	Subshader {
		Cull Off ZTest Always ZWrite Off
		ColorMask RGB             

		Pass {    

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			uniform float _X;
			uniform float _Y;
			uniform float _BarrelDistortion;

			float2 barrelDistortion(float2 uv)
			{
				float2 k = uv.xy - 0.5;
				float r2 = (k.x + _X) * (k.x + _X) + (k.y + _Y) * (k.y + _Y);
				float l = 1 + r2 * (_BarrelDistortion * sqrt(r2));
                
				return k*l + 0.5;
			}

			float4 frag (v2f_img i) : SV_Target
			{
				half2 uv = i.uv;
				float4 screen = tex2D(_MainTex, uv);
				if (_BarrelDistortion > 0) {
					uv = barrelDistortion(uv);
				}
				half2 red = 1.01*uv - 0.005;
				half2 green = 1.02*uv - 0.01;
				half2 blue = 1.04*uv - 0.02;

				if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) {
					screen.r = 0;
					screen.g = 0;
					screen.b = 0;
				} else {
					screen.r = tex2D(_MainTex, red).r;
					screen.g = tex2D(_MainTex, green).g;
					screen.b = tex2D(_MainTex, blue).b;
				}
				return screen;
			}
	ENDCG 
		}
	}
	Fallback off
}