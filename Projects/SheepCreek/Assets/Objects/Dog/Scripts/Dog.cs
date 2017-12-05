using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour {
	public GameObject boom;


	public void Start() {
		Camera.main.transform.SetParent(boom.transform);
		Camera.main.transform.localPosition = new Vector3();
		Camera.main.transform.localRotation = Quaternion.identity;
	}

	
	public void Update() {
		
	}
}
