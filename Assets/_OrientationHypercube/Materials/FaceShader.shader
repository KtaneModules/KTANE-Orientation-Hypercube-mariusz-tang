Shader "Pattern Hypercube/Face Shader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Transparency ("Transparency", float) = 0
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "TransparentCutout"
		}

		LOD 200
		Cull Back
		ZTest On
		ZWrite On
		Lighting Off
		Fog { Mode Off }
		AlphaToMask On

		Blend One OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _Color;
			float _Transparency;
			float4 _MainTex_ST;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				// return float4(i.uv.x, i.uv.y, 0, 1);
				float2 pos = i.uv - 0.5;
				if (abs(pos.x) > 0.4 || abs(pos.y) > 0.4) return _Color;
				float alpha = tex2D(_MainTex, i.uv1 + pos / 14.0).a;
				if (alpha > 0.5) return _Color;
				float4 res = _Color;
				res.a = _Transparency;
				return res;
			}
			ENDCG
		}
	}
}
