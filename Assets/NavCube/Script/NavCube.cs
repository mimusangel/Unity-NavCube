using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavCube : MonoBehaviour
{
	public Bounds bound;
	public LayerMask ColliderLayer;

	[HideInInspector]
	[System.NonSerialized]
	public NavCubeWorld world;

	private void Awake()
	{
		LoadWorld();
	}

	public void LoadWorld()
	{
		if (world != null) return;
		if (NavCubeWorld.Exist(gameObject.scene))
		{
			world = NavCubeWorld.Load(gameObject.scene);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(transform.position + bound.center, bound.size);
	}

	public IEnumerator BakeChunk(Vector3Int chunkPosition)
	{
		NavCubeChunk chunk = null;
		if (world != null)
		{
			chunk = world.GetChunk(chunkPosition);
			yield return BakeChunk(chunk);
		}
		yield return null;
	}

	public IEnumerator BakeChunk(NavCubeChunk chunk)
	{
		if (chunk != null)
		{
			//Debug.Log($"Bake Chunk {chunk.position}");
			Vector3 position = chunk.position * NavCubeChunk.NavCube_ChunkSize;
			Vector3 offset = Vector3.one * 0.5f;
			Vector3 size = offset * 0.5f;
			for (int x = 0; x < NavCubeChunk.NavCube_ChunkSize; x++)
			{
				for (int y = 0; y < NavCubeChunk.NavCube_ChunkSize; y++)
				{
					for (int z = 0; z < NavCubeChunk.NavCube_ChunkSize; z++)
					{
						Vector3 point = position + new Vector3(x, y, z) + offset;
						if (Physics.CheckBox(point, size, Quaternion.identity, ColliderLayer.value))
						{
							chunk.SetData(x, y, z, NavCubeType.Blocked, int.MaxValue);
						}
						else
						{
							if (Physics.CheckBox(point + Vector3.down, size, Quaternion.identity, ColliderLayer.value))
							{
								chunk.SetData(x, y, z, NavCubeType.Walking, 1);
							}
							else
							{
								chunk.SetData(x, y, z, NavCubeType.Flying, 1);
							}
						}
					}
				}
			}
		}
		yield return null;
	}
}
