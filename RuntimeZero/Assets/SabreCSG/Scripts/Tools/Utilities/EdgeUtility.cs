#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sabresaurus.SabreCSG
{
	public static class EdgeUtility
	{
		public static bool SplitPolygonsByEdges(Polygon[] polygons, List<Edge> sourceEdges, out Polygon[] finalPolygons, out List<Edge> newEdges)
		{
			// First of all refine the list of edges to those that are on the polygons and share a polygon with another specified edge
			// Verification step, no more than two edges should be selected per face

			// Once the list of edges is refined, walk through each set of polygons
			// Where a polygon has two specified edges, it needs to be split in two
			// Where a polygon has one specified edge, it needs a vertex to be added
			List<Polygon> newPolygons = new List<Polygon>(polygons); // Complete set of new polygons
			newEdges = new List<Edge>(); // These are the new edges we create

			List<Edge> edges = new List<Edge>();

			// Pull out a list of edges that occur on any of the polygons at least twice.
			// This way we ignore edges on other brushes or edges which aren't possible to connect via a polygon
			for (int edge1Index = 0; edge1Index < sourceEdges.Count; edge1Index++) 
			{
				bool found = false;

				for (int i = 0; i < polygons.Length && !found; i++) 
				{
					Edge edge1 = sourceEdges[edge1Index];

					for (int edge2Index = 0; edge2Index < sourceEdges.Count && !found; edge2Index++) 
					{
						if(edge2Index != edge1Index) // Skip the same edge
						{
							Edge edge2 = sourceEdges[edge2Index];

							bool edge1Contained = Polygon.ContainsEdge(polygons[i], edge1);
							bool edge2Contained = Polygon.ContainsEdge(polygons[i], edge2);

							if(edge1Contained && edge2Contained)
							{
								if(!edges.Contains(edge1))
								{
									edges.Add(edge1);
								}

								if(!edges.Contains(edge2))
								{
									edges.Add(edge2);
								}
								found = true;
							}
						}
					}
				}
			}				

			// Now process each polygon
			for (int i = 0; i < polygons.Length; i++) 
			{
				Polygon polygon = polygons[i];

				List<Edge> edgesOnPolygon = new List<Edge>();
				for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++) 
				{
					Edge edge = edges[edgeIndex];
					if(Polygon.ContainsEdge(polygon, edge))
					{
						edgesOnPolygon.Add(edge);
					}
				}

				if(edgesOnPolygon.Count == 1)
				{
					Vertex newVertex;
					// Add vertex
					if(!SplitPolygonAtEdge(polygon, edgesOnPolygon[0], out newVertex))
					{
						Debug.LogError("Could not add vertex to adjacent polygon");
					}
				}
				else if(edgesOnPolygon.Count == 2)
				{
					// Split into two
					Edge edge1 = edgesOnPolygon[0];
					Edge edge2 = edgesOnPolygon[1];

					// First split the shared polygon
					Vector3 edge1Center = edge1.GetCenterPoint();
					Vector3 edge2Center = edge2.GetCenterPoint();

					Vector3 thirdPoint = edge1Center + polygon.Plane.normal;

					Plane splitPlane = new Plane(edge1Center, edge2Center, thirdPoint);

					Polygon splitPolygon1;
					Polygon splitPolygon2;
					Vertex edge1Vertex;
					Vertex edge2Vertex;

					Polygon.SplitPolygon(polygon, out splitPolygon1, out splitPolygon2, out edge1Vertex, out edge2Vertex, splitPlane);

					newEdges.Add(new Edge(edge1Vertex, edge2Vertex));
					newPolygons.Remove(polygon);
					newPolygons.Add(splitPolygon1);
					newPolygons.Add(splitPolygon2);
				}
			}

			finalPolygons = newPolygons.ToArray();

			return true;
		}

		public static bool SplitPolygonAtEdge(Polygon polygon, Edge edge, out Vertex newVertex)
		{
			newVertex = null;

			List<Vertex> vertices = new List<Vertex>(polygon.Vertices);
			for (int i = 0; i < polygon.Vertices.Length; i++) 
			{
				Vector3 position1 = polygon.Vertices[i].Position;
				Vector3 position2 = polygon.Vertices[(i+1)%polygon.Vertices.Length].Position;

				if((edge.Vertex1.Position.EqualsWithEpsilon(position1) && edge.Vertex2.Position.EqualsWithEpsilon(position2))
					|| (edge.Vertex1.Position.EqualsWithEpsilon(position2) && edge.Vertex2.Position.EqualsWithEpsilon(position1)))
				{
					newVertex = Vertex.Lerp(polygon.Vertices[i], polygon.Vertices[(i+1) % polygon.Vertices.Length], 0.5f);
					vertices.Insert(i+1, newVertex);
					break;
				}
			}

			if(vertices.Count == polygon.Vertices.Length)
			{
				// Could not add vertex to adjacent polygon
				return false;
			}

			polygon.SetVertices(vertices.ToArray());

			return true;
		}
	}
}
#endif