using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCubeAgent : MonoBehaviour
{
	[System.NonSerialized]
	private NavCubeWorld navCube = null;

	[Min(1)]
	public int height = 1;
	public int offset = 0;

	private Vector3Int destination;
	private List<Vector3Int> path;

	[HideInInspector]
	public int movedir = 1 | 2 | 4;

	[HideInInspector]
	public NavCubeType moveType = NavCubeType.Walking;

	public bool autoMoving = true;
	public bool autoRotate = true;
	[Min(0.001f)]
	public float moveSpeed = 1.0f;

	[Tooltip("Only Gizmos")]
	public bool pathDrawLine = true;

	private float moveTime = 0.0f;

	private void Awake()
	{
		if (NavCubeWorld.Exist(gameObject.scene))
		{
			navCube = NavCubeWorld.Load(gameObject.scene);
		}
	}

	public void SetDestination(Vector3Int dest)
	{
		if (destination != dest)
		{
			destination = dest;
			if (navCube != null) navCube.SearchPathAsync(this, PositionInNavCube, destination, moveType);
		}
	}
	
	public Vector3Int PositionInNavCube
	{
		get
		{
			if (navCube == null)
				return transform.position.FloorToInt() - (Vector3Int.up * offset);

			return navCube.GetWorldPosition(transform.position - (Vector3Int.up * offset));
		}
	}

	private void Start()
	{
		AlignFloor();
	}

	public void Align()
	{
		transform.position = transform.position + new Vector3(0.5f, 0.0f, 0.5f);
	}

	public void AlignFloor()
	{
		transform.position = transform.position.FloorToInt() + new Vector3(0.5f, 0.0f, 0.5f);
	}

	void Update()
    {
		if (navCube != null)
		{
			Vector3 offset = Vector3.one * 0.5f;
			Vector3 size = offset * 0.5f;

			if (path != null)
			{
				if (autoMoving && path.Count > 0)
				{
					if (path.Count >= 2)
					{
						Vector3 currentPos = path[0];
						Vector3 nextPos = path[1];
						float moveTotalTime = Vector3.Distance(currentPos, nextPos) / moveSpeed;

						moveTime += Time.deltaTime;
						if (moveTime >= moveTotalTime)
						{
							moveTime -= moveTotalTime;
							path.RemoveAt(0);
							if (path.Count >= 2)
							{
								currentPos = path[0];
								nextPos = path[1];
								moveTotalTime = Vector3.Distance(currentPos, nextPos) / moveSpeed;
							}
						}
						if (path.Count >= 2)
						{
							transform.position = Vector3.LerpUnclamped(currentPos, nextPos, moveTime / moveTotalTime);
							Align();
							if (autoRotate)
							{
								transform.forward = (nextPos - currentPos);
							}
						}
						else
						{
							if (path.Count == 1)
							{
								transform.position = path[0];
								AlignFloor();
							}
							moveTime = 0;
							path = null;
						}

					}
					else
					{
						if (path.Count == 1)
						{
							transform.position = path[0];
							AlignFloor();
						}
						moveTime = 0;
						path = null;
					}

				}
				if (path != null && pathDrawLine)
				{
					for (int i = 0; i < path.Count - 1; i++)
					{
						Debug.DrawLine(
							path[i] + offset,
							path[i + 1] + offset,
							Color.green
						);
					}
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(transform.position.FloorToInt() + new Vector3(0.5f, height / 2.0f, 0.5f) - (Vector3.up * offset), new Vector3(1, height, 1));
	}

	public void CompleteSearchPath(List<Vector3Int> path)
	{
		this.path = path;
	}
}
