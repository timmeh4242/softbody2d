using UnityEngine;
using System.Collections;

public class Water2D : MonoBehaviour {

	float[] xpositions;
	float[] ypositions;
	float[] velocities;
	float[] accelerations;
	LineRenderer Body;

	GameObject[] meshobjects;
	Mesh[] meshes;

	GameObject[] colliders;

	const float springconstant = 0.02f;
	const float damping = 0.04f;
	const float spread = 0.05f;
	const float z = -1f;

	float baseheight;
	float left;
	float bottom;

	public GameObject splash;
	public Material mat;
	public GameObject watermesh;

	void Start () {
		SpawnWater(0f, 10f, 0f, -10f);
	}

	public void SpawnWater (float Left, float Width, float Top, float Bottom) {

		int edgecount = Mathf.RoundToInt (Width) * 5;
		int nodecount = edgecount + 1;

		Body = gameObject.AddComponent<LineRenderer> ();
		Body.material = mat;
		Body.material.renderQueue = 1000;
		Body.SetVertexCount (nodecount);
		Body.SetWidth (1f, 1f);

		xpositions = new float[nodecount];
		ypositions = new float[nodecount];
		velocities = new float[nodecount];
		accelerations = new float[nodecount];

		meshobjects = new GameObject[edgecount];
		meshes = new Mesh[edgecount];
		colliders = new GameObject[edgecount];

		baseheight = Top;
		bottom = Bottom;
		left = Left;

		for (int i = 0; i < nodecount; i++) {
			ypositions[i] = Top;
			xpositions[i] = Left + Width * i / edgecount;
			accelerations[i] = 0;
			velocities[i] = 0;
			Body.SetPosition (i, new Vector3(xpositions[i], ypositions[i], z));
		}

		for (int i = 0; i < edgecount; i++) {
			meshes [i] = new Mesh ();

			Vector3[] Vertices = new Vector3[4];
			Vertices [0] = new Vector3 (xpositions [i], ypositions [i], z);
			Vertices [1] = new Vector3 (xpositions [i + 1], ypositions [i + 1], z);
			Vertices [2] = new Vector3 (xpositions [i], bottom, z);
			Vertices [3] = new Vector3 (xpositions [i + 1], bottom, z);

			Vector2[] UVs = new Vector2[4];
			UVs [0] = new Vector2 (0, 1);
			UVs [1] = new Vector2(1, 1);
			UVs [2] = new Vector2 (0, 0);
			UVs [3] = new Vector2 (1, 0);

			int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };
			meshes [i].vertices = Vertices;
			meshes [i].uv = UVs;
			meshes [i].triangles = tris;

			meshobjects[i] = Instantiate (watermesh, Vector3.zero, Quaternion.identity) as GameObject;
			meshobjects[i].GetComponent<MeshFilter>().mesh = meshes[i];
			meshobjects[i].transform.SetParent(transform);

			colliders[i] = new GameObject();
			colliders[i].name = "Trigger";
			colliders[i].AddComponent<BoxCollider2D>();
			colliders[i].transform.SetParent (transform);
			colliders[i].transform.position = new Vector3(Left + Width * (i + 0.5f) / edgecount, Top - 0.5f, 0);
			colliders[i].transform.localScale = new Vector3(Width / edgecount, 1, 1);
			colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
			colliders[i].AddComponent<WaterDetector>();
		}
	}

	void UpdateMeshes () {

		for (int i = 0; i < meshes.Length; i++) {
			Vector3[] Vertices = new Vector3[4];
			Vertices [0] = new Vector3 (xpositions [i], ypositions [i], z);
			Vertices [1] = new Vector3 (xpositions [i + 1], ypositions [i + 1], z);
			Vertices [2] = new Vector3 (xpositions [i], bottom, z);
			Vertices [3] = new Vector3 (xpositions [i + 1], bottom, z);

			meshes [i].vertices = Vertices;
		}
	}

	void FixedUpdate () {
		
		for (int i = 0; i < xpositions.Length; i++) {
			float force = springconstant * (ypositions [i] - baseheight) + velocities [i] * damping;
			accelerations [i] = -force;
			ypositions [i] += velocities [i];
			velocities [i] += accelerations [i];
			Body.SetPosition (i, new Vector3 (xpositions [i], ypositions [i], z));
		}

		float[] leftDeltas = new float[xpositions.Length];
		float[] rightDeltas = new float[xpositions.Length];

		for (int j = 0; j < 8; j++) {

			for (int i = 0; i < xpositions.Length; i++) {
				if (i > 0) {
					leftDeltas [i] = spread * (ypositions [i] - ypositions [i - 1]);
					velocities [i - 1] += leftDeltas [i];
				}
				if (i < xpositions.Length - 1) {
					rightDeltas[i] = spread * (ypositions[i] - ypositions[i + 1]);
					velocities[i+ 1] += rightDeltas[i];
				}
			}
		}

		for (int i = 0; i < xpositions.Length; i++) {
			if (i > 0) {
				ypositions [i - 1] += leftDeltas [i];
			}
			if (i < xpositions.Length - 1) {
				ypositions [i + 1] += rightDeltas [i];
			}
		}

		UpdateMeshes ();
	}

	public void Splash (float xpos, float velocity) {
		
		if (xpos >= xpositions [0] && xpos <= xpositions [xpositions.Length - 1]) {
			xpos -= xpositions [0];
			int index = Mathf.RoundToInt ((xpositions.Length - 1) * (xpos / (xpositions[xpositions.Length - 1] - xpositions[0])));
			velocities [index] = velocity;

			float lifetime = 0.93f + Mathf.Abs (velocity) * 0.07f;
			splash.GetComponent<ParticleSystem> ().startSpeed = 8 + 2 * Mathf.Pow (Mathf.Abs (velocity), 0.5f);
			splash.GetComponent<ParticleSystem> ().startSpeed = 9 + 2 * Mathf.Pow (Mathf.Abs (velocity), 0.5f);
			splash.GetComponent<ParticleSystem> ().startLifetime = lifetime;

//			Vector3 position = new Vector3 (xpositions [index], ypositions [index] - 0.35f, 5);
//			Quaternion rotation = Quaternion.LookRotation (new Vector3 (xpositions [Mathf.FloorToInt (xpositions.Length / 2)], baseheight + 8, 5) - position);

			Vector3 position = new Vector3 (xpositions [index], ypositions [index], 0);
			GameObject splish = Instantiate (splash, position, Quaternion.identity) as GameObject;
			Destroy (splish, lifetime + 0.3f);
		}
	}
}
