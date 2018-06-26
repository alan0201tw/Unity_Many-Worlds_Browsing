Shader "Custom/GeometryLineDrawer"
{
	Properties
	{
		_Color("Color", Color) = (0, 0, 1, 1)
	}
	SubShader
		{
			Tags{ "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma target 5.0
				#pragma vertex vert
				//#pragma geometry
				#pragma fragment frag	
				#include "UnityCG.cginc"

				fixed4 _Color;
				float4 _Position;

				struct bufferData
				{
					float3 vertex;
				};

				StructuredBuffer<bufferData> _BufferPoints;

				struct v2f
				{
					float4 vertex : SV_POSITION;
				};

				v2f vert(uint vid : SV_VertexID)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(float4(_Position.xyz + _BufferPoints[vid].vertex, 1));
					return o;
				}
				

				fixed4 frag(v2f i) : SV_Target
				{
					return _Color;
				}
				ENDCG
			}
		}
}
