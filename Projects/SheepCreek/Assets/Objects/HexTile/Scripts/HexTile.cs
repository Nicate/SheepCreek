using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour {
	// Should be unique.
	public string type;

	[System.Serializable]
	public struct TypeWeight {
		public string type;
		public float weight;
	}

	public List<TypeWeight> typeWeights;

	[HideInInspector]
	public bool border;


	public string selectType() {
		float totalWeight = 0.0f;

		foreach(TypeWeight typeWeight in typeWeights) {
			totalWeight += typeWeight.weight;
		}

		float value = Random.value * totalWeight;

		float cumulativeWeight = 0.0f;

		foreach(TypeWeight typeWeight in typeWeights) {
			cumulativeWeight += typeWeight.weight;

			if(value < cumulativeWeight) {
				return typeWeight.type;
			}
		}

		return "We're never gonna get here.";
	}
}
