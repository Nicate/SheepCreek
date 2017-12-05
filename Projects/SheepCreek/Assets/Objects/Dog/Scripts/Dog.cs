using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour {
	public GameObject boom;

	private Vector3 boomPosition;
	private Quaternion boomRotation;

	[HideInInspector]
	public Simulation simulation;


	public void Start() {
		Camera.main.transform.SetParent(boom.transform);
		Camera.main.transform.localPosition = new Vector3();
		Camera.main.transform.localRotation = Quaternion.identity;

		boomPosition = boom.transform.localPosition;
		boomRotation = boom.transform.rotation;
	}

	
	public void Update() {
		// Move.
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();

		Vector3 direction = new Vector3();

		if(Input.GetKey(KeyCode.LeftArrow)) {
			direction.x = -1.0f;

			sprite.flipX = false;
		}
		else if(Input.GetKey(KeyCode.RightArrow)) {
			direction.x = 1.0f;

			sprite.flipX = true;
		}

		if(Input.GetKey(KeyCode.DownArrow)) {
			direction.z = -1.0f;
		}
		else if(Input.GetKey(KeyCode.UpArrow)) {
			direction.z = 1.0f;
		}

		Vector3 target = transform.position + direction.normalized;

		agent.destination = target;

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

	public void LateUpdate() {
		// Fix boom.
		boom.transform.position = transform.position + boomPosition;
		boom.transform.rotation = boomRotation;
	}
}
