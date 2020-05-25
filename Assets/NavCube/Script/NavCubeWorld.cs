using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class NavCubeWorld
{
	public string folder = "";

	[NonSerialized]
	public List<NavCubeChunk> chunks;

	[NonSerialized]
	public static Dictionary<string, NavCubeWorld> loadedNavCube = new Dictionary<string, NavCubeWorld>();

	[NonSerialized]
	public List<MavCubeLink> links = new List<MavCubeLink>();

	public NavCubeWorld()
	{
		chunks = new List<NavCubeChunk>();
	}

	public Vector3Int GetChunkPosition(Vector3 worldPosition)
	{
		return (worldPosition / NavCubeChunk.NavCube_ChunkSize).FloorToInt();
	}

	public Vector3Int GetLocalPosition(Vector3 worldPosition)
	{
		Vector3Int chunkPosition = GetChunkPosition(worldPosition);
		Vector3 chunkWorldPos = chunkPosition * NavCubeChunk.NavCube_ChunkSize;
		Vector3 vecI = worldPosition - chunkWorldPos;
		return vecI.FloorToInt();
	}
	
	public Vector3Int GetWorldPosition(Vector3 worldPosition)
	{
		return worldPosition.FloorToInt();
	}

	public NavCubeChunk GetChunk(Vector3Int position)
	{
		if (chunks == null)
		{
			chunks = new List<NavCubeChunk>();
			return null;
		}
		foreach (NavCubeChunk chunk in chunks)
		{
			if (chunk.position == position)
			{
				return chunk;
			}
		}
		return null;
	}

	public NavCubeChunk.NavCubeData GetData(Vector3Int worldPosition)
	{
		Vector3Int chunkPosition = GetChunkPosition(worldPosition);
		NavCubeChunk chunk = GetChunk(chunkPosition);
		if (chunk == null)
		{
			return new NavCubeChunk.NavCubeData();
		}
		Vector3Int localPosition = GetLocalPosition(worldPosition);
		return chunk.GetData(localPosition);
	}

	#region Link
	public void AddLink(MavCubeLink link)
	{
		if (links == null)
		{
			links = new List<MavCubeLink>();
		}
		if (links.Contains(link) == false)
		{
			links.Add(link);
		}
	}

	public void RemoveLink(MavCubeLink link)
	{
		if (links == null)
		{
			links = new List<MavCubeLink>();
			return;
		}
		links.Remove(link);
	}
	#endregion

	#region Pathfinder
	List<NavCubeNode> closedList;
	List<NavCubeNode> openList;

	public void AddPathLink(NavCubeNode current, Vector3Int end, NavCubeType searchType, NavCubeAgent agent)
	{
		if (links == null)
		{
			links = new List<MavCubeLink>();
			return;
		}
		
		foreach (MavCubeLink link in links)
		{
			Vector3Int nextPos;
			if (link.InPosA(current.position))
			{
				nextPos = current.position + link.AtoB;
			}
			else if (link.InPosB(current.position))
			{
				nextPos = current.position + link.BtoA;
			}
			else
			{
				continue;
			}
			NavCubeChunk.NavCubeData data = GetData(nextPos);
			if (agent != null)
			{
				bool noValise = false;
				for (int i = 0; i < agent.height; i++)
				{
					int ii = i - agent.offset;
					if (GetData(nextPos + Vector3Int.up * ii).type == NavCubeType.Blocked)
					{
						noValise = true;
						break ;
					}
				}
				if (noValise)
				{
					continue;
				}
			}
			else
			{
				if (data.type == NavCubeType.Blocked)
				{
					continue;
				}
			}

			float heuristic = nextPos.Manhattan(end);
			NavCubeNode ncn = new NavCubeNode(nextPos, current.cost + link.cost * link.movingCost, heuristic, current);
			if (closedList.Contains(ncn))
			{
				continue;
			}

			if (openList.Contains(ncn))
			{
				NavCubeNode ol_ncn = openList.Find(x => x.position.Equals(nextPos));
				if (ol_ncn.total > ncn.total)
				{
					ol_ncn.cost = ncn.cost;
					ol_ncn.heuristic = ncn.heuristic;
					ol_ncn.parent = ncn.parent;
				}
				continue;
			}

			if (searchType == NavCubeType.Walking)
			{
				if (data.type == NavCubeType.Walking)
				{
					openList.Add(ncn);
				}
			}
			else
			{
				openList.Add(ncn);
			}
		}
	}

	public void AddNeighbour(NavCubeNode current, Vector3Int end, Vector3Int dir, float cost, NavCubeType searchType, NavCubeAgent agent)
	{
		Vector3Int pos = current.position + dir;
		NavCubeChunk.NavCubeData data = GetData(pos);

		if (agent != null)
		{
			for (int i = 0; i < agent.height; i++)
			{
				int ii = i - agent.offset;
				if (GetData(pos + Vector3Int.up * ii).type == NavCubeType.Blocked)
				{
					return;
				}
			}
		}
		else
		{
			if (data.type == NavCubeType.Blocked)
			{
				return;
			}
		}
			

		float heuristic = pos.Manhattan(end);
		NavCubeNode ncn = new NavCubeNode(pos, current.cost + data.cost * cost, heuristic, current);
		if (closedList.Contains(ncn))
		{
			return;
		}
		if (openList.Contains(ncn))
		{
			NavCubeNode ol_ncn = openList.Find(x => x.position.Equals(pos));
			if (ol_ncn.total > ncn.total)
			{
				ol_ncn.cost = ncn.cost;
				ol_ncn.heuristic = ncn.heuristic;
				ol_ncn.parent = ncn.parent;
			}
			return;
		}

		if (searchType == NavCubeType.Walking)
		{
			if (data.type == NavCubeType.Walking)
			{
				openList.Add(ncn);
			}
		}
		else
		{
			openList.Add(ncn);
		}
	}

	public List<Vector3Int> GetPath(Vector3Int start, Vector3Int end, NavCubeType type = NavCubeType.Walking, int maxResolveTry = 4096, NavCubeAgent agent = null)
	{
		closedList = new List<NavCubeNode>();
		openList = new List<NavCubeNode>();

		List<Vector3Int> path = null;
		if (agent != null)
		{
			for (int i = 0; i < agent.height; i++)
			{
				int ii = i - agent.offset;
				if (GetData(start + Vector3Int.up * ii).type == NavCubeType.Blocked || GetData(end + Vector3Int.up * ii).type == NavCubeType.Blocked)
				{
					return null;
				}
			}
		}
		else
		{
			if (GetData(start).type == NavCubeType.Blocked || GetData(end).type == NavCubeType.Blocked)
			{
				return null;
			}
		}
		openList.Add(new NavCubeNode(start.Copy(), 0.0f, 0.0f));
		NavCubeNode current = null;
		while (openList.Count > 0)
		{
			current = openList[0];
			openList.RemoveAt(0);
			if (current.Equals(end))
			{
				break;
			}
			
			if (agent == null || (agent.movedir & 1) == 1)
			{
				AddNeighbour(current, end, new Vector3Int(1, 0, 0), NavCubeNode.Cost100, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 0, 0), NavCubeNode.Cost100, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, 1, 0), NavCubeNode.Cost100, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, -1, 0), NavCubeNode.Cost100, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, 0, 1), NavCubeNode.Cost100, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, 0, -1), NavCubeNode.Cost100, type, agent);
			}

			if (agent == null || (agent.movedir & 2) == 2)
			{
				AddNeighbour(current, end, new Vector3Int(1, -1, 0), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, -1, 0), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, -1, 1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, -1, -1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 0, -1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, 0, -1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, 0, 1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 0, 1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, 1, 0), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 1, 0), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, 1, 1), NavCubeNode.Cost101, type, agent);
				AddNeighbour(current, end, new Vector3Int(0, 1, -1), NavCubeNode.Cost101, type, agent);
			}

			if (agent == null || (agent.movedir & 4) == 4)
			{
				AddNeighbour(current, end, new Vector3Int(-1, -1, -1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, -1, -1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, -1, 1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, -1, 1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 1, -1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, 1, -1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(1, 1, 1), NavCubeNode.Cost111, type, agent);
				AddNeighbour(current, end, new Vector3Int(-1, 1, 1), NavCubeNode.Cost111, type, agent);
			}

			AddPathLink(current, end, type, agent);


			openList = openList.OrderBy(x => x.total).ToList<NavCubeNode>();
			closedList.Add(current);

			if (closedList.Count >= maxResolveTry)
			{
				break;
			}
		}

		if (current != null && current.Equals(end))
		{
			Debug.Log($"Path found ! (Search Stack: {closedList.Count})");
			path = new List<Vector3Int>();
			NavCubeNode node = current;
			while (node != null)
			{
				path.Insert(0, node.position);
				node = node.parent;
			}
		}
		else
		{
			Debug.Log("Path not found !");
		}

		closedList.Clear();
		openList.Clear();
		return path;
	}
	#endregion

	#region Async_Method
	[NonSerialized] // One Thread per agent
	private static Dictionary<NavCubeAgent, Thread> _AsyncAgentSearch = new Dictionary<NavCubeAgent, Thread>();
	public void SearchPathAsync(NavCubeAgent agent, Vector3Int start, Vector3Int end, NavCubeType type = NavCubeType.Walking, int maxResolveTry = 4096)
	{
		if (_AsyncAgentSearch == null)
		{
			_AsyncAgentSearch = new Dictionary<NavCubeAgent, Thread>();
		}
		Thread asyncThread = null;
		if (_AsyncAgentSearch.ContainsKey(agent))
		{
			asyncThread = _AsyncAgentSearch[agent];
			if (asyncThread.IsAlive)
			{
				asyncThread.Abort();
			}
		}
		asyncThread = new Thread(() =>
		{
			try
			{
				List<Vector3Int> path = GetPath(start, end, type, maxResolveTry, agent);
				agent.CompleteSearchPath(path);
			}
			catch (ThreadAbortException)
			{
				Debug.Log("Thread Exit !");
			}
		});
		asyncThread.Start();
		if (_AsyncAgentSearch.ContainsKey(agent))
		{
			_AsyncAgentSearch[agent] = asyncThread;
		}
		else
		{
			_AsyncAgentSearch.Add(agent, asyncThread);
		}
	}
	#endregion

	#region SaveAndLoad
	public void Save()
	{
		string filePath = $"{Application.streamingAssetsPath}/{folder}/ncw.nc";
		FileStream fs = new FileStream(filePath, FileMode.Create);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(fs, this);
		fs.Close();
	}

	public void SaveAll()
	{
		Save();
		SaveAllChunk();
	}

	public void SaveAllChunk()
	{
		foreach (NavCubeChunk chunk in chunks)
		{
			SaveChunk(chunk);
		}
	}

	public void SaveChunk(NavCubeChunk chunk)
	{
		string filePath = $"{Application.streamingAssetsPath}/{folder}/{chunk.GetFileName()}";
		FileStream fs = new FileStream(filePath, FileMode.Create);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(fs, chunk);
		fs.Close();
	}

	public void LoadAllChunk()
	{
		string path = $"{Application.streamingAssetsPath}/{folder}/";
		if (chunks == null)
		{
			chunks = new List<NavCubeChunk>();
		}
		chunks.Clear();
		string[] filePaths = Directory.GetFiles(path, "*.ncc");
		foreach (string filePath in filePaths)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open);
			BinaryFormatter formatter = new BinaryFormatter();
			NavCubeChunk chunk = formatter.Deserialize(fs) as NavCubeChunk;
			fs.Close();
			if (chunk != null)
			{
				chunk.world = this;
				chunks.Add(chunk);
			}
		}
	}

	public static NavCubeWorld Load(string pathfolder)
	{
		if (loadedNavCube == null)
		{
			loadedNavCube = new Dictionary<string, NavCubeWorld>();
		}
		if (loadedNavCube.ContainsKey(pathfolder))
		{
			return loadedNavCube[pathfolder];
		}

		NavCubeWorld world;
		string filePath = $"{Application.streamingAssetsPath}/{pathfolder}/ncw.nc";
		FileStream fs = new FileStream(filePath, FileMode.Open);
		BinaryFormatter formatter = new BinaryFormatter();
		world = formatter.Deserialize(fs) as NavCubeWorld;
		fs.Close();
		world.LoadAllChunk();
		loadedNavCube.Add(pathfolder, world);
		return world;
	}

	public static NavCubeWorld Load(Scene scene)
	{
		return Load($"NavCube/{scene.name}");
	}

	public static bool Exist(Scene scene)
	{
		string filePath = $"{Application.streamingAssetsPath}/NavCube/{scene.name}/ncw.nc";
		return File.Exists(filePath);
	}

	public static NavCubeWorld LoadOrCreate(string pathfolder)
	{
		if (loadedNavCube == null)
		{
			loadedNavCube = new Dictionary<string, NavCubeWorld>();
		}
		string filePath = $"{Application.streamingAssetsPath}/{pathfolder}/ncw.nc";
		if (File.Exists(filePath))
		{
			return Load(pathfolder);
		}
		else
		{
			NavCubeWorld world = new NavCubeWorld();
			world.folder = pathfolder;
			world.Save();
			loadedNavCube.Add(pathfolder, world);
			return world;
		}
	}

	public NavCubeChunk LoadOrCreateChunk(Vector3Int currentChunkPos)
	{
		NavCubeChunk ncc = GetChunk(currentChunkPos);
		if (ncc != null)
		{
			ncc.world = this;
			return ncc;
		}
		ncc = new NavCubeChunk(this);
		ncc.position = currentChunkPos.Copy();
		chunks.Add(ncc);
		return ncc;
	}
	#endregion
}
