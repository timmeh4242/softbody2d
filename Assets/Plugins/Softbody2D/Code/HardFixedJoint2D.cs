using UnityEngine;
using System.Collections;

public class HardFixedJoint2D : MonoBehaviour {

//	public Transform Anchor;
//	public Vector2 Offset;
	Vector2 StartingPosition;

	// Use this for initialization
	void Start () {
		StartingPosition = this.transform.localPosition;
		StartCoroutine (LateFixedUpdate());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator LateFixedUpdate() {
		var waitForFixedUpdate = new WaitForFixedUpdate ();

		while (true) {
			yield return waitForFixedUpdate;
			this.transform.localPosition = StartingPosition;
		}
	}
}
