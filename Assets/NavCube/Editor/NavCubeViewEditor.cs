using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavCubeView))]
public class NavCubeViewEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if (GUILayout.Button("Update Render"))
		{
			((NavCubeView)target).MakeRender();
			SceneView.RepaintAll();
		}
	}
}
