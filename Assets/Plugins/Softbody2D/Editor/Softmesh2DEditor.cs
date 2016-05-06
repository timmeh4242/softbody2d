namespace FinerGames {

	using UnityEngine;
	using System.Collections;
	using UnityEditor;

	[CustomEditor(typeof(Softmesh2D))]
	public class Softmesh2DEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if(GUILayout.Button("Update Mesh"))
			{
				Softmesh2D script = (Softmesh2D)target;
				script.UpdateMesh ();
			}
		}
	}
}