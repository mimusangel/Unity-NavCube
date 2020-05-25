using System.Collections;
using System.IO;
using System.Threading;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[CustomEditor(typeof(NavCube))]
public class NavCubeEditor : Editor
{
	private bool IsBaking = false;
	private Vector3Int bakeChunk = new Vector3Int();
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		DrawUILine(Color.white);

		NavCube navCube = target as NavCube;
		bool repaint = false;

		if (navCube.world == null)
		{
			navCube.LoadWorld();
			repaint = true;
		}


		Vector3Int origin = ((navCube.bound.min + navCube.transform.position) / (float)NavCubeChunk.NavCube_ChunkSize).FloorToInt();
		Vector3Int originEnd = ((navCube.bound.max + navCube.transform.position) / (float)NavCubeChunk.NavCube_ChunkSize).CeilToInt();


		GUILayout.Label("Chunk:");
		GUILayout.Label($" Min : {origin}");
		GUILayout.Label($" Max : {originEnd}");

		//EditorGUI.BeginChangeCheck();
		//navCube.isRenderChunk = GUILayout.Toggle(navCube.isRenderChunk, "Render Chunk");
		//if (EditorGUI.EndChangeCheck())
		//{
		//	repaint = true;
		//}

		//EditorGUI.BeginChangeCheck();
		//navCube.isRenderNavCube = GUILayout.Toggle(navCube.isRenderNavCube, "Render Nav Cube");
		//if (EditorGUI.EndChangeCheck())
		//{
		//	repaint = true;
		//}

		DrawUILine(Color.white);

		GUILayout.Label($"Nav Cube World: {navCube.world != null}");

		if (navCube.world == null) IsBaking = false;

		if (IsBaking)
		{
			GUILayout.Label("Baking...");
		}
		else
		{
			if (GUILayout.Button("Bake"))
			{
				Scene scene = navCube.gameObject.scene;

				if (AssetDatabase.IsValidFolder("Assets/StreamingAssets") == false)
				{
					string guid = AssetDatabase.CreateFolder("Assets", "StreamingAssets");
				}
				
				string path = "Assets/StreamingAssets/NavCube";
				if (AssetDatabase.IsValidFolder(path) == false)
				{
					string guid = AssetDatabase.CreateFolder("Assets/StreamingAssets", "NavCube");
				}

				if (AssetDatabase.IsValidFolder($"{path}/{scene.name}") == false)
				{
					string guid = AssetDatabase.CreateFolder(path, scene.name);
				}

				string folderPath = $"NavCube/{scene.name}";

				Vector3Int numberChunk = originEnd - origin;

				IsBaking = true;
				
				EditorCoroutineUtility.StartCoroutine(BakeWorld(navCube, origin, numberChunk, folderPath), this);
			}
			if (navCube.world != null)
			{
				bakeChunk = EditorGUILayout.Vector3IntField("Chunk", bakeChunk);
				if (GUILayout.Button("Bake Chunk"))
				{
					EditorCoroutineUtility.StartCoroutine(BakeChunk(navCube, bakeChunk), this);
				}
			}
		}

		if (repaint)
		{
			//if (navCube.isRenderNavCube)
			//{
			//	IsBaking = true;
			//	navCube.MakeNavCubeRender();
			//}
			SceneView.RepaintAll();
		}
	}

	IEnumerator BakeChunk(NavCube navCube, Vector3Int chunk)
	{
		if (navCube.world != null)
		{
			NavCubeChunk ncc = navCube.world.LoadOrCreateChunk(chunk);
			if (ncc != null)
			{
				yield return navCube.BakeChunk(ncc);

				navCube.world.SaveChunk(ncc);
			}
		}
		yield return null;

		IsBaking = false;
		//navCube.MakeNavCubeRender();

		SceneView.RepaintAll();
	}

	IEnumerator BakeWorld(NavCube navCube, Vector3Int origin, Vector3Int numberChunk, string folderPath)
	{
		NavCubeChunk currentChunk;
		Vector3Int currentChunkPos = new Vector3Int();

		NavCubeWorld ncw = NavCubeWorld.LoadOrCreate(folderPath); //LoadOrCreateWorld(folderPath);
		navCube.world = ncw;

		//ncw.chunks.Clear();

		for (int x = 0; x < numberChunk.x; x++)
		{
			currentChunkPos.x = x + origin.x;
			for (int y = 0; y < numberChunk.y; y++)
			{
				currentChunkPos.y = y + origin.y;
				for (int z = 0; z < numberChunk.z; z++)
				{
					currentChunkPos.z = z + origin.z;
					currentChunk = ncw.LoadOrCreateChunk(currentChunkPos);

					yield return navCube.BakeChunk(currentChunk);
					ncw.SaveChunk(currentChunk);

					//EditorUtility.SetDirty(currentChunk);
				}
			}
		}


		//AssetDatabase.SaveAssets();
		//AssetDatabase.Refresh();
		

		yield return null;

		IsBaking = false;
		//navCube.MakeNavCubeRender();

		SceneView.RepaintAll();
	}

	//public NavCubeWorld LoadOrCreateWorld(string pathFolder)
	//{
	//	NavCubeWorld ncw;
	//	string path = $"{pathFolder}/ncw.asset";

	//	ncw = AssetDatabase.LoadAssetAtPath<NavCubeWorld>(path);
	//	if (ncw == null)
	//	{
	//		ncw = ScriptableObject.CreateInstance<NavCubeWorld>();
	//		ncw.folder = pathFolder;
	//		AssetDatabase.CreateAsset(ncw, path);
	//	}

	//	return ncw;
	//}

	//public NavCubeChunk LoadOrCreateChunk(Vector3Int chunkPosition, string pathFolder)
	//{
	//	string fileNameChunk = $"ncc_{chunkPosition.x}_{chunkPosition.y}_{chunkPosition.z}.asset";
	//	NavCubeChunk chunk;

	//	string path = $"{pathFolder}/{fileNameChunk}";

	//	chunk = AssetDatabase.LoadAssetAtPath<NavCubeChunk>(path);
	//	if (chunk == null)
	//	{
	//		chunk = ScriptableObject.CreateInstance<NavCubeChunk>();
	//		chunk.position = chunkPosition.Copy();
	//		AssetDatabase.CreateAsset(chunk, path);
	//	}

	//	return chunk;
	//}
	/**
	 * UTILITY
	 * **/

	public static void DrawUILine(Color color)
	{
		Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(12));
		r.height = 2;
		r.y += 5;
		EditorGUI.DrawRect(r, color);
	}
}
