namespace FinerGames {

	using UnityEngine;
	using System.Collections;

	public class PointMass : MonoBehaviour {

		public Softbody2D ParentSoftbody2D;
		public BoxCollider2D m_collider;
		public Rigidbody2D m_rigidbody;
		public Transform CWPointMass;
		public Transform CCWPointMass;
		public Vector3 StartingPosition;
		Vector3 PreviousPosition;

		[HideInInspector]
		public SpringJoint2D CenterSpring;

		[HideInInspector]
		public SpringJoint2D CWSpring;

		[HideInInspector]
		public SpringJoint2D CCWSpring;

		void Awake() {
			StartingPosition = this.transform.localPosition;
			ParentSoftbody2D = this.transform.GetComponentInParent<Softbody2D> ();
			m_collider = this.GetComponent<BoxCollider2D> ();
		}

		void OnCollisionEnter2D(Collision2D arg1) {
			if (arg1.transform == this.transform.parent || arg1.transform.parent == this.transform.parent)
				return;
			
			ParentSoftbody2D.OnCollisionEnter2DHandler (arg1);
		}

		void OnCollisionExit2D(Collision2D arg1) {
			if (arg1.transform == this.transform.parent || arg1.transform.parent == this.transform.parent)
				return;

			ParentSoftbody2D.OnCollisionExit2DHandler (arg1);
		}

		void OnCollisionStay2D(Collision2D arg1) {
			if (arg1.transform == this.transform.parent || arg1.transform.parent == this.transform.parent)
				return;
			
			ParentSoftbody2D.OnCollisionStay2DHandler (arg1);
		}
	}
}
