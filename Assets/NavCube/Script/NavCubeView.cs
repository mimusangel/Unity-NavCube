using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavCubeView : MonoBehaviour
{
	[HideInInspector]
	public List<List<Matrix4x4>> chunkRenderNav = new List<List<Matrix4x4>>();

	public Mesh DebugNavCubeMesh;
	public Material DebugNavCubeMaterial;

	public NavCubeWorld LoadWorld()
	{
		if (NavCubeWorld.Exist(gameObject.scene))
		{
			return NavCubeWorld.Load(gameObject.scene);
		}
		return null;
	}
	
	public void MakeRender()
	{
		chunkRenderNav.Clear();
		NavCubeWorld world = LoadWorld();
		if (world == null)
		{
			return;
		}
		chunkRenderNav.Add(new List<Matrix4x4>());
		if (world.chunks == null)
			return;

		Vector3 offset = Vector3.one * 0.5f;
		Vector3 size = offset * 0.5f;

		foreach (NavCubeChunk chunk in world.chunks)
		{
			Vector3Int position = chunk.position * NavCubeChunk.NavCube_ChunkSize;
			for (int x = 0; x < NavCubeChunk.NavCube_ChunkSize; x++)
			{
				for (int y = 0; y < NavCubeChunk.NavCube_ChunkSize; y++)
				{
					for (int z = 0; z < NavCubeChunk.NavCube_ChunkSize; z++)
					{
						//if (chunk.data[x, y, z] != (int)NavCubeType.Blocked)
						if (chunk.GetData(x, y, z).type != NavCubeType.Blocked)
						{
							continue;
						}
						List<Matrix4x4> chunkMatrices = chunkRenderNav[chunkRenderNav.Count - 1];
						if (chunkMatrices.Count >= 1000)
						{
							chunkMatrices = new List<Matrix4x4>();
							chunkRenderNav.Add(chunkMatrices);
						}
						chunkMatrices.Add(Matrix4x4.TRS(
							position + new Vector3(x, y, z) + offset,
							Quaternion.identity,
							size
						));
					}
				}
			}
		}
	}

	public void Update()
	{
		if (DebugNavCubeMesh && DebugNavCubeMesh)
		{
			foreach (List<Matrix4x4> matList in chunkRenderNav)
			{
				Graphics.DrawMeshInstanced(DebugNavCubeMesh, 0, DebugNavCubeMaterial, matList.ToArray());
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		NavCubeWorld world = LoadWorld();
		if (world == null)
		{
			return;
		}
		Gizmos.color = Color.red;
		foreach (NavCubeChunk chunk in world.chunks)
		{
			Vector3 position = chunk.position * NavCubeChunk.NavCube_ChunkSize;
			Vector3 size = Vector3.one * NavCubeChunk.NavCube_ChunkSize;

			Gizmos.DrawWireCube(
				position + size / 2.0f,
				size
			);
		}
	}
}
