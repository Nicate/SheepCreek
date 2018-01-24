using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour {
	[HideInInspector]
	public Simulation simulation;

	
	public void Update() {
		// Move.
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		
		if(Input.GetMouseButtonDown(0)) {
			Vector3? target = simulation.hexTiles.pickSurface();

			if(target.HasValue) {
				agent.destination = target.Value;
			}
		}

		// Flip.
		SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();

		Vector3 facing = agent.destination - transform.position;

		if(facing.x < -0.1f) {
			sprite.flipX = false;
		}
		else if(facing.x > 0.1f) {
			sprite.flipX = true;
		}

		// Woof.
		if(Input.GetKey(KeyCode.Space)) {
			AudioSource[] woofs = GetComponents<AudioSource>();

			bool play = true;

			foreach(AudioSource woof in woofs) {
				if(woof.isPlaying) {
					play = false;
				}
			}

			if(play) {
				AudioSource woof = woofs[Random.Range(0, woofs.Length)];

				woof.Play();

				// Scary woof.
				foreach(Sheep sheep in simulation.sheep) {
					if((transform.position - sheep.transform.position).magnitude < simulation.hexTiles.radius * simulation.hexTiles.scale * 2.0f) {
						sheep.increaseScare(0.5f);
					}
				}
			}
		}
	}
}
