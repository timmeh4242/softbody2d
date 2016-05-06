namespace FinerGames {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	[RequireComponent (typeof (Rigidbody2D))]
	[RequireComponent (typeof (PolygonCollider2D))]
	public class Softbody2D : MonoBehaviour {

	//	public Sprite m_Sprite;

		public int NumberOfPoints = 4;
		public PhysicsMaterial2D PhysicsMaterial;
		public float GravityScale = 1f;
		public float Drag = 0f;
		public float AngularDrag;
		public bool CollideConnected = false;
		public float DampingRatio = 0f;
		public float Frequency = 1f;
		public float Mass = 1f;
		public float Radius = 1f;

		public List<PointMass> m_PointMasses = new List<PointMass> ();
		public Rigidbody2D m_CentralPointMass;
		public PolygonCollider2D m_PolygonCollider2D;
//		Softmesh2D m_Softmesh2D;
		SoftSpriteMeshLink m_SoftSpriteMeshLink;
		public List<Triangle> Triangles = new List<Triangle> ();
		public int[] TriangleIndex;


	//	public float TotalSize;
	//	public float AverageSize;
	//	public float SizeDifference;
	//	public int SampleTime = 0;
	//	public int TimeSquashed = 0;
	//	public int AllowedTimeSquashed = 0;

		void Awake() {
			Generate (false);
			m_SoftSpriteMeshLink = this.GetComponent<SoftSpriteMeshLink> ();
		}
		// Use this for initialization
		void Start () {
			StartCoroutine (LateFixedUpdate ());
		}

		void OnDisable() {
			StopCoroutine (LateFixedUpdate ());
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		IEnumerator LateFixedUpdate () {

			var waitForFixedUpdate = new WaitForFixedUpdate();
			while (true)
			{
				yield return waitForFixedUpdate;

	//			if(SampleTime < 60) {
	//				TotalSize += m_PolyCollider.bounds.size.magnitude;
	//				SampleTime ++;
	//			}
	//
	//			if(SampleTime == 59) {
	//				AverageSize = TotalSize / SampleTime;
	//			}
	//
	//			if (SampleTime >= 60) {
	//				SizeDifference = Mathf.Abs (m_PolyCollider.bounds.size.magnitude - AverageSize);
	//				if (SizeDifference > .1f)
	//					TimeSquashed++;
	//				else
	//					TimeSquashed = 0;
	//
	//				if (TimeSquashed > AllowedTimeSquashed) {
	//					ClearForces ();
	//					SampleTime = 0;
	//					TimeSquashed = 0;
	//					TotalSize = 0f;
	//					AverageSize = 0f;
	//				}
	//			}

				UpdateCollider ();
//				m_Softmesh2D.UpdateMesh2 ();
				m_SoftSpriteMeshLink.UpdateMesh (m_PointMasses.ToArray ());
			}
		}

		void OnCollisionEnter2D (Collision2D arg1) {
	//		Debug.Log ("collided with " + arg1.gameObject.name + " at " + arg1.relativeVelocity + " velocity");
	//		Debug.Log (arg1.transform.parent.name);
		}

		void OnCollisionExit2D (Collision2D arg1) {}

		void OnCollisionStay2D (Collision2D arg1) {
	//		Debug.Log ("colliding with " + arg1);
	//		Debug.Log (arg1.transform.name);
		}

		public void Generate (bool IsEditor) {
			
			m_CentralPointMass = this.GetComponent<Rigidbody2D> ();
			m_CentralPointMass.mass = Mass;
			m_CentralPointMass.gravityScale = GravityScale;
			m_CentralPointMass.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

			m_PolygonCollider2D = this.GetComponent<PolygonCollider2D> ();

			List<GameObject> pointMassCache = new List<GameObject> ();
			foreach (Transform child in this.transform)
			{
				pointMassCache.Add (child.gameObject);
			}

			for (int i = 0; i < pointMassCache.Count; i++) {
				if (IsEditor) {
					DestroyImmediate (pointMassCache [i]);
				} else {
					Destroy (pointMassCache [i]);
				}
			}

			m_PointMasses.Clear ();

			var angle = 2 * Mathf.PI / NumberOfPoints;
			for (int i = 0; i < NumberOfPoints; i++) {
				var go = new GameObject ();
				var pm = go.AddComponent<FinerGames.PointMass> ();
				go.transform.SetParent (this.transform);
//				var ep = go.AddComponent<PointMass> ();
				var rb = go.AddComponent<Rigidbody2D> ();
				rb.mass = Mass;
				rb.gravityScale = GravityScale;
				rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

	//			ep.Collider = go.AddComponent<EdgeCollider2D> ();
	//			var cc = go.AddComponent<CircleCollider2D> ();
	//			cc.isTrigger = true;
	//			cc.radius = .0025f;

				var bc = go.AddComponent<BoxCollider2D> ();
				bc.size = new Vector2 (.01f, .01f);

				var x = Radius * Mathf.Cos (angle * -i);
				var y = Radius * Mathf.Sin (angle * -i);
				go.transform.localPosition = new Vector3 (x, y, 0f);
				go.transform.localScale = new Vector3 (1f, 1f, 1f);
				go.name = "PointMass (" + i + ")";
				m_PointMasses.Add (pm);
			}

//			var points = new Vector2[m_PointMasses.Count];
			TriangleIndex = new int[m_PointMasses.Count];
			for (int i = 0; i < m_PointMasses.Count; i++) {
				var centerSpring = m_PointMasses[i].gameObject.AddComponent<SpringJoint2D> ();
				centerSpring.connectedBody = m_CentralPointMass;
				centerSpring.autoConfigureDistance = false;
				centerSpring.enableCollision = false;

				var triangle = new Triangle ();
				TriangleIndex [i] = i;

				var cwSpring = m_PointMasses [i].gameObject.AddComponent<SpringJoint2D> ();
				cwSpring.enableCollision = false;
				var ccwSpring = m_PointMasses [i].gameObject.AddComponent<SpringJoint2D> ();
				ccwSpring.enableCollision = false;

				if (i == 0) {
					cwSpring.connectedBody = m_PointMasses [i + 1].GetComponent<Rigidbody2D> ();
					ccwSpring.connectedBody = m_PointMasses [m_PointMasses.Count - 1].GetComponent<Rigidbody2D> ();
					m_PointMasses [i].GetComponent<PointMass>().CWPointMass = m_PointMasses [i + 1].transform;

//					triangle.Vertices.Add(m_PointMasses [i]
				} else if (i == m_PointMasses.Count - 1) {
					cwSpring.connectedBody = m_PointMasses [0].GetComponent<Rigidbody2D> ();
					ccwSpring.connectedBody = m_PointMasses [i - 1].GetComponent<Rigidbody2D> ();
					m_PointMasses [i].GetComponent<PointMass>().CWPointMass = m_PointMasses [0].transform;
				} else {
					cwSpring.connectedBody = m_PointMasses [i + 1].GetComponent<Rigidbody2D> ();
					ccwSpring.connectedBody = m_PointMasses [i - 1].GetComponent<Rigidbody2D> ();
					m_PointMasses[i].GetComponent<PointMass>().CWPointMass = m_PointMasses [i + 1].transform;
				}

				cwSpring.autoConfigureDistance = false;
				ccwSpring.autoConfigureDistance = false;

				centerSpring.dampingRatio = DampingRatio;
				centerSpring.frequency = Frequency;

				cwSpring.dampingRatio = DampingRatio;
				cwSpring.frequency = Frequency;

				ccwSpring.dampingRatio = DampingRatio;
				ccwSpring.frequency = Frequency;

//				points[i] = m_PointMasses [i].transform.localPosition;
			}

			UpdateCollider ();
		}

		void UpdateCollider ()
		{
	//		for (int i = 0; i < PointMasses.Count; i++) {
	//			var ep = PointMasses [i].GetComponent<PointMass> ();
	//			var points2 = new Vector2[2];
	//			points2 [0] = Vector2.zero;
	//			points2 [1] = ep.transform.InverseTransformPoint (ep.LeftBody.position);
	//			ep.Collider.points = points2;
	//		}

			var points = new Vector2[m_PointMasses.Count];
			for (int i = 0; i < m_PointMasses.Count; i++) {
				points [i] = m_PointMasses [i].transform.localPosition;
			}
			m_PolygonCollider2D.points = points;
		}

		public void Reset () {

		}

		public void ClearForces () {
			m_CentralPointMass.velocity = Vector2.zero;

			for (int i = 0; i < m_PointMasses.Count; i++) {
				m_PointMasses [i].GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				m_PointMasses [i].transform.localPosition = m_PointMasses [i].GetComponent<PointMass> ().StartingPosition;
			}
		}
	}
}
