namespace FinerGames {

	using UnityEngine;
	using System.Collections;

	public class PointMass : MonoBehaviour {

		public Transform CWPointMass;
//		public Transform CCWPointMass;
		public Vector3 StartingPosition;
		Vector3 PreviousPosition;

		void Start () {
			StartingPosition = this.transform.localPosition;
		}

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
}
