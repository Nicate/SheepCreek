using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HexTiles : MonoBehaviour {
	private static readonly float cosine = Mathf.Cos(Mathf.PI / 6.0f);
	private static readonly float sine = Mathf.Sin(Mathf.PI / 6.0f);
	

	[System.Serializable]
	public struct HexTileWeight {
		public HexTile hexTile;
		public float weight;
	}

	public List<HexTileWeight> hexTileWeights;
	public float radius;
	public float scale;

	public int uExtent;
	public int vExtent;


	private struct Circle {
		public Vector2 position;
		public float radius;
	}

	private List<Circle> circles = new List<Circle>();

	public int numberOfCircles;
	public float circleRadiusMinimum;
	public float circleRadiusMaximum;

	private struct Coordinate {
		public int u;
		public int v;

		public Coordinate(int u, int v) {
			this.u = u;
			this.v = v;
		}
		
		public override bool Equals(object obj) {
			if(obj == null || GetType() != obj.GetType()) {
				return false;
			}

			Coordinate that = (Coordinate) obj;

			return that.u == u && that.v == v;
		}
		
		public override int GetHashCode() {
			return u + 31 * v;
		}
	}

	private Dictionary<Coordinate, HexTile> hexTiles = new Dictionary<Coordinate, HexTile>();

	public float minimumHeight;
	public float maximumHeight;

	public float heightMean;
	public float heightDeviation;

	public List<HexTileWeight> borderHexTileWeights;
	public int borderWidth;
	

	public void Start () {
		// Calculate the closest bound.
		float maximumDistance = calculatePosition(uExtent, -vExtent).magnitude;

		for(int count = 0; count < numberOfCircles; count++) {
			Circle circle = new Circle();

			circle.position = Random.insideUnitCircle * (maximumDistance - circleRadiusMaximum);
			circle.radius = Random.Range(circleRadiusMinimum, circleRadiusMaximum);

			circles.Add(circle);
		}

		// Shuffle the list of coordinates.
		List<Coordinate> coordinates = new List<Coordinate>();

		for(int v = -vExtent; v <= vExtent; v++) {
			for(int u = -uExtent; u <= uExtent; u++) {
				coordinates.Add(new Coordinate(u, v));
			}
		}

		ListRandom.shuffle(coordinates);

		// Place tiles.
		foreach(Coordinate coordinate in coordinates) {
			Vector3 position = calculatePosition(coordinate.u, coordinate.v);
			Quaternion rotation = Quaternion.identity;

			position.y = calculateHeight(coordinate.u, coordinate.v);

			if(accept(position)) {
				HexTile hexTile = Instantiate(selectHexTile(coordinate.u, coordinate.v), position, rotation, transform);
				hexTile.transform.localScale = new Vector3(scale, scale, scale);
				hexTile.name = "HexTile " + coordinate.u + " " + coordinate.v;
				
				hexTiles.Add(coordinate, hexTile);
			}
		}

		// Make borders.
		for(int count = 0; count < borderWidth; count++) {
			HashSet<Coordinate> borderCoordinates = new HashSet<Coordinate>();

			foreach(Coordinate coordinate in hexTiles.Keys) {
				borderCoordinates.UnionWith(calculateBorder(coordinate.u, coordinate.v));
			}

			foreach(Coordinate borderCoordinate in borderCoordinates) {
				Vector3 position = calculatePosition(borderCoordinate.u, borderCoordinate.v);
				Quaternion rotation = Quaternion.identity;

				position.y = calculateHeight(borderCoordinate.u, borderCoordinate.v);
			
				HexTile hexTile = Instantiate(selectBorderHexTile(borderCoordinate.u, borderCoordinate.v), position, rotation, transform);
				hexTile.transform.localScale = new Vector3(scale, scale, scale);
				hexTile.name = "Border HexTile " + borderCoordinate.u + " " + borderCoordinate.v;

				hexTile.border = true;
				
				hexTiles.Add(borderCoordinate, hexTile);
			}
		}

		// Build navigation mesh.
		NavMeshSurface navigationMeshSurface = GetComponent<NavMeshSurface>();
		navigationMeshSurface.BuildNavMesh();
	}


	private bool accept(Vector3 position) {
		Vector2 point = new Vector2(position.x, position.z);

		foreach(Circle circle in circles) {
			Vector2 localPoint = point - circle.position;

			if(localPoint.magnitude < circle.radius) {
				return true;
			}
		}

		return false;
	}


	private Vector3 calculatePosition(int u, int v) {
		Vector3 position = new Vector3();

		position.x = (u + v) * cosine * radius * scale;
		position.z = (u - v) * sine * radius * scale;

		return position;
	}


	private float calculateHeight(int u, int v) {
		float totalHeight = 0.0f;
		int numberOfNeighbours = 0;
		
		for(int q = -1; q <= 1; q++) {
			for(int p = -1; p <= 1; p++) {
				// Skip ourselves and the two outliers.
				if(p != q) {
					Coordinate neighbour = new Coordinate(u + p, v + q);
					
					if(hexTiles.ContainsKey(neighbour)) {
						totalHeight += hexTiles[neighbour].transform.position.y;
						numberOfNeighbours += 1;
					}
				}
			}
		}

		if(numberOfNeighbours > 0) {
			return (totalHeight / numberOfNeighbours) + (GaussianRandom.value * heightDeviation + heightMean);
		}
		else {
			return Random.Range(minimumHeight, maximumHeight);
		}
	}


	private List<Coordinate> calculateBorder(int u, int v) {
		List<Coordinate> coordinates = new List<Coordinate>();
		
		for(int q = -1; q <= 1; q++) {
			for(int p = -1; p <= 1; p++) {
				// Skip ourselves and the two outliers.
				if(p != q) {
					Coordinate neighbour = new Coordinate(u + p, v + q);

					if(!hexTiles.ContainsKey(neighbour)) {
						coordinates.Add(neighbour);
					}
				}
			}
		}

		return coordinates;
	}


	private HexTile selectHexTile(int u, int v) {
		List<HexTile> neighbours = new List<HexTile>();
		
		for(int q = -1; q <= 1; q++) {
			for(int p = -1; p <= 1; p++) {
				// Skip ourselves and the two outliers.
				if(p != q) {
					Coordinate neighbour = new Coordinate(u + p, v + q);

					if(hexTiles.ContainsKey(neighbour)) {
						neighbours.Add(hexTiles[neighbour]);
					}
				}
			}
		}

		if(neighbours.Count > 0) {
			// This is like performing a weighted selection (since it is uniformly distributed) but WAY easier.
			HexTile neighbour = ListRandom.select(neighbours);

			// Now the actual weighted selection (see how much easier it is now);
			return neighbour.selectHexTile();
		}
		else {
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

			// We canna get here cap'n!
			return null;
		}
	}


	private HexTile selectBorderHexTile(int u, int v) {
		float totalWeight = 0.0f;

		foreach(HexTileWeight borderHexTileWeight in borderHexTileWeights) {
			totalWeight += borderHexTileWeight.weight;
		}

		float value = Random.value * totalWeight;

		float cumulativeWeight = 0.0f;

		foreach(HexTileWeight borderHexTileWeight in borderHexTileWeights) {
			cumulativeWeight += borderHexTileWeight.weight;

			if(value < cumulativeWeight) {
				return borderHexTileWeight.hexTile;
			}
		}

		// We canna get here cap'n!
		return null;
	}


	public Vector3 getRandomPosition() {
		// Find a non-border hextile.
		HexTile hexTile;
		do {
			hexTile = ListRandom.select(new List<HexTile>(hexTiles.Values));
		}
		while(hexTile.border);

		// Find the closest navigable point.
		NavMeshHit hit;
		if(NavMesh.SamplePosition(hexTile.transform.position, out hit, radius * scale, -1)) {
			// For some reason SamplePosition does not return a position ON the NavMesh.
			return new Vector3(hit.position.x, hexTile.transform.position.y, hit.position.z);
		}
		else {
			Debug.Log("Could not sample NavMesh position.");

			return hexTile.transform.position;
		}
	}
}
