using System;
using System.Collections.Generic;
using UnityEngine;

public class NavCubeNode : IEquatable<NavCubeNode>, IEquatable<Vector3Int>
{
	public readonly static float Cost100 = 1.0f;
	public readonly static float Cost101 = 1.414214f;
	public readonly static float Cost111 = 1.732051f;

	public Vector3Int position = new Vector3Int();
	public float cost = 0.0f;
	public float heuristic = 0.0f;
	public NavCubeNode parent = null;

	public float total
	{
		get
		{
			return cost + heuristic;
		}
	}

	public NavCubeNode(Vector3Int pos, float cost, float heuristic, NavCubeNode parent = null)
	{
		this.position = pos;
		this.cost = cost;
		this.heuristic = heuristic;
		this.parent = parent;

	}

	public bool Equals(NavCubeNode other)
	{
		if (other == null)
		{
			return false;
		}
		return this.position == other.position;
	}

	public bool Equals(Vector3Int other)
	{
		if (other == null)
		{
			return false;
		}
		return this.position == other;
	}
}