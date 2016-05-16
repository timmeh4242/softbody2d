using UnityEngine;
using System.Collections;

public class WaterDetector : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D arg1) {

		if (arg1.GetComponent<Rigidbody2D> () != null) {
			transform.parent.GetComponent<Water2D> ().Splash (transform.position.x, arg1.attachedRigidbody.velocity.y * arg1.attachedRigidbody.mass / 40f);
		}
	}
}
