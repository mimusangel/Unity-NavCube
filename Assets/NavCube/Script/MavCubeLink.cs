using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MavCubeLink : MonoBehaviour
{
	[HideInInspector]
	public Vector2Int size = new Vector2Int(1, 1);
	//[HideInInspector]
	public Vector3 target = new Vector3();

	[Tooltip(("Moving Cost")), Min(0.0f)]
	public float movingCost = 1;

	private float _cost = -1;
	public float cost
	{
		get
		{
			if (_cost < 0)
			{
				_cost = Vector3.Distance(positionA, positionB);
			}
			return _cost;
		}
	}

	public Vector3Int _positionA;
	public Vector3Int positionA
	{
		get
		{
			return _positionA;
		}
	}
	public Vector3Int positionB
	{
		get
		{
			return target.FloorToInt();
		}
	}

	public Vector3Int AtoB
	{
		get
		{
			return positionB - positionA;
		}
	}

	public Vector3Int BtoA
	{
		get
		{
			return positionA - positionB;
		}
	}

	public bool monoDirection = false;

	private void Start()
	{
		_positionA = transform.position.FloorToInt();
		NavCubeWorld world = LoadWorld();
		if (world != null)
		{
			world.AddLink(this);
		}
	}

	private void OnDestroy()
	{
		NavCubeWorld world = LoadWorld();
		if (world != null)
		{
			world.RemoveLink(this);
		}
	}

	private void OnEnable()
	{
		NavCubeWorld world = LoadWorld();
		if (world != null)
		{
			world.AddLink(this);
		}
	}

	private void OnDisable()
	{
		NavCubeWorld world = LoadWorld();
		if (world != null)
		{
			world.RemoveLink(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 offset = new Vector3((size.x / 2.0f), 0.1f, (size.y / 2.0f));
		Vector3 A = (Vector3)transform.position.FloorToInt() + offset;
		Vector3 B = (Vector3)positionB + offset;
		Vector3 sizedraw = new Vector3(size.x, 0.2f, size.y);
		Gizmos.DrawLine(A, B);
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(A, sizedraw);
		if (monoDirection)
			Gizmos.color = Color.red;
		Gizmos.DrawWireCube(B, sizedraw);
	}

	public NavCubeWorld LoadWorld()
	{
		if (NavCubeWorld.Exist(gameObject.scene))
		{
			return NavCubeWorld.Load(gameObject.scene);
		}
		return null;
	}

	public bool InPosA(Vector3Int pos)
	{
		Vector3Int p = positionA;
		if (pos.y != p.y)
			return false;
		if (pos.x < p.x || pos.x >= p.x + size.x ||
			pos.z < p.z || pos.z >= p.z + size.y)
			return false;
		return true;
	}

	public bool InPosB(Vector3Int pos)
	{
		Vector3Int p = positionB;
		if (pos.y != p.y)
			return false;
		if (pos.x < p.x || pos.x >= p.x + size.x ||
			pos.z < p.z || pos.z >= p.z + size.y)
			return false;
		return !monoDirection;
	}
}
