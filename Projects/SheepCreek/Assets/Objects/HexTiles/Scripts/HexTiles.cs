using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HexTiles : MonoBehaviour {
	private static readonly float cosine = Mathf.Cos(Mathf.PI / 6.0f);
	private static readonly float sine = Mathf.Sin(Mathf.PI / 6.0f);
	

	public List<HexTile> hexTilePrefabs;
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

	public List<HexTile> borderHexTilePrefabs;
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

		ShuffleRandom.shuffle(coordinates);

		// Place tiles.
		foreach(Coordinate coordinate in coordinates) {
			Vector3 position = calculatePosition(coordinate.u, coordinate.v);
			Quaternion rotation = Quaternion.identity;

			position.y = calculateHeight(coordinate.u, coordinate.v);

			if(accept(position)) {
				HexTile hexTile = Instantiate(hexTilePrefabs[Random.Range(0, hexTilePrefabs.Count)], position, rotation, transform);
				hexTile.transform.localScale = new Vector3(scale, 10.0f, scale);
				hexTile.name = "HexTile " + coordinate.u + " " + coordinate.v;
				
				hexTiles.Add(coordinate, hexTile);
			}
		}

		// Build navigation mesh.
		NavMeshSurface navigationMeshSurface = GetComponent<NavMeshSurface>();
		navigationMeshSurface.BuildNavMesh();

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
			
				HexTile hexTile = Instantiate(borderHexTilePrefabs[Random.Range(0, borderHexTilePrefabs.Count)], position, rotation, transform);
				hexTile.transform.localScale = new Vector3(scale, 10.0f, scale);
				hexTile.name = "Border HexTile " + borderCoordinate.u + " " + borderCoordinate.v;

				hexTile.border = true;
				
				hexTiles.Add(borderCoordinate, hexTile);
			}
		}
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
}
