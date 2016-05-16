using UnityEngine;
using System.Collections;

public class AnchorPoint : MonoBehaviour {

	public Transform ConnectedPoint;
	public Vector3 Offset;
	public bool AutoConfigureOffset = true;

	void OnValidate () {
		if (ConnectedPoint == null)
			return;

		if(AutoConfigureOffset == true)
			Offset = this.transform.localPosition - ConnectedPoint.localPosition;
	}

	void OnEnable () {
		if (ConnectedPoint == null)
			return;

		Offset = this.transform.localPosition - ConnectedPoint.localPosition;
		StartCoroutine (LateFixedUpdate ());
	}

	void OnDisable() {
		StopCoroutine (LateFixedUpdate ());
	}

	IEnumerator LateFixedUpdate () {

		var waitForFixedUpdate = new WaitForFixedUpdate();
		while (true) {
			yield return waitForFixedUpdate;

			if(ConnectedPoint != null)
				this.transform.localPosition = ConnectedPoint.localPosition + Offset;
		}
	}
}
