#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sabresaurus.SabreCSG
{
	public static class VertexUtility
	{
		public static Polygon[] WeldVerticesToCenter(Polygon[] sourcePolygons, List<Vertex> sourceVertices)
		{
			// Picks the average position of the selected vertices and sets all their 
			// positions to that position. Duplicate vertices and polygons are then removed.
			VertexWeldOperation vertexOperation = new VertexWeldCentroidOperation(sourcePolygons, sourceVertices);
			return vertexOperation.Execute().ToArray();
		}

		public static Polygon[] WeldNearbyVertices(float tolerance, Polygon[] sourcePolygons, List<Vertex> sourceVertices)
		{
			// Takes the selected vertices and welds together any of them that are within the tolerance distance of 
			// other vertices. Duplicate vertices and polygons are then removed.
			VertexWeldOperation vertexOperation = new VertexWeldToleranceOperation(sourcePolygons, sourceVertices, tolerance);
			return vertexOperation.Execute().ToArray();
		}

		private static bool AreNeighbours(int index1, int index2, int length)
		{
			// First check the wrap points
			if(index1 == length-1 && index2 == 0
				|| index2 == length-1 && index1 == 0)
			{
				return true;
			}
			else if(index2-index1 == 1
				|| index1-index2 == 1) // Now check normally
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static Polygon[] ConnectVertices(Polygon[] polygons, List<Vertex> sourceVertices, out List<Edge> newEdges)
		{
			List<Polygon> newPolygons = new List<Polygon>(polygons);

			newEdges = new List<Edge>();

			for (int i = 0; i < newPolygons.Count; i++) 
			{
				// Source vertices on the polygon
				int matchedIndex1 = -1;
				int matchedIndex2 = -1;

				for (int j = 0; j < sourceVertices.Count; j++) 
				{
					int index = System.Array.IndexOf(newPolygons[i].Vertices, sourceVertices[j]);

					if(index != -1)
					{
						if(matchedIndex1 == -1)
						{
							matchedIndex1 = index;
						}
						else if(matchedIndex2 == -1)
						{
							matchedIndex2 = index;
						}
					}
				}

				// Check that found two valid points and that they're not neighbours 
				// (neighbouring vertices can't be connected as they already are by an edge)
				if(matchedIndex1 != -1 && matchedIndex2 != -1
					&& !AreNeighbours(matchedIndex1, matchedIndex2, newPolygons[i].Vertices.Length)) 
				{
//					Vertex neighbourVertex = newPolygons[i].Vertices[(matchedIndex1 + 1) % newPolygons[i].Vertices.Length];
//
//					Vector3 vector1 = newPolygons[i].Vertices[matchedIndex1].Position - neighbourVertex.Position;
//					Vector3 vector2 = newPolygons[i].Vertices[matchedIndex2].Position - newPolygons[i].Vertices[matchedIndex1].Position;
//					Vector3 normal = Vector3.Cross(vector1, vector2).normalized;
//
//					Vector3 thirdPoint = newPolygons[i].Vertices[matchedIndex1].Position + normal;
					Vector3 thirdPoint = newPolygons[i].Vertices[matchedIndex1].Position + newPolygons[i].Plane.normal;

					// First split the shared polygon
					Plane splitPlane = new Plane(newPolygons[i].Vertices[matchedIndex1].Position, newPolygons[i].Vertices[matchedIndex2].Position, thirdPoint);

					Polygon splitPolygon1;
					Polygon splitPolygon2;
					Vertex newVertex1;
					Vertex newVertex2;

					if(Polygon.SplitPolygon(newPolygons[i], out splitPolygon1, out splitPolygon2, out newVertex1, out newVertex2, splitPlane))
					{
						newPolygons[i] = splitPolygon1;
						newPolygons.Insert(i+1, splitPolygon2);
						// Skip over new polygon
						i++;
						newEdges.Add(new Edge(newVertex1, newVertex2));
					}
					else
					{
						Debug.LogWarning("Split polygon failed");
					}
				}
			}
			return newPolygons.ToArray();
		}

		public static void DisplacePolygons(Polygon[] polygons, float distance)
		{
			// Used for determining if two vertices are the same
			Polygon.VertexComparerEpsilon vertexComparer = new Polygon.VertexComparerEpsilon();
			// Used for determining if two positions or normals are the same
			Polygon.Vector3ComparerEpsilon vectorComparer = new Polygon.Vector3ComparerEpsilon();

			// Group overlapping positions and also track their normals
			List<List<Vertex>> groupedVertices = new List<List<Vertex>>();
			List<List<Vector3>> groupedNormals = new List<List<Vector3>>();

			// Maps back from a vertex to the polygon it came from, used for UV calculation
			Dictionary<Vertex, Polygon> vertexPolygonMappings = new Dictionary<Vertex, Polygon>();

			for (int polygonIndex = 0; polygonIndex < polygons.Length; polygonIndex++) 
			{
				Vertex[] vertices = polygons[polygonIndex].Vertices;

				// Group the selected vertices into clusters
				for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++) 
				{
					Vertex sourceVertex = vertices[vertexIndex];

					vertexPolygonMappings[sourceVertex] = polygons[polygonIndex];

					bool added = false;

					for (int groupIndex = 0; groupIndex < groupedVertices.Count; groupIndex++) 
					{
						if(groupedVertices[groupIndex].Contains(sourceVertex, vertexComparer))
						{
							groupedVertices[groupIndex].Add(sourceVertex);
							// Add the normal of the polygon if it hasn't already been added (this prevents issues with two polygons that are coplanar)
							if(!groupedNormals[groupIndex].Contains(polygons[polygonIndex].Plane.normal, vectorComparer))
							{
								groupedNormals[groupIndex].Add(polygons[polygonIndex].Plane.normal);
							}
							added = true;
							break;
						}
					}

					if(!added)
					{
						groupedVertices.Add(new List<Vertex>() { sourceVertex } );
						groupedNormals.Add(new List<Vector3>() { polygons[polygonIndex].Plane.normal } );
					}
				}
			}

			List<List<Vector3>> groupedPositions = new List<List<Vector3>>();
			List<List<Vector2>> groupedUV = new List<List<Vector2>>();

			// Calculate the new positions and UVs, but don't assign them as they must be calculated in one go
			for (int i = 0; i < groupedVertices.Count; i++) 
			{
				groupedPositions.Add(new List<Vector3>());
				groupedUV.Add(new List<Vector2>());

				for (int j = 0; j < groupedVertices[i].Count; j++) 
				{
					Vector3 position = groupedVertices[i][j].Position;
					for (int k = 0; k < groupedNormals[i].Count; k++) 
					{
						position += groupedNormals[i][k] * distance;
					}
					Polygon primaryPolygon = vertexPolygonMappings[groupedVertices[i][j]];

					Vector2 uv = GeometryHelper.GetUVForPosition(primaryPolygon, position);
					groupedPositions[i].Add(position);
					groupedUV[i].Add(uv);
				}
			}

			// Apply the new positions and UVs now that they've all been calculated
			for (int i = 0; i < groupedVertices.Count; i++) 
			{
				for (int j = 0; j < groupedVertices[i].Count; j++) 
				{
					Vertex vertex = groupedVertices[i][j];
					vertex.Position = groupedPositions[i][j];
					vertex.UV = groupedUV[i][j];
				}
			}

			// Polygon planes have moved, so recalculate them
			for (int polygonIndex = 0; polygonIndex < polygons.Length; polygonIndex++) 
			{
				polygons[polygonIndex].CalculatePlane();
			}
		}
	}
}
#endif