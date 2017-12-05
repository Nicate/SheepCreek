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

	public HexTiles hexTiles;

	public int numberOfSheep;

	
	private Dog dog;
	private List<Sheep> sheep = new List<Sheep>();


	public void Start() {
		dog = Instantiate(dogPrefab, hexTiles.getRandomPosition(), Quaternion.identity, transform);
		dog.name = "Dog";

		for(int count = 0; count < numberOfSheep; count++) {
			Sheep aSheep = Instantiate(selectSheep(), hexTiles.getRandomPosition(), Quaternion.identity, transform);
			aSheep.name = "Sheep" + count;
			sheep.Add(aSheep);
		}
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
