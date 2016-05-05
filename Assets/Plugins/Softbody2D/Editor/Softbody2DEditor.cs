using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Softbody2D))]
public class Softbody2DEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if(GUILayout.Button("Generate Softbody2D"))
		{
			Softbody2D script = (Softbody2D)target;
			script.Generate (true);
		}

		if(GUILayout.Button("Clear Forces"))
		{
			Softbody2D script = (Softbody2D)target;
			script.ClearForces ();
		}
	}
}