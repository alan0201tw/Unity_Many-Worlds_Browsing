#ifndef LINE_DATA
#define LINE_DATA

#include "UnityCG.cginc"

struct line_data
{
	float3 begin;
	float3 end;
	int index;
};

struct color_data
{
	float4 color;
};

// Rect : float4 : xMin, xMax, yMin, yMax

#endif