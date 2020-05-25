using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavCubeAgent))]
public class NavCubeAgentEditor : Editor
{
	static string[] moveDir = new string[] { "Line", "Line + Diagonal", "Line + All Diagonal" };
	static string[] moveType = new string[] { "Walking", "Flying" };

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		NavCubeAgent agent = target as NavCubeAgent;




		agent.moveType = (NavCubeType)EditorGUILayout.Popup("Move Type", (int)agent.moveType, moveType) ;


		int index = 0;
		if ((agent.movedir & 4) == 4) index = 2;
		else if ((agent.movedir & 2) == 2) index = 1;
		index = EditorGUILayout.Popup("Move Direction", index, moveDir);
		switch (index)
		{
			case 1:
				agent.movedir = 1 | 2;
				break;
			case 2:
				agent.movedir = 1 | 2 | 4;
				break;
			case 0:
			default:
				agent.movedir = 1;
				break;
		}

	}
}
