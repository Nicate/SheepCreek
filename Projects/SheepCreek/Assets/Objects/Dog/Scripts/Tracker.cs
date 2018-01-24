using UnityEngine;

public class Tracker : MonoBehaviour {
	public Simulation simulation;


	public void LateUpdate () {
		transform.position = simulation.dog.transform.position;
	}
}
