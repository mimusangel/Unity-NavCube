using UnityEngine;

public static class NavCubeExtends
{

	public static Vector3Int CeilToInt(this Vector3 vec)
	{
		return new Vector3Int(
			Mathf.CeilToInt(vec.x),
			Mathf.CeilToInt(vec.y),
			Mathf.CeilToInt(vec.z)
		);
	}

	public static Vector3 Ceil(this Vector3 vec)
	{
		return new Vector3(
			Mathf.Ceil(vec.x),
			Mathf.Ceil(vec.y),
			Mathf.Ceil(vec.z)
		);
	}
	
	public static Vector3Int FloorToInt(this Vector3 vec)
	{
		return new Vector3Int(
			Mathf.FloorToInt(vec.x),
			Mathf.FloorToInt(vec.y),
			Mathf.FloorToInt(vec.z)
		);
	}

	public static Vector3 Floor(this Vector3 vec)
	{
		return new Vector3(
			Mathf.Floor(vec.x),
			Mathf.Floor(vec.y),
			Mathf.Floor(vec.z)
		);
	}

	public static Vector3Int RoundToInt(this Vector3 vec)
	{
		return new Vector3Int(
			Mathf.RoundToInt(vec.x),
			Mathf.RoundToInt(vec.y),
			Mathf.RoundToInt(vec.z)
		);
	}

	public static Vector3 Round(this Vector3 vec)
	{
		return new Vector3(
			Mathf.Round(vec.x),
			Mathf.Round(vec.y),
			Mathf.Round(vec.z)
		);
	}

	public static Vector3Int Copy(this Vector3Int vec)
	{
		return new Vector3Int(vec.x, vec.y, vec.z);
	}

	public static Vector3Int Modulo(this Vector3Int vec, int mod)
	{
		return new Vector3Int(
			vec.x % mod,
			vec.y % mod,
			vec.z % mod
		);
	}

	public static Vector3Int Abs(this Vector3Int vec)
	{
		return new Vector3Int(
			Mathf.Abs(vec.x),
			Mathf.Abs(vec.y),
			Mathf.Abs(vec.z)
		);
	}

	public static int Manhattan(this Vector3Int vec)
	{
		return Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z);
	}

	public static int Manhattan(this Vector3Int A, Vector3Int B)
	{
		Vector3Int vec = B - A;
		return vec.Manhattan();
	}

}