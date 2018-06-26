Shader "MWB/GeomtryLineShader"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "MWBStrcuture.cginc"

			StructuredBuffer<line_data> buf_line;
			StructuredBuffer<color_data> buf_color;
			/*
			struct input
			{
				float4 pos0 : SV_POSITION;
				float4 pos1 : TEXCOORD0;
				float4 col : COLOR;
			}; 
			*/
			struct i2g
			{
				float4 pos0 : SV_POSITION;
				float4 pos1 : TEXCOORD0;
				float4 col : COLOR;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			i2g vert(uint id : SV_VertexID)
			{
				i2g o;
				o.pos0 = float4(buf_line[id].begin, 1.0f);
				o.pos1 = float4(buf_line[id].end, 1.0f);

				o.col = buf_color[buf_line[id].index].color;
				 
				return o;
			}
			
			[maxvertexcount(4)]
			void geom(point i2g p[1], inout LineStream<g2f> lineStream)
			{
				float4 s[2];
				s[0] = mul(UNITY_MATRIX_VP, p[0].pos0);
				s[1] = mul(UNITY_MATRIX_VP, p[0].pos1);
				
				g2f pOut;
				pOut.col = p[0].col;

				pOut.pos = s[0];
				lineStream.Append(pOut);

				pOut.pos = s[1];
				lineStream.Append(pOut);
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
	}
}
