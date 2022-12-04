Shader "Unlit/MaskShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ScreenRatio("ScreenRatio", Vector) = (1,1,0,0)
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
		LOD 100

		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 screenPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _ScreenRatio;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = o.vertex.xyw;

				// This might be platform-specific. Test with OpenGL.
				o.screenPos.y *= -1.0f;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				float2 uv = (i.screenPos.xy / i.screenPos.z) * 0.5f *_ScreenRatio.xy + 0.5f;

				fixed4 col = tex2D(_MainTex, float2(uv.x, 1.0-uv.y));

				return col;
			}
			ENDCG
		}
	}
}
