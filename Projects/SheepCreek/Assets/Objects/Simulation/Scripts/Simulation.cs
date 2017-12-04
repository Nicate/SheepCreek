using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour {
	public Dog dogPrefab;

	[System.Serializable]
	public struct SheepWeight {
		public Sheep sheep;
		public float weight;
	}

	public List<SheepWeight> sheepWeights;


	public void Start() {
		
	}
	
	public void Update() {
		
	}


	private Sheep selectSheep() {
		float totalWeight = 0.0f;

		foreach(SheepWeight sheepWeight in sheepWeights) {
			totalWeight += sheepWeight.weight;
		}

		float value = Random.value * totalWeight;

		float cumulativeWeight = 0.0f;

		foreach(SheepWeight sheepWeight in sheepWeights) {
			cumulativeWeight += sheepWeight.weight;

			if(value < cumulativeWeight) {
				return sheepWeight.sheep;
			}
		}

		// We canna get here cap'n!
		return null;
	}
}
