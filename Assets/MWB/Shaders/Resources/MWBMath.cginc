#ifndef MWB_MATH
#define MWB_MATH

#include "UnityCG.cginc"

inline float sqrMagnitude(float3 vec)
{
	return sqrt(dot(vec, vec));
}

// Rect : float4 : xMin, xMax, yMin, yMax
inline float rect_contain_pt(float4 rect, float2 pt)
{
	if (!(pt.x >= rect.x && pt.x <= rect.y))
		return -1;

	if (!(pt.y >= rect.z && pt.y <= rect.w))
		return -1;
	 
	return 1; 
}

inline float3 manual_world2Screen_point(float3 wp, float4x4 world2Screen, int pixelWidth, int pixelHeight)
{
	float4x4 mat = world2Screen;

	// multiply world point by VP matrix
	float4 temp = mul(mat, float4(wp.x, wp.y, wp.z, 1));

	if (temp.w == 0)
	{
		// point is exactly on camera focus point, screen point is undefined
		// unity handles this by returning 0,0,0
		return float3(0, 0, 0);
	}
	else
	{
		// convert x and y from clip space to window coordinates
		temp.x = (temp.x / temp.w + 1) * 0.5 * pixelWidth;
		temp.y = (temp.y / temp.w + 1) * 0.5 * pixelHeight;
		return float3(temp.x, temp.y, wp.z);
	}
}

inline float3 project_point_on_line(float3 line_start, float3 line_dir, float3 target_point)
{
	//get vector from point on line to point in space
	float3 linePointToPoint; 
	linePointToPoint.x = target_point.x - line_start.x; 
	linePointToPoint.y = target_point.y - line_start.y;
	linePointToPoint.z = target_point.z - line_start.z;

	float t = linePointToPoint.x * line_dir.x + linePointToPoint.y * line_dir.y + linePointToPoint.z * line_dir.z;

	return line_start + line_dir * t;
}

inline int point_on_which_side_of_line_segemnt(float3 linePoint1, float3 linePoint2, float3 target_point)
{
	float3 lineVec;
	lineVec.x = linePoint2.x - linePoint1.x;
	lineVec.y = linePoint2.y - linePoint1.y;
	lineVec.z = linePoint2.z - linePoint1.z;

	float3 pointVec;
	pointVec.x = target_point.x - linePoint1.x;
	pointVec.y = target_point.y - linePoint1.y;
	pointVec.z = target_point.z - linePoint1.z;

	float dot = pointVec.x * lineVec.x + pointVec.y * lineVec.y + pointVec.z * lineVec.z;

	//point is on side of linePoint2, compared to linePoint1
	if (dot > 0)
	{
		//point is on the line segment
		if (sqrMagnitude(pointVec) <= sqrMagnitude(lineVec))
		{
			return 0;
		}

		//point is not on the line segment and it is on the side of linePoint2
		else
		{
			return 2;
		}
	}

	//Point is not on side of linePoint2, compared to linePoint1.
	//Point is not on the line segment and it is on the side of linePoint1.
	else
	{
		return 1;
	}
}

inline float3 project_point_on_line_seg(float3 line_str, float3 line_end, float3 target_point)
{
	float3 vec;
	vec.x = line_end.x - line_str.x;
	vec.y = line_end.y - line_str.y;
	vec.z = line_end.z - line_str.z;

	float3 projectedPoint = project_point_on_line(line_str, normalize(vec), target_point);

	int side = point_on_which_side_of_line_segemnt(line_str, line_end, projectedPoint);

	//The projected point is on the line segment
	if (side == 0)
	{
		return projectedPoint;
	}

	if (side == 1)
	{
		return line_str;
	}

	if (side == 2)
	{
		return line_end;
	}

	//output is invalid
	return float3(0, 0, 0);
}

#endif