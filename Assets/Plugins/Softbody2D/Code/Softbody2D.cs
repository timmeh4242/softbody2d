using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody2D))]
public class Softbody2D : MonoBehaviour {

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

	List<GameObject> EdgePoints = new List<GameObject> ();
	Rigidbody2D m_Rigidbody2D;

	public float TotalSize;
	public float AverageSize;
	public float SizeDifference;
	public int SampleTime = 0;
	public int TimeSquashed = 0;
	public int AllowedTimeSquashed = 0;

	void Awake() {
		Generate (false);
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

			for (int i = 0; i < EdgePoints.Count; i++) {
				var ep = EdgePoints [i].GetComponent<EdgePoint> ();
				var points = new Vector2[2];
				points [0] = Vector2.zero;
				points [1] = ep.transform.InverseTransformPoint (ep.LeftBody.position);
				ep.EdgeCollider.points = points;
			}
		}
	}

	void OnCollisionEnter2D (Collision2D arg1) {
		Debug.Log ("collided with " + arg1.gameObject.name + " at " + arg1.relativeVelocity + " velocity");
	}

	void OnCollisionStay2D (Collision2D arg1) {
		Debug.Log ("colliding with " + arg1);
	}

	public void Generate (bool IsEditor) {
		m_Rigidbody2D = this.GetComponent<Rigidbody2D> ();
		m_Rigidbody2D.mass = Mass;
		m_Rigidbody2D.gravityScale = GravityScale;
		m_Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

		foreach (Transform child in this.transform)
		{
			EdgePoints.Add (child.gameObject);
		}

		for (int i = 0; i < EdgePoints.Count; i++) {
			if (IsEditor) {
				DestroyImmediate (EdgePoints [i]);
			} else {
				Destroy (EdgePoints[i]);
			}
		}

		EdgePoints.Clear ();

		var angle = 2 * Mathf.PI / NumberOfPoints;
		for (int i = 0; i < NumberOfPoints; i++) {
			var go = new GameObject ();
			go.transform.SetParent (this.transform);
			go.AddComponent<EdgePoint> ();
			var rb = go.AddComponent<Rigidbody2D> ();
			rb.mass = Mass;
			rb.gravityScale = GravityScale;
			rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			go.AddComponent<EdgeCollider2D> ();
			var cc = go.AddComponent<CircleCollider2D> ();
			cc.isTrigger = true;
			cc.radius = .0025f;
			var x = Radius * Mathf.Cos (angle * i);
			var y = Radius * Mathf.Sin (angle * i);
			go.transform.localPosition = new Vector3 (x, y, 0f);
			go.transform.localScale = new Vector3 (1f, 1f, 1f);
			go.name = "EdgePoint (" + i + ")";
			EdgePoints.Add (go);
		}

		var points = new Vector2[EdgePoints.Count];

		for (int i = 0; i < EdgePoints.Count; i++) {
			var centerSpring = EdgePoints[i].AddComponent<SpringJoint2D> ();
			centerSpring.connectedBody = m_Rigidbody2D;
			centerSpring.autoConfigureDistance = false;

			var leftSpring = EdgePoints[i].AddComponent<SpringJoint2D> ();
			var rightSpring = EdgePoints[i].AddComponent<SpringJoint2D> ();

			if (i == 0) {
				leftSpring.connectedBody = EdgePoints [EdgePoints.Count - 1].GetComponent<Rigidbody2D> ();
				rightSpring.connectedBody = EdgePoints [i + 1].GetComponent<Rigidbody2D> ();
				EdgePoints[i].GetComponent<EdgePoint>().LeftBody = EdgePoints [EdgePoints.Count - 1].transform;
			} else if (i == EdgePoints.Count - 1) {
				leftSpring.connectedBody = EdgePoints [i - 1].GetComponent<Rigidbody2D> ();
				rightSpring.connectedBody = EdgePoints [0].GetComponent<Rigidbody2D> ();
				EdgePoints[i].GetComponent<EdgePoint>().LeftBody = EdgePoints [i - 1].transform;
			} else {
				leftSpring.connectedBody = EdgePoints [i - 1].GetComponent<Rigidbody2D> ();
				rightSpring.connectedBody = EdgePoints [i + 1].GetComponent<Rigidbody2D> ();
				EdgePoints[i].GetComponent<EdgePoint>().LeftBody = EdgePoints [i - 1].transform;
			}

			leftSpring.autoConfigureDistance = false;
			rightSpring.autoConfigureDistance = false;

			centerSpring.dampingRatio = DampingRatio;
			centerSpring.frequency = Frequency;
			leftSpring.dampingRatio = DampingRatio;
			leftSpring.frequency = Frequency;
			rightSpring.dampingRatio = DampingRatio;
			rightSpring.frequency = Frequency;

			points[i] = EdgePoints [i].transform.localPosition;
		}
	}

	public void Reset () {

	}

	public void ClearForces() {
		m_Rigidbody2D.velocity = Vector2.zero;

		for (int i = 0; i < EdgePoints.Count; i++) {
			EdgePoints [i].GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
			EdgePoints [i].transform.localPosition = EdgePoints [i].GetComponent<EdgePoint> ().StartingPosition;
		}
	}
}
