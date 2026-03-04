using System;
using UnityEngine;

public static class Vector2Extension
{
	public static Vector2 Rotate(this Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		float num2 = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}
}
