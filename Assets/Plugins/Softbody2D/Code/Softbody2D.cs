﻿namespace FinerGames {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	[RequireComponent (typeof (Rigidbody2D))]
	[RequireComponent (typeof (PolygonCollider2D))]
	public class Softbody2D : MonoBehaviour {

		public int NumberOfPoints = 4;
		public PhysicsMaterial2D PhysicsMaterial;
		public float GravityScale = 1f;
		public float Drag = 0f;
		public float AngularDrag = .05f;
		public bool CollideConnected = false;
		public float DampingRatio = 0f;
		public float Frequency = 1f;
		public float Mass = 1f;
		public float Radius = 1f;
		public bool m_IsKinematic = false;

//		public List<Transform> CollidedTransforms = new List<Transform> ();

		Transform PointMassContainer;
		[HideInInspector]
		public List<PointMass> m_PointMasses = new List<PointMass> ();

		[HideInInspector]
		public Rigidbody2D m_rigidbody;

		[HideInInspector]
		public PolygonCollider2D m_collider;

		DeformableSprite m_DeformableSprite;

		[HideInInspector]
		public List<Triangle> Triangles = new List<Triangle> ();

		[HideInInspector]
		public int[] TriangleIndex;

		void Awake() {
//			Generate (false);
			m_rigidbody = this.GetComponent<Rigidbody2D> ();
			m_collider = this.GetComponentInChildren<PolygonCollider2D> ();
			m_DeformableSprite = this.GetComponent<DeformableSprite> ();
		}

		void OnEnable () {
			StartCoroutine (LateFixedUpdate ());
		}

		void OnDisable() {
			StopCoroutine (LateFixedUpdate ());
		}

		IEnumerator LateFixedUpdate () {

			var waitForFixedUpdate = new WaitForFixedUpdate();
			while (true) {
				yield return waitForFixedUpdate;

				UpdateCollider ();
				if (m_DeformableSprite != null)
//					m_DeformableSprite.UpdateMesh (m_PointMasses.ToArray ());
					m_DeformableSprite.UpdateMesh ();
			}
		}

		public void OnCollisionEnter2DHandler (Collision2D arg1) {

			// removing notion of pre-filtering
			// - for now put the component that consumes the event in charge
			// - later on can add notion of filtered collision events to softbody that other components can optionally consume
//			if (CollidedTransforms.Contains (arg1.transform))
//				return;

//			CollidedTransforms.Add (arg1.transform);
			this.gameObject.SendMessage ("OnCollisionEnter2D", arg1, SendMessageOptions.DontRequireReceiver);
//			for (int i = 0; i < m_PointMasses.Count; i++) {
//				m_PointMasses[i].CenterSpring.reactionForce
//			}
		}

		public void OnCollisionExit2DHandler (Collision2D arg1) {

			// removing notion of pre-filtering
			// - for now put the component that consumes the event in charge
			// - later on can add notion of filtered collision events to softbody that other components can optionally consume
//			if (!CollidedTransforms.Contains (arg1.transform))
//				return;
//
//			if (m_collider.IsTouching (arg1.collider))
//				return;
//
//			if (m_PointMasses.Any (_ => _.m_collider.IsTouching (arg1.collider)))
//				return;
//
//			CollidedTransforms.Remove (arg1.transform);
			this.transform.SendMessage ("OnCollisionExit2D", arg1, SendMessageOptions.DontRequireReceiver);
		}

		public void OnCollisionStay2DHandler (Collision2D arg1) {
			this.transform.SendMessage ("OnCollisionStay2D", arg1, SendMessageOptions.DontRequireReceiver);
		}

		public void Generate (bool IsEditor) {

			var mass = Mass / NumberOfPoints;

			m_rigidbody = this.GetComponent<Rigidbody2D> ();
			m_rigidbody.mass = mass;
			m_rigidbody.gravityScale = GravityScale;
			m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

			m_collider = this.GetComponentInChildren<PolygonCollider2D> ();
			PointMassContainer = this.transform.FindChild ("PointMasses");
			if (PointMassContainer == null) {
				PointMassContainer = new GameObject ().transform;
				PointMassContainer.name = "PointMasses";
				PointMassContainer.SetParent (this.transform);
			}
			PointMassContainer.transform.localPosition = Vector3.zero;

			List<GameObject> pointMassCache = new List<GameObject> ();
			foreach (Transform child in this.transform)
			{
				if (child.GetComponent<PointMass> () == true)
					pointMassCache.Add (child.gameObject);
			}
			foreach (Transform child in PointMassContainer)
			{
				if (child.GetComponent<PointMass> () == true)
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
				go.transform.SetParent (PointMassContainer);
				var pm = go.AddComponent<PointMass> ();
				pm.ParentSoftbody2D = this;
				pm.m_rigidbody = go.AddComponent<Rigidbody2D> ();
				pm.m_rigidbody.mass = mass;
				pm.m_rigidbody.gravityScale = GravityScale;
				pm.m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				pm.m_rigidbody.drag = Drag;
				pm.m_rigidbody.angularDrag = AngularDrag;

				pm.m_collider = go.AddComponent<BoxCollider2D> ();
				pm.m_collider.size = new Vector2 (.01f, .01f);

				var x = Radius * Mathf.Cos (angle * -i);
				var y = Radius * Mathf.Sin (angle * -i);
				go.transform.localPosition = new Vector3 (x, y, 0f);
				go.transform.localScale = new Vector3 (1f, 1f, 1f);
				go.name = "PointMass (" + i + ")";
				m_PointMasses.Add (pm);
			}

			TriangleIndex = new int[m_PointMasses.Count];
			for (int i = 0; i < m_PointMasses.Count; i++) {
				m_PointMasses[i].CenterSpring = m_PointMasses[i].gameObject.AddComponent<SpringJoint2D> ();
				m_PointMasses[i].CenterSpring.autoConfigureDistance = false;
				m_PointMasses[i].CenterSpring.distance = 0f;
				m_PointMasses[i].CenterSpring.autoConfigureConnectedAnchor = false;
				m_PointMasses[i].CenterSpring.connectedBody = m_rigidbody;
				m_PointMasses[i].CenterSpring.connectedAnchor = m_PointMasses [i].transform.localPosition;
				m_PointMasses[i].CenterSpring.enableCollision = false;

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

				m_PointMasses[i].CenterSpring.dampingRatio = DampingRatio;
				m_PointMasses[i].CenterSpring.frequency = Frequency;

				cwSpring.dampingRatio = DampingRatio;
				cwSpring.frequency = Frequency;

				ccwSpring.dampingRatio = DampingRatio;
				ccwSpring.frequency = Frequency;
			}

			var anchorPoints = this.GetComponentsInChildren<AnchorPoint> ();
			for (int i = 0; i < anchorPoints.Length; i++) {

				var currentDistance = Mathf.Infinity;
				var shortestDistance = currentDistance;
				for (int j = 0; j < m_PointMasses.Count; j++) {
					currentDistance = Vector2.Distance (anchorPoints [i].transform.position, m_PointMasses [j].transform.position);
					if (currentDistance < shortestDistance) {
						anchorPoints [i].ConnectedPoint = m_PointMasses [j].transform;
						shortestDistance = currentDistance;
					}
				}

//				var closestPointMass = m_PointMasses
//					.Select(p => new { Point = p, Distance2 = p.transform.position.x * p.transform.position.x + p.transform.position.y * transform.position.y })
//					.Aggregate((p1, p2) => p1.Distance2 < p2.Distance2 ? p1 : p2)
//					.Point;
//				
//				anchorPoints [i].ConnectedPoint = closestPointMass.transform;

				// reference alternative
//				// project every element to get a map between it and the square of the distance
//				var map = pointsList                                            
//					.Select(p => new { Point = p, Distance = p.x * p.x + p.y * p.y });
//
//				var closestPoint = map // get the list of points with the min distance
//					.Where(m => m.Distance == map.Min(t => t.Distance)) 
//					.First() // get the first item in that list (guaranteed to exist)
//					.Point; // take the point
			}

			UpdateCollider ();
		}

		void UpdateCollider () {
			var points = new Vector2[m_PointMasses.Count];
			for (int i = 0; i < m_PointMasses.Count; i++) {
				points [i] = m_PointMasses [i].transform.localPosition;
			}
			m_collider.points = points;
		}

		public void ClearForces () {
			m_rigidbody.velocity = Vector2.zero;

			for (int i = 0; i < m_PointMasses.Count; i++) {
				m_PointMasses [i].GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				m_PointMasses [i].transform.localPosition = m_PointMasses [i].GetComponent<PointMass> ().StartingPosition;
			}
		}

		public void IsKinematic(bool value) {
			
			var drag = 0f;
			if (value == true) {
				drag = 10f;
			} else {
				drag = 0f;
			}

			m_rigidbody.isKinematic = value;
//			m_rigidbody.drag = drag;
			for (int i = 0; i < m_PointMasses.Count; i++) {
//				m_PointMasses [i].m_rigidbody.isKinematic = value;
				m_PointMasses [i].m_rigidbody.drag = drag;
			}
		}

		public void AddForce(Vector2 force) {
			m_rigidbody.AddForce (force);
			for (int i = 0; i < m_PointMasses.Count; i++) {
				m_PointMasses [i].m_rigidbody.AddForce (force);
			}
		}
	}
}
