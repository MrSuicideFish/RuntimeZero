Shader "Hidden/LowBitOverlay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EdgeColor ("EdgeColor", Color) = (0.5,0.5,0.5,1)
		_NumTiles ("Number of Tiles", float) = 8.0
		_Threshhold ("Threshhold", float) = 1.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _NumTiles;
			half4 _EdgeColor;
			float _Threshold;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				half4 size = 1.0 / _NumTiles;

				half2 Pbase = i.uv - fmod (i.uv, size.xx);
				half2 PCenter = Pbase + (size / 2.0).xx;

				half2 st = (i.uv)/size;

				half4 c1 = (half4)0;
				half4 c2 = (half4)0;
				half4 invOff = (1 - _EdgeColor);

				if (st.x > st.y) 
				{
					c1 = invOff;
				}

				half thresholdB = 1.0 - 2.1;

				if (st.x > thresholdB)
					c2 = c1;
				if (st.y > thresholdB)
					c2 = c1;

				half4 cBottom = c2;
				c1 = (half4)0;
				c2 = (half4)0;

				if (st.x > st.y)
					c1 = invOff;
				if (st.x < _Threshold)
					c2 = c1;
				if (st.y < _Threshold)
					c2 = c1;

				half4 cTop = c2;
				half4 tileColor = tex2D (_MainTex, PCenter);
				half4 result = tileColor + cTop - cBottom;
				return result;
			}
			ENDCG
		}
	}
}
