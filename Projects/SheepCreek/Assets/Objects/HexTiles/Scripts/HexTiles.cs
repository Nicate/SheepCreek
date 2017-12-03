using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HexTiles : MonoBehaviour {
	private static readonly float cosine = Mathf.Cos(Mathf.PI / 6.0f);
	private static readonly float sine = Mathf.Sin(Mathf.PI / 6.0f);
	

	public GameObject hexTilePrefab;
	public float radius;

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

	private Dictionary<Coordinate, GameObject> heights = new Dictionary<Coordinate, GameObject>();

	public float minimumHeight;
	public float maximumHeight;

	public float heightMean;
	public float heightDeviation;


	public void Start () {
		// Closest bound.
		float maximumDistance = calculatePosition(uExtent, -vExtent).magnitude;

		for(int count = 0; count < numberOfCircles; count++) {
			Circle circle = new Circle();

			circle.position = Random.insideUnitCircle * (maximumDistance - circleRadiusMaximum);
			circle.radius = Random.Range(circleRadiusMinimum, circleRadiusMaximum);

			circles.Add(circle);
		}

		List<Coordinate> coordinates = new List<Coordinate>();

		for(int v = -vExtent; v <= vExtent; v++) {
			for(int u = -uExtent; u <= uExtent; u++) {
				coordinates.Add(new Coordinate(u, v));
			}
		}

		ShuffleRandom.shuffle(coordinates);

		foreach(Coordinate coordinate in coordinates) {
			Vector3 position = calculatePosition(coordinate.u, coordinate.v);
			Quaternion rotation = Quaternion.identity;

			position.y = calculateHeight(coordinate.u, coordinate.v);

			if(accept(position)) {
				GameObject hexTile = Instantiate(hexTilePrefab, position, rotation, transform);
				hexTile.name = "HexTile " + coordinate.u + " " + coordinate.v;

				heights.Add(coordinate, hexTile);
			}
		}

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

		position.x = (u + v) * cosine * radius;
		position.z = (u - v) * sine * radius;

		return position;
	}


	private float calculateHeight(int u, int v) {
		float totalHeight = 0.0f;
		int numberOfNeighbours = 0;

		for(int q = v - 1; q <= v + 1; q++) {
			for(int p = u - 1; p <= u + 1; p++) {
				// We ourselves are also a neighbour but since we don't exist yet there is no height.
				Coordinate neighbour = new Coordinate(p, q);

				if(heights.ContainsKey(neighbour)) {
					totalHeight += heights[neighbour].transform.position.y;
					numberOfNeighbours += 1;
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
}
