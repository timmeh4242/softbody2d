using UnityEngine;
using System.Collections;

public class EdgePoint : MonoBehaviour {

	public Transform LeftBody;
	public Transform RightBody;
	public Vector3 StartingPosition;
	Vector3 PreviousPosition;
	SpringJoint2D spring;
	public EdgeCollider2D EdgeCollider;

	void Start () {
		StartingPosition = this.transform.localPosition;
		spring = this.GetComponent<SpringJoint2D> ();
		EdgeCollider = this.GetComponent<EdgeCollider2D> ();
	}

//	void FixedUpdate() {
//		var points = new Vector2[2];
//		points [0] = Vector2.zero;
//		points [1] = this.transform.InverseTransformPoint (LeftBody.position);
//		EdgeCollider.points = points;
//	}

	void OnCollisionEnter2D(Collision2D arg1) {
		this.transform.parent.SendMessage ("OnCollisionEnter2D", arg1);
	}

	void OnCollisionExit2D(Collision2D arg1) {
		this.transform.parent.SendMessage ("OnCollisionExit2D", arg1);
	}

	void OnCollisionStay2D(Collision2D arg1) {
		this.transform.parent.SendMessage ("OnCollisionStay2D", arg1);
	}
}
