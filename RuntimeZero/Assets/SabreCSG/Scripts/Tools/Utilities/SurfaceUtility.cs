#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sabresaurus.SabreCSG
{
	// Two vectors that are used to map UV directions to 3D space
	public class UVOrientation
	{
		public Vector3 NorthVector;
		public Vector3 EastVector;

		public float NorthScale = 0.5f;
		public float EastScale = 0.5f;
	}

	public static class SurfaceUtility
	{
		public static bool EdgeMatches(Edge edge1, Edge edge2)
		{
			// First of all determine if the two lines are collinear

			Vector3 direction1 = edge1.Vertex2.Position - edge1.Vertex1.Position;
			Vector3 direction2 = edge2.Vertex2.Position - edge2.Vertex1.Position;

			Vector3 direction1Normalized = direction1.normalized;
			Vector3 direction2Normalized = direction2.normalized;

			float dot = Vector3.Dot(direction1Normalized, direction2Normalized);

			// Are the lines parallel?
			if(dot > 0.999f || dot < -0.999f)
			{
				// The lines are parallel, next calculate perpendicular distance between them

				// Calculate a normal vector perpendicular to the line
				Vector3 normal;

				float upDot = Vector3.Dot(direction1Normalized, Vector3.up);

				if(Mathf.Abs(upDot) > 0.9f)
				{
					normal = Vector3.Cross(Vector3.forward, direction1Normalized).normalized;
				}
				else
				{
					normal = Vector3.Cross(Vector3.up, direction1Normalized).normalized;
				}

				// Calculate the tangent vector
				Vector3 tangent = Vector3.Cross(normal, direction1);

				// Take the offset from a point on each line
				Vector3 offset = edge2.Vertex2.Position - edge1.Vertex1.Position;

				// Find the perpendicular distance between the lines along both normal and tangent directions
				float normalDistance = Vector3.Dot(normal, offset);
				float tangentDistance = Vector3.Dot(tangent, offset);

				// If the perpendicular distance is very small
				if(Mathf.Abs(normalDistance) < 0.0001f
					&& Mathf.Abs(tangentDistance) < 0.0001f)
				{
					// Lines are colinear
					// Check if either segment contains one of the points from the other segment

					float signedDistance = 0;

					Plane edge1Plane1 = new Plane(direction1Normalized, edge1.Vertex2.Position);

					signedDistance = edge1Plane1.GetDistanceToPoint(edge2.Vertex1.Position);
					if(signedDistance >= 0)
					{
						return true;
					}

					signedDistance = edge1Plane1.GetDistanceToPoint(edge2.Vertex2.Position);
					if(signedDistance >= 0)
					{
						return true;
					}

					Plane edge1Plane2 = new Plane(-direction1Normalized, edge1.Vertex1.Position);

					signedDistance = edge1Plane2.GetDistanceToPoint(edge2.Vertex1.Position);
					if(signedDistance <= 0)
					{
						return true;
					}

					signedDistance = edge1Plane2.GetDistanceToPoint(edge2.Vertex2.Position);
					if(signedDistance <= 0)
					{
						return true;
					}

					Plane edge2Plane1 = new Plane(direction2Normalized, edge2.Vertex2.Position);

					signedDistance = edge2Plane1.GetDistanceToPoint(edge1.Vertex1.Position);
					if(signedDistance <= 0)
					{
						return true;
					}

					signedDistance = edge2Plane1.GetDistanceToPoint(edge1.Vertex2.Position);
					if(signedDistance <= 0)
					{
						return true;
					}

					Plane edge2Plane2 = new Plane(-direction2Normalized, edge2.Vertex1.Position);

					signedDistance = edge2Plane2.GetDistanceToPoint(edge1.Vertex1.Position);
					if(signedDistance >= 0)
					{
						return true;
					}

					signedDistance = edge2Plane2.GetDistanceToPoint(edge1.Vertex2.Position);
					if(signedDistance >= 0)
					{
						return true;
					}

					return false;
				}
				else
				{
					// Lines are not colinear, there is a perpendicular gap between them
					return false;
				}
			}
			else
			{
				// Lines are not parallel
				return false;
			}
		}



//		public static bool EdgeMatches(Edge edge1, Edge edge2)
//		{			
//			if(EdgeContainsEdge(edge1, edge2))
//			{
//				return true;
//			}
//			else if(EdgeContainsEdge(edge2, edge1))
//			{
//				return true;
//			}
//			else
//			{
//				return false;
//			}
//		}
//
//		public static bool EdgeContainsEdge(Edge edge1, Edge edge2)
//		{
//			Vector3 direction1 = edge1.Vertex2.Position - edge1.Vertex1.Position;
//			Vector3 direction2 = edge2.Vertex2.Position - edge2.Vertex1.Position;
//
//			float dot = Vector3.Dot(direction1, direction2);
//			if(dot > 0.999f || dot < -0.999f)
//			{
//				float squareLength2 = direction2.sqrMagnitude;
//
//				Vector3 offset = edge2.Vertex1.Position - edge1.Vertex1.Position;
//
//				float f = Vector3.Dot(direction2, offset);
//
//
//
//				// Find the offset perpendicular to the lines
//				Vector3 normalizedDirection2 = direction2.normalized;
//				Vector3 toSubtract = normalizedDirection2 * Vector3.Dot(normalizedDirection2, offset);
//
//				Vector3 perpendicularOffset = offset - toSubtract;
//
//				if(perpendicularOffset.sqrMagnitude > 0)
//				{
//					return false;
//				}
//
//				float t = f / squareLength2;
//
//				if(t >= 0 && t <= 1)
//				{
//					return true;
//				}
//
//				offset = edge2.Vertex1.Position - edge1.Vertex2.Position;
//
//				f = Vector3.Dot(direction2, offset);
//
//				t = f / squareLength2;
//
//				if(t >= 0 && t <= 1)
//				{
//					return true;
//				}
//
//				// Neither point from edge 1 is within edge 2
//				return false;
//			}
//			else
//			{
//				// Lines are not parallel
//				return false;
//			}
//
//		}

		public static bool FindSharedEdge(Polygon polygon1, Polygon polygon2, out Edge matchedEdge1, out Edge matchedEdge2)
		{
			for (int i = 0; i < polygon1.Vertices.Length; i++) 
			{
				Edge edge1 = new Edge(polygon1.Vertices[i], polygon1.Vertices[(i+1) % polygon1.Vertices.Length]);

				for (int j = 0; j < polygon2.Vertices.Length; j++) 
				{
					Edge edge2 = new Edge(polygon2.Vertices[j], polygon2.Vertices[(j+1) % polygon2.Vertices.Length]);

					if(EdgeMatches(edge1, edge2))
					{
						matchedEdge1 = edge1;
//						matchedEdge2 = edge2;
						matchedEdge2 = new Edge(edge2.Vertex2, edge2.Vertex1);

						return true;
					}
						
//					if (edge1.Vertex1.Position.EqualsWithEpsilon(edge2.Vertex1.Position) 
//						&& edge1.Vertex2.Position.EqualsWithEpsilon(edge2.Vertex2.Position))
//					{
//						matchedEdge1 = edge1;
//						matchedEdge2 = edge2;
//						return true;
//					} // Check if the edge is the other way around
//					else if (edge1.Vertex1.Position.EqualsWithEpsilon(edge2.Vertex2.Position) 
//						&& edge1.Vertex2.Position.EqualsWithEpsilon(edge2.Vertex1.Position))
//					{
//						matchedEdge1 = edge1;
//						matchedEdge2 = new Edge(edge2.Vertex2, edge2.Vertex1);
//						return true;
//					}
				}
			}

			// None found
			matchedEdge1 = null;
			matchedEdge2 = null;
			return false;
		}

		public static void FacetPolygon(Polygon polygon, Polygon[] allPolygons)
		{
			for (int i = 0; i < polygon.Vertices.Length; i++) 
			{
				Vertex vertex = polygon.Vertices[i];
				vertex.Normal = polygon.Plane.normal;
			}
		}

		public static void SmoothPolygon(Polygon polygon, Polygon[] allPolygons)
		{
			float smoothingAngle = 60;

			for (int i = 0; i < polygon.Vertices.Length; i++) 
			{
				Vertex vertex = polygon.Vertices[i];

				Vector3 sourceNormal = polygon.Plane.normal;

				Vector3 newNormal = sourceNormal;
				int totalNormalCount = 1;

				for (int j = 0; j < allPolygons.Length; j++) 
				{
					Polygon otherPolygon = allPolygons[j];
					// Ignore the same polygon
					if(otherPolygon != polygon)
					{
						for (int k = 0; k < otherPolygon.Vertices.Length; k++) 
						{
							Vertex otherVertex = otherPolygon.Vertices[k];
							if(otherVertex.Position == vertex.Position)
							{
								if(Vector3.Angle(sourceNormal, otherPolygon.Plane.normal) <= smoothingAngle)
								{
									newNormal += otherPolygon.Plane.normal;
									totalNormalCount++;
								}
							}
						}
					}
				}

				vertex.Normal = newNormal * (1f / totalNormalCount);
			} 
		}

		public static void ExtrudePolygonOld(Polygon sourcePolygon, out Polygon[] outputPolygons, out Quaternion rotation)
		{
			float extrusionDistance = 1;

			Polygon newPolygon = sourcePolygon.DeepCopy();
			newPolygon.UniqueIndex = -1;
			newPolygon.Flip();

			Vector3 normal = sourcePolygon.Plane.normal;

			Polygon oppositePolygon = sourcePolygon.DeepCopy();
			oppositePolygon.UniqueIndex = -1;

			Vertex[] vertices = oppositePolygon.Vertices;
			for (int i = 0; i < vertices.Length; i++) 
			{
				vertices[i].Position += normal;
			}
			oppositePolygon.SetVertices(vertices);

			Polygon[] brushSides = new Polygon[sourcePolygon.Vertices.Length];

			for (int i = 0; i < newPolygon.Vertices.Length; i++) 
			{
				Vertex vertex1 = newPolygon.Vertices[i].DeepCopy();
				Vertex vertex2 = newPolygon.Vertices[(i+1)%newPolygon.Vertices.Length].DeepCopy();

				Vector2 uvDelta = vertex2.UV - vertex1.UV;

				float sourceDistance = Vector3.Distance(vertex1.Position, vertex2.Position);

				Vector2 rotatedUVDelta = uvDelta.Rotate(90) * (extrusionDistance / sourceDistance);

				Vertex vertex3 = vertex1.DeepCopy();
				vertex3.Position += normal * extrusionDistance;
				vertex3.UV += rotatedUVDelta;

				Vertex vertex4 = vertex2.DeepCopy();
				vertex4.Position += normal * extrusionDistance;
				vertex4.UV += rotatedUVDelta;

				Vertex[] newVertices = new Vertex[] { vertex1, vertex2, vertex4, vertex3 };

				brushSides[i] = new Polygon(newVertices, sourcePolygon.Material, false, false);
				brushSides[i].Flip();
				brushSides[i].ResetVertexNormals();
			}

			List<Polygon> polygons = new List<Polygon>();
			polygons.Add(newPolygon);
			polygons.Add(oppositePolygon);
			polygons.AddRange(brushSides);

			outputPolygons = polygons.ToArray();
			rotation = Quaternion.identity;
		}

		public static void ExtrudePolygon(Polygon sourcePolygon, out Polygon[] outputPolygons, out Quaternion rotation)
		{
			float extrusionDistance = 1;

			Polygon basePolygon = sourcePolygon.DeepCopy();
			basePolygon.UniqueIndex = -1;

			rotation = Quaternion.LookRotation(basePolygon.Plane.normal);
			Quaternion cancellingRotation = Quaternion.Inverse(rotation);

			Vertex[] vertices = basePolygon.Vertices;
//			Vector3 offsetPosition = vertices[0].Position;

			for (int i = 0; i < vertices.Length; i++) 
			{
//				vertices[i].Position -= offsetPosition;
				vertices[i].Position = cancellingRotation * vertices[i].Position;

				vertices[i].Normal = cancellingRotation * vertices[i].Normal;
			}

//			Vector3 newOffsetPosition = vertices[0].Position;
//			Vector3 delta = newOffsetPosition - offsetPosition;
//			for (int i = 0; i < vertices.Length; i++) 
//			{
//				vertices[i].Position += delta;
//			}

			basePolygon.SetVertices(vertices);

			Vector3 normal = basePolygon.Plane.normal;
			Polygon oppositePolygon = basePolygon.DeepCopy();
			oppositePolygon.UniqueIndex = -1;

			basePolygon.Flip();

			vertices = oppositePolygon.Vertices;
			for (int i = 0; i < vertices.Length; i++) 
			{
				vertices[i].Position += normal;
			}
			oppositePolygon.SetVertices(vertices);

			Polygon[] brushSides = new Polygon[sourcePolygon.Vertices.Length];

			for (int i = 0; i < basePolygon.Vertices.Length; i++) 
			{
				Vertex vertex1 = basePolygon.Vertices[i].DeepCopy();
				Vertex vertex2 = basePolygon.Vertices[(i+1)%basePolygon.Vertices.Length].DeepCopy();

				Vector2 uvDelta = vertex2.UV - vertex1.UV;

				float sourceDistance = Vector3.Distance(vertex1.Position, vertex2.Position);

				Vector2 rotatedUVDelta = uvDelta.Rotate(90) * (extrusionDistance / sourceDistance);

				Vertex vertex3 = vertex1.DeepCopy();
				vertex3.Position += normal * extrusionDistance;
				vertex3.UV += rotatedUVDelta;

				Vertex vertex4 = vertex2.DeepCopy();
				vertex4.Position += normal * extrusionDistance;
				vertex4.UV += rotatedUVDelta;

				Vertex[] newVertices = new Vertex[] { vertex1, vertex2, vertex4, vertex3 };

				brushSides[i] = new Polygon(newVertices, sourcePolygon.Material, false, false);
				brushSides[i].Flip();
				brushSides[i].ResetVertexNormals();
			}

			List<Polygon> polygons = new List<Polygon>();
			polygons.Add(basePolygon);
			polygons.Add(oppositePolygon);
			polygons.AddRange(brushSides);

			outputPolygons = polygons.ToArray();
		}

		public static void GetPrimaryPolygonDescribers(Polygon polygon, out Vertex vertex1, out Vertex vertex2, out Vertex vertex3)
		{
			int vertexIndex1 = 0;
			int vertexIndex2 = 1;
			int vertexIndex3 = 2;

			// Update UVs
			Vector3 pos1 = polygon.Vertices[vertexIndex1].Position;
			Vector3 pos2 = polygon.Vertices[vertexIndex2].Position;
			Vector3 pos3 = polygon.Vertices[vertexIndex3].Position;

			Plane testPlane = new Plane(pos1, pos2, pos3);

			if(testPlane.normal == Vector3.zero && polygon.Vertices.Length > 3)
			{
				for (vertexIndex3 = 3; vertexIndex3 < polygon.Vertices.Length; vertexIndex3++) 
				{
					pos3 = polygon.Vertices[vertexIndex3].Position;

					testPlane = new Plane(pos1, pos2, pos3);

					if(testPlane.normal != Vector3.zero)
					{
						break;
					}
				}
			}

			vertex1 = polygon.Vertices[vertexIndex1];
			vertex2 = polygon.Vertices[vertexIndex2];
			vertex3 = polygon.Vertices[vertexIndex3];
		}

		public static UVOrientation GetNorthEastVectors(Polygon polygon, Transform brushTransform)
		{
			Vertex vertex1;
			Vertex vertex2;
			Vertex vertex3;
			// Get three vertices which will reliably give us good UV information (i.e. not collinear)
			GetPrimaryPolygonDescribers(polygon, out vertex1, out vertex2, out vertex3);

			// Take 3 positions and their corresponding UVs
			Vector3 pos1 = brushTransform.TransformPoint(vertex1.Position);
			Vector3 pos2 = brushTransform.TransformPoint(vertex2.Position);
			Vector3 pos3 = brushTransform.TransformPoint(vertex3.Position);

			Vector2 uv1 = vertex1.UV;
			Vector2 uv2 = vertex2.UV;
			Vector2 uv3 = vertex3.UV;

			// Construct a matrix to map to the triangle's UV space
			Matrix2x2 uvMatrix = new Matrix2x2()
			{
				m00 = uv2.x - uv1.x, m10 = uv3.x - uv1.x,
				m01 = uv2.y - uv1.y, m11 = uv3.y - uv1.y,
			};

			// Invert the matrix to map from UV space
			Matrix2x2 uvMatrixInverted = uvMatrix.Inverse;

			// Construct a matrix to map to the triangle's world space
			Matrix3x2 positionMatrix = new Matrix3x2()
			{
				m00 = pos2.x - pos1.x, m10 = pos3.x - pos1.x,
				m01 = pos2.y - pos1.y, m11 = pos3.y - pos1.y,
				m02 = pos2.z - pos1.z, m12 = pos3.z - pos1.z,
			};

			// Multiply the inverted UVs by the positional matrix to get the UV vectors in world space
			Matrix3x2 multipliedMatrix = positionMatrix.Multiply(uvMatrixInverted);

			// Extract the world vectors that correspond to UV north (0,1) and UV east (1,0). Note that these aren't
			// normalized and their magnitude is the reciprocal of tiling
			Vector3 eastVectorScaled = new Vector3(multipliedMatrix.m00, multipliedMatrix.m01, multipliedMatrix.m02);
			Vector3 northVectorScaled = new Vector3(multipliedMatrix.m10, multipliedMatrix.m11, multipliedMatrix.m12);

			return new UVOrientation()
			{
				NorthVector = northVectorScaled.normalized,
				EastVector = eastVectorScaled.normalized,
				NorthScale = northVectorScaled.magnitude,
				EastScale = eastVectorScaled.magnitude,
			};
		}

		public static Vector2 GetUVOffset(Polygon polygon)
		{
			Vertex vertex1;
			Vertex vertex2;
			Vertex vertex3;
			// Get three vertices which will reliably give us good UV information (i.e. not collinear)
			SurfaceUtility.GetPrimaryPolygonDescribers(polygon, out vertex1, out vertex2, out vertex3);

			Vector2 newUV = GeometryHelper.GetUVForPosition(vertex1.Position,
				vertex2.Position,
				vertex3.Position,
				vertex1.UV,
				vertex2.UV,
				vertex3.UV,
				polygon.GetCenterPoint());

			return newUV;
		}

		public static Pair<float?, float?> GetUVOffset(List<Polygon> polygons)
		{
			float? northOffset = 0;
			float? eastOffset = 0;

			for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++) 
			{
				Polygon polygon = polygons[polygonIndex];

				Vector2 uvOffset = SurfaceUtility.GetUVOffset(polygon);

				if(polygonIndex == 0)
				{
					northOffset = uvOffset.y;
					eastOffset = uvOffset.x;
				}
				else
				{
					if(!northOffset.HasValue || !northOffset.Value.EqualsWithEpsilon(uvOffset.y))
					{
						northOffset = null;
					}

					if(!eastOffset.HasValue || !eastOffset.Value.EqualsWithEpsilon(uvOffset.x))
					{
						eastOffset = null;
					}
				}
			}

			return new Pair<float?, float?> (eastOffset, northOffset);
		}
	}
}
#endif