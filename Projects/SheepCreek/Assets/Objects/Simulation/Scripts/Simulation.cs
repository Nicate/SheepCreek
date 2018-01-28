using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Simulation : MonoBehaviour {
	public Dog dogPrefab;

	[System.Serializable]
	public struct SheepWeight {
		public Sheep sheep;
		public float weight;
	}

	public List<SheepWeight> sheepWeights;

	public int numberOfSheep;


	public HexTiles hexTiles;

	public Music music;

	
	[HideInInspector]
	public Dog dog;
	
	[HideInInspector]
	public List<Sheep> sheep = new List<Sheep>();


	public int level2Count;
	public int level3Count;
	public int level4Count;

	private int oldSheepCount = 0;


	public Text text;


	public void Start() {
		dog = Instantiate(dogPrefab, hexTiles.getRandomPosition(), Quaternion.identity, transform);
		dog.name = "Dog";
		dog.simulation = this;

		for(int count = 0; count < numberOfSheep; count++) {
			Sheep aSheep = Instantiate(selectSheep(), hexTiles.getRandomPosition(), Quaternion.identity, transform);
			aSheep.name = "Sheep" + count;
			aSheep.simulation = this;
			sheep.Add(aSheep);
		}
	}
	
	public void Update() {
		// We can only go up right now.
		if(oldSheepCount < level2Count && sheep.Count >= level2Count) {
			music.increaseLevel();
		}

		if(oldSheepCount < level3Count && sheep.Count >= level3Count) {
			music.increaseLevel();
		}

		if(oldSheepCount < level4Count && sheep.Count >= level4Count) {
			music.increaseLevel();
		}

		oldSheepCount = sheep.Count;

		// TODO This is just a basic win/score condition.
		if(Time.timeSinceLevelLoad > 120.0f) {
			Time.timeScale = 0.0f;

			text.text = "Your score: " + sheep.Count + "\n\nPress Alt-F4 to close the game.";

			text.enabled = true;
		}
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


	public void makeBaby(Vector3 position) {
		Sheep aSheep = Instantiate(selectSheep(), position, Quaternion.identity, transform);
		aSheep.name = "Sheep" + sheep.Count;
		aSheep.simulation = this;
		sheep.Add(aSheep);
	}
}
