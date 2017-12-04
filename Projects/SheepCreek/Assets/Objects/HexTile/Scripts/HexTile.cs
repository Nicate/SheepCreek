using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour {
	// Should be unique.
	public string type;

	[System.Serializable]
	public struct HexTileWeight {
		public HexTile hexTile;
		public float weight;
	}

	public List<HexTileWeight> hexTileWeights;

	[HideInInspector]
	public bool border;


	public HexTile selectHexTile() {
		float totalWeight = 0.0f;

		foreach(HexTileWeight hexTileWeight in hexTileWeights) {
			totalWeight += hexTileWeight.weight;
		}

		float value = Random.value * totalWeight;

		float cumulativeWeight = 0.0f;

		foreach(HexTileWeight hexTileWeight in hexTileWeights) {
			cumulativeWeight += hexTileWeight.weight;

			if(value < cumulativeWeight) {
				return hexTileWeight.hexTile;
			}
		}

		// We're never gonna get here.
		return null;
	}
}
