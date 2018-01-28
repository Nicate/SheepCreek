using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour {
	[HideInInspector]
	public Simulation simulation;

	public float rate;


	private struct Target {
		public Vector3 position;
		public float weight;
	}

	private Dictionary<string, Target> targets = new Dictionary<string, Target>();

	private Vector3 randomTarget;


	private float scared;


	private NavMeshAgent agent;


	public void Start () {
		scared = 0.0f;

		randomTarget = transform.position;

		agent = GetComponent<NavMeshAgent>();
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

		addTarget("random", randomTarget, 1.0f);

		// Move to weighted (local) average of all destinations.
		agent.destination = getDestination();

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


	private Target getTarget(string name) {
		return targets[name];
	}

	private void addTarget(string name, Vector3 position, float weight) {
		Target target = new Target();

		target.position = position;
		target.weight = weight;

		targets[name] = target;
	}


	private Vector3 getDestination() {
		Vector3 position = new Vector3();

		foreach(Target target in targets.Values) {
			position += (target.position - transform.position) * target.weight;
		}

		return position + transform.position;
	}
}
