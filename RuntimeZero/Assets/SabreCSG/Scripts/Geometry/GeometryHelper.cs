#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Sabresaurus.SabreCSG
{
	public struct PolygonRaycastHit
	{
		public Vector3 Point;
		public Vector3 Normal;
		public float Distance;
		public GameObject GameObject;
	}

	public static class GeometryHelper
	{
		public static Polygon RaycastPolygons(List<Polygon> polygons, Ray ray, out float hitDistance, float polygonSkin = 0)
		{
			Polygon closestPolygon = null;
			float closestSquareDistance = float.PositiveInfinity;
			hitDistance = 0;

			if(polygons != null)
			{
				// 
				for (int i = 0; i < polygons.Count; i++) 
				{
					if(polygons[i].ExcludeFromFinal)
					{
						continue;
					}

					// Skip any polygons that are facing away from the ray
					if(Vector3.Dot(polygons[i].Plane.normal, ray.direction) > 0)
					{
						continue;
					}

					if(GeometryHelper.RaycastPolygon(polygons[i], ray, polygonSkin))
					{
						// Get the real hit point by testing the ray against the polygon's plane
						Plane plane = polygons[i].Plane;

						float rayDistance;
						plane.Raycast(ray, out rayDistance);
						Vector3 hitPoint = ray.GetPoint(rayDistance);

						// Find the square distance from the camera to the hit point (squares used for speed)
						float squareDistance = (ray.origin - hitPoint).sqrMagnitude;
						// If the distance is closer than the previous closest polygon, use this one.
						if(squareDistance < closestSquareDistance)
						{
							closestPolygon = polygons[i];
							closestSquareDistance = squareDistance;
							hitDistance = rayDistance;
						}
					}
				}
			}

			return closestPolygon;
		}

		public static bool RaycastPolygon(Polygon polygon, Ray ray, float polygonSkin = 0)
		{
			// TODO: This probably won't work if the ray and polygon are coplanar, but right now that's not a usecase
//			polygon.CalculatePlane();
			Plane plane = polygon.Plane;
			float distance = 0;

			// First of all find if and where the ray hit's the polygon's plane
			if(plane.Raycast(ray, out distance))
			{
				Vector3 hitPoint = ray.GetPoint(distance);

				// Now find out if the point on the polygon plane is behind each polygon edge
				for (int i = 0; i < polygon.Vertices.Length; i++) 
				{
					Vector3 point1 = polygon.Vertices[i].Position;
					Vector3 point2 = polygon.Vertices[(i+1)%polygon.Vertices.Length].Position;

					Vector3 edge = point2 - point1; // Direction from a vertex to the next
					Vector3 polygonNormal = plane.normal;

					// Cross product of the edge with the polygon's normal gives the edge's normal
					Vector3 edgeNormal = Vector3.Cross(edge, polygonNormal);

					Vector3 edgeCenter = (point1+point2) * 0.5f;

					if(polygonSkin != 0)
					{
						edgeCenter += edgeNormal.normalized * polygonSkin;
					}

					Vector3 pointToEdgeCentroid = edgeCenter - hitPoint;

					// If the point is outside an edge this will return a negative value
					if(Vector3.Dot(edgeNormal, pointToEdgeCentroid) < 0)
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		// Calculates the new UV for the target position on the polygon based on the current UV set up
		// From: http://answers.unity3d.com/questions/383804/calculate-uv-coordinates-of-3d-point-on-plane-of-m.html
		public static Vector2 GetUVForPositionOld(Polygon polygon, Vector3 newPosition)
		{
			int vertexIndex1 = 0;
			int vertexIndex2 = 1;
			int vertexIndex3 = 2;

			// Update UVs
			Vector3 pos1 = polygon.Vertices[vertexIndex1].Position;
			Vector3 pos2 = polygon.Vertices[vertexIndex2].Position;
			Vector3 pos3 = Vector3.zero;

			for (int i = 2; i < polygon.Vertices.Length; i++) 
			{
				vertexIndex3 = i;

				pos3 = polygon.Vertices[vertexIndex3].Position;

				Plane tempPlane = new Plane(pos1,pos2,pos3);

				if(tempPlane.normal != Vector3.zero)
				{
					break;
				}
			}

			Plane plane = new Plane(pos1,pos2,pos3);
			Vector3 planePoint = MathHelper.ClosestPointOnPlane(newPosition, plane);

			Vector2 uv1 = polygon.Vertices[vertexIndex1].UV;
			Vector2 uv2 = polygon.Vertices[vertexIndex2].UV;
			Vector2 uv3 = polygon.Vertices[vertexIndex3].UV;

			// calculate vectors from point f to vertices p1, p2 and p3:
			Vector3 f1 = pos1-planePoint;
			Vector3 f2 = pos2-planePoint;
			Vector3 f3 = pos3-planePoint;

			// calculate the areas (parameters order is essential in this case):
			Vector3 va = Vector3.Cross(pos1-pos2, pos1-pos3); // main triangle cross product
			Vector3 va1 = Vector3.Cross(f2, f3); // p1's triangle cross product
			Vector3 va2 = Vector3.Cross(f3, f1); // p2's triangle cross product
			Vector3 va3 = Vector3.Cross(f1, f2); // p3's triangle cross product

			float a = va.magnitude; // main triangle area

			// calculate barycentric coordinates with sign:
			float a1 = va1.magnitude/a * Mathf.Sign(Vector3.Dot(va, va1));
			float a2 = va2.magnitude/a * Mathf.Sign(Vector3.Dot(va, va2));
			float a3 = va3.magnitude/a * Mathf.Sign(Vector3.Dot(va, va3));

			// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
			Vector2 uv = uv1 * a1 + uv2 * a2 + uv3 * a3;

			return uv;
		}

		public static Vector2 GetUVForPosition(Polygon polygon, Vector3 newPosition)
		{
			int vertexIndex1 = 0;
			int vertexIndex2 = 0;
			int vertexIndex3 = 0;

			// Account for overlapping vertices
			for (int i = vertexIndex1+1; i < polygon.Vertices.Length; i++) 
			{
				if(!polygon.Vertices[i].Position.EqualsWithEpsilon(polygon.Vertices[vertexIndex1].Position))
				{
					vertexIndex2 = i;
					break;
				}
			}

			for (int i = vertexIndex2+1; i < polygon.Vertices.Length; i++) 
			{
				if(!polygon.Vertices[i].Position.EqualsWithEpsilon(polygon.Vertices[vertexIndex2].Position))
				{
					vertexIndex3 = i;
					break;
				}
			}

			// Now account for the fact that the picked three vertices might be collinear
			Vector3 pos1 = polygon.Vertices[vertexIndex1].Position;
			Vector3 pos2 = polygon.Vertices[vertexIndex2].Position;
			Vector3 pos3 = polygon.Vertices[vertexIndex3].Position;

			Plane plane = new Plane(pos1,pos2,pos3);
			if(plane.normal == Vector3.zero)
			{
				for (int i = 2; i < polygon.Vertices.Length; i++) 
				{
					vertexIndex3 = i;

					pos3 = polygon.Vertices[vertexIndex3].Position;

					Plane tempPlane = new Plane(pos1,pos2,pos3);

					if(tempPlane.normal != Vector3.zero)
					{
						break;
					}
				}
				plane = new Plane(pos1,pos2,pos3);
			}

			// Should now have a good set of positions, so continue

			Vector3 planePoint = MathHelper.ClosestPointOnPlane(newPosition, plane);

			Vector2 uv1 = polygon.Vertices[vertexIndex1].UV;
			Vector2 uv2 = polygon.Vertices[vertexIndex2].UV;
			Vector2 uv3 = polygon.Vertices[vertexIndex3].UV;

			// calculate vectors from point f to vertices p1, p2 and p3:
			Vector3 f1 = pos1-planePoint;
			Vector3 f2 = pos2-planePoint;
			Vector3 f3 = pos3-planePoint;

			// calculate the areas (parameters order is essential in this case):
			Vector3 va = Vector3.Cross(pos1-pos2, pos1-pos3); // main triangle cross product
			Vector3 va1 = Vector3.Cross(f2, f3); // p1's triangle cross product
			Vector3 va2 = Vector3.Cross(f3, f1); // p2's triangle cross product
			Vector3 va3 = Vector3.Cross(f1, f2); // p3's triangle cross product

			float a = va.magnitude; // main triangle area

			// calculate barycentric coordinates with sign:
			float a1 = va1.magnitude/a * Mathf.Sign(Vector3.Dot(va, va1));
			float a2 = va2.magnitude/a * Mathf.Sign(Vector3.Dot(va, va2));
			float a3 = va3.magnitude/a * Mathf.Sign(Vector3.Dot(va, va3));

			// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
			Vector2 uv = uv1 * a1 + uv2 * a2 + uv3 * a3;

			return uv;
		}

		public static Vector2 GetUVForPosition(Vector3 pos1, Vector3 pos2, Vector3 pos3, 
			Vector2 uv1, Vector2 uv2, Vector2 uv3, 
			Vector3 newPosition)
		{
			Plane plane = new Plane(pos1,pos2,pos3);
			Vector3 planePoint = MathHelper.ClosestPointOnPlane(newPosition, plane);

			// calculate vectors from point f to vertices p1, p2 and p3:
			Vector3 f1 = pos1-planePoint;
			Vector3 f2 = pos2-planePoint;
			Vector3 f3 = pos3-planePoint;

			// calculate the areas (parameters order is essential in this case):
			Vector3 va = Vector3.Cross(pos1-pos2, pos1-pos3); // main triangle cross product
			Vector3 va1 = Vector3.Cross(f2, f3); // p1's triangle cross product
			Vector3 va2 = Vector3.Cross(f3, f1); // p2's triangle cross product
			Vector3 va3 = Vector3.Cross(f1, f2); // p3's triangle cross product

			float a = va.magnitude; // main triangle area

			// calculate barycentric coordinates with sign:
			float a1 = va1.magnitude/a * Mathf.Sign(Vector3.Dot(va, va1));
			float a2 = va2.magnitude/a * Mathf.Sign(Vector3.Dot(va, va2));
			float a3 = va3.magnitude/a * Mathf.Sign(Vector3.Dot(va, va3));

			// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
			Vector2 uv = uv1 * a1 + uv2 * a2 + uv3 * a3;

			return uv;
		}
	}
}
#endif