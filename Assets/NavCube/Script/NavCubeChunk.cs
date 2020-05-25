using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NavCubeChunk
{
	public readonly static int NavCube_ChunkSize = 16;

	public int x;
	public int y;
	public int z;
	
	public NavCubeData[,,] data;


	[NonSerialized]
	public NavCubeWorld world;

	public NavCubeChunk(NavCubeWorld world)
	{
		data = new NavCubeData[NavCube_ChunkSize, NavCube_ChunkSize, NavCube_ChunkSize];
		for (int x = 0; x < NavCube_ChunkSize; x++)
		{
			for (int y = 0; y < NavCube_ChunkSize; y++)
			{
				for (int z = 0; z < NavCube_ChunkSize; z++)
				{
					data[x, y, z] = new NavCubeData();
				}
			}
		}
	}

	public Vector3Int position
	{
		get
		{
			return new Vector3Int(x, y, z);
		}
		set
		{
			x = value.x;
			y = value.y;
			z = value.z;
		}
	}

	public string GetFileName()
	{
		return $"{position.x}_{position.y}_{position.z}.ncc";
	}

	public NavCubeData GetData(int x, int y, int z)
	{
		return data[x, y, z];
	}

	public NavCubeData GetData(Vector3Int vec)
	{
		return GetData(vec.x, vec.y, vec.z);
	}

	public void SetData(int x, int y, int z, NavCubeType type, int cost)
	{
		data[x, y, z].type = type;
		data[x, y, z].cost = cost;
	}

	[Serializable]
	public class NavCubeData
	{
		public NavCubeType type;
		public int cost;

		public NavCubeData(NavCubeType t = NavCubeType.Blocked, int c = int.MaxValue)
		{
			type = t;
			cost = c;
		}
	}
}
