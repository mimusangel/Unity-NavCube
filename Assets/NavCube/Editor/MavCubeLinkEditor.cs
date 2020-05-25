using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MavCubeLink))]
public class MavCubeLinkEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MavCubeLink link = target as MavCubeLink;

		link.size.x = Mathf.Max(EditorGUILayout.IntField("Size X", link.size.x), 1);
		link.size.y = Mathf.Max(EditorGUILayout.IntField("Size Z", link.size.y), 1);

	}

	private void OnSceneGUI()
	{
		MavCubeLink link = target as MavCubeLink;
		EditorGUI.BeginChangeCheck();
		Vector3 posB = Handles.PositionHandle(link.target, Quaternion.identity);


		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(link, "Change Target Position");
			link.target = posB;
		}
	}
}
