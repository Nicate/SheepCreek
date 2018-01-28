using UnityEngine;

public class Billboard : MonoBehaviour {
	public Camera billboardCamera;


	public void Start () {
		// Cheat it since we are working with prefabs.
		if(billboardCamera == null) {
			billboardCamera = Camera.main;
		}

		updateRotation();
	}
	

	public void LateUpdate () {
		updateRotation();
	}


	private void updateRotation() {
		transform.forward = billboardCamera.transform.forward;
		transform.right = billboardCamera.transform.right;
		transform.up = billboardCamera.transform.up;
	}
}
