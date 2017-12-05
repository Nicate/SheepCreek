using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour {
	private float scared;

	[HideInInspector]
	public Simulation simulation;

	public float rate;

	private Vector3 randomTarget;


	public void Start () {
		scared = 0.0f;

		randomTarget = transform.position;
	}
	

	public void Update () {
		float factor = Time.deltaTime * rate;

		Vector3 target = transform.position;

		Vector3 delta = simulation.dog.transform.position - transform.position;
		
		Vector3 direction = delta.normalized;
		float distance = delta.magnitude;

		bool spooked = false;

		if(distance < 1.0f + scared * 2) {
			target -= direction;

			increaseScare(0.1f * factor);

			spooked = true;
		}

		// Wobble.
		if(Random.value < 0.1f * factor) {
			Vector3 wobble = Random.insideUnitSphere;
			wobble.y = 0.0f;
			randomTarget = transform.position + wobble * (1.0f + scared);
		}

		// Move.
		if(spooked) {
			randomTarget = target;
		}

		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		agent.destination = randomTarget;

		// Whew.
		decreaseScare(0.01f * factor);

		// Babies.
		if(Random.value < 0.01f * factor) {
			foreach(Sheep sheep in simulation.sheep) {
				if(sheep != this && (sheep.transform.position - transform.position).magnitude < simulation.hexTiles.radius * simulation.hexTiles.scale) {
					simulation.makeBaby((sheep.transform.position + transform.position) / 2.0f);
					break;
				}
			}
		}

		// Flip.
		SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();

		Vector3 facing = randomTarget - transform.position;

		if(facing.x < -0.1f) {
			sprite.flipX = false;
		}
		else if(facing.x > 0.1f) {
			sprite.flipX = true;
		}
	}


	public void increaseScare(float scary) {
		scared += scary;

		scared = Mathf.Clamp01(scared);
	}
	
	public void decreaseScare(float scary) {
		scared -= scary;

		scared = Mathf.Clamp01(scared);
	}
}
