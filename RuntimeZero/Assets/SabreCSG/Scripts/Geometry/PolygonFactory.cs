#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sabresaurus.SabreCSG
{
	public static class PolygonFactory
	{
		public const float CONVEX_EPSILON = 0.001f;

	    public static Polygon[] GenerateCube()
	    {
	        Polygon[] polygons = new Polygon[6];

	        // Left
	        polygons[0] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(-1, -1, -1), new Vector3(-1, 0, 0), new Vector2(1,0)),
				new Vertex(new Vector3(-1, -1, 1), new Vector3(-1, 0, 0), new Vector2(0,0)),
				new Vertex(new Vector3(-1, 1, 1), new Vector3(-1, 0, 0), new Vector2(0,1)),
				new Vertex(new Vector3(-1, 1, -1), new Vector3(-1, 0, 0), new Vector2(1,1)),
			}, null, false, false);

	        // Front
	        polygons[1] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(1, -1, -1), new Vector3(1, 0, 0), new Vector2(0,0)),
				new Vertex(new Vector3(1, 1, -1), new Vector3(1, 0, 0), new Vector2(0,1)),
				new Vertex(new Vector3(1, 1, 1), new Vector3(1, 0, 0), new Vector2(1,1)),
				new Vertex(new Vector3(1, -1, 1), new Vector3(1, 0, 0), new Vector2(1,0)),
			}, null, false, false);

	        // Bottom
	        polygons[2] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(-1, -1, -1), new Vector3(0, -1, 0), new Vector2(1,0)),
				new Vertex(new Vector3(1, -1, -1), new Vector3(0, -1, 0), new Vector2(0,0)),
				new Vertex(new Vector3(1, -1, 1), new Vector3(0, -1, 0), new Vector2(0,1)),
				new Vertex(new Vector3(-1, -1, 1), new Vector3(0, -1, 0), new Vector2(1,1)),
			}, null, false, false);

	        // Top
	        polygons[3] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(-1, 1, -1), new Vector3(0, 1, 0), new Vector2(1,0)),
				new Vertex(new Vector3(-1, 1, 1), new Vector3(0, 1, 0), new Vector2(0,0)),
				new Vertex(new Vector3(1, 1, 1), new Vector3(0, 1, 0), new Vector2(0,1)),
				new Vertex(new Vector3(1, 1, -1), new Vector3(0, 1, 0), new Vector2(1,1)),
			}, null, false, false);

	        // Right
			polygons[4] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(-1, -1, -1), new Vector3(0, 0, -1), new Vector2(0,0)),
				new Vertex(new Vector3(-1, 1, -1), new Vector3(0, 0, -1), new Vector2(0,1)),
				new Vertex(new Vector3(1, 1, -1), new Vector3(0, 0, -1), new Vector2(1,1)),
				new Vertex(new Vector3(1, -1, -1), new Vector3(0, 0, -1), new Vector2(1,0)),
			}, null, false, false);

	        // Back
			polygons[5] = new Polygon(new Vertex[] {
				new Vertex(new Vector3(-1, -1, 1), new Vector3(0, 0, 1), new Vector2(1,0)),
				new Vertex(new Vector3(1, -1, 1), new Vector3(0, 0, 1), new Vector2(0,0)),
				new Vertex(new Vector3(1, 1, 1), new Vector3(0, 0, 1), new Vector2(0,1)),
				new Vertex(new Vector3(-1, 1, 1), new Vector3(0, 0, 1), new Vector2(1,1)),
			}, null, false, false);

	        return polygons;
	    }

		public static Polygon[] GenerateCylinder(int sideCount = 20)
		{
			Polygon[] polygons = new Polygon[sideCount * 3];

			float angleDelta = Mathf.PI * 2 / sideCount;

			for (int i = 0; i < sideCount; i++)
			{
				polygons[i] = new Polygon(new Vertex[] 
					{
						new Vertex(new Vector3(Mathf.Sin(i * angleDelta), -1, Mathf.Cos(i * angleDelta)), 
							new Vector3(Mathf.Sin(i * angleDelta), 0, Mathf.Cos(i * angleDelta)), 
							new Vector2(i * (1f/sideCount),0)),
						new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), -1, Mathf.Cos((i+1) * angleDelta)), 
							new Vector3(Mathf.Sin((i+1) * angleDelta), 0, Mathf.Cos((i+1) * angleDelta)), 
							new Vector2((i+1) * (1f/sideCount),0)),
						new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), 1, Mathf.Cos((i+1) * angleDelta)), 
							new Vector3(Mathf.Sin((i+1) * angleDelta), 0, Mathf.Cos((i+1) * angleDelta)), 
							new Vector2((i+1) * (1f/sideCount),1)),
						new Vertex(new Vector3(Mathf.Sin(i * angleDelta), 1, Mathf.Cos(i * angleDelta)), 
							new Vector3(Mathf.Sin(i * angleDelta), 0, Mathf.Cos(i * angleDelta)), 
							new Vector2(i * (1f/sideCount),1)),
					}, null, false, false);
			}

			Vertex capCenterVertex = new Vertex(new Vector3(0,1,0), Vector3.up, new Vector2(0,0));
			
			for (int i = 0; i < sideCount; i++)
			{
				Vertex vertex1 = new Vertex(new Vector3(Mathf.Sin(i * angleDelta), 1, Mathf.Cos(i * angleDelta)), Vector3.up, new Vector2(Mathf.Sin(i * angleDelta), Mathf.Cos(i * angleDelta)));
				Vertex vertex2 = new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), 1, Mathf.Cos((i+1) * angleDelta)), Vector3.up, new Vector2(Mathf.Sin((i+1) * angleDelta), Mathf.Cos((i+1) * angleDelta)));

				Vertex[] capVertices = new Vertex[] { vertex1, vertex2, capCenterVertex.DeepCopy() };
				polygons[sideCount + i] = new Polygon(capVertices, null, false, false);
			}

			capCenterVertex = new Vertex(new Vector3(0,-1,0), Vector3.down, new Vector2(0,0));

			for (int i = 0; i < sideCount; i++)
			{
				Vertex vertex1 = new Vertex(new Vector3(Mathf.Sin(i * -angleDelta), -1, Mathf.Cos(i * -angleDelta)), Vector3.down, new Vector2(Mathf.Sin(i * angleDelta), Mathf.Cos(i * angleDelta)));
				Vertex vertex2 = new Vertex(new Vector3(Mathf.Sin((i+1) * -angleDelta), -1, Mathf.Cos((i+1) * -angleDelta)), Vector3.down, new Vector2(Mathf.Sin((i+1) * angleDelta), Mathf.Cos((i+1) * angleDelta)));

				Vertex[] capVertices = new Vertex[] { vertex1, vertex2, capCenterVertex.DeepCopy() };
				polygons[sideCount * 2 + i] = new Polygon(capVertices, null, false, false);
			}
			
			return polygons;
		}

		public static Polygon[] GeneratePrism(int sideCount)
		{
			Polygon[] polygons = new Polygon[sideCount * 3];

			float angleDelta = Mathf.PI * 2 / sideCount;

			for (int i = 0; i < sideCount; i++)
			{
				Vector3 normal = new Vector3(Mathf.Sin((i+0.5f) * angleDelta), 0, Mathf.Cos((i+0.5f) * angleDelta));
				polygons[i] = new Polygon(new Vertex[] {

					new Vertex(new Vector3(Mathf.Sin(i * angleDelta), -1, Mathf.Cos(i * angleDelta)), 
						normal,
						new Vector2(0,0)),
					new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), -1, Mathf.Cos((i+1) * angleDelta)), 
						normal,
						new Vector2(1,0)),
					new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), 1, Mathf.Cos((i+1) * angleDelta)), 
						normal,
						new Vector2(1,1)),
					new Vertex(new Vector3(Mathf.Sin(i * angleDelta), 1, Mathf.Cos(i * angleDelta)), 
						normal,
						new Vector2(0,1)),
				}, null, false, false);
			}

			Vertex capCenterVertex = new Vertex(new Vector3(0,1,0), Vector3.up, new Vector2(0,0));

			for (int i = 0; i < sideCount; i++)
			{
				Vertex vertex1 = new Vertex(new Vector3(Mathf.Sin(i * angleDelta), 1, Mathf.Cos(i * angleDelta)), Vector3.up, new Vector2(Mathf.Sin(i * angleDelta), Mathf.Cos(i * angleDelta)));
				Vertex vertex2 = new Vertex(new Vector3(Mathf.Sin((i+1) * angleDelta), 1, Mathf.Cos((i+1) * angleDelta)), Vector3.up, new Vector2(Mathf.Sin((i+1) * angleDelta), Mathf.Cos((i+1) * angleDelta)));

				Vertex[] capVertices = new Vertex[] { vertex1, vertex2, capCenterVertex.DeepCopy() };
				polygons[sideCount + i] = new Polygon(capVertices, null, false, false);
			}

			capCenterVertex = new Vertex(new Vector3(0,-1,0), Vector3.down, new Vector2(0,0));

			for (int i = 0; i < sideCount; i++)
			{
				Vertex vertex1 = new Vertex(new Vector3(Mathf.Sin(i * -angleDelta), -1, Mathf.Cos(i * -angleDelta)), Vector3.down, new Vector2(Mathf.Sin(i * angleDelta), Mathf.Cos(i * angleDelta)));
				Vertex vertex2 = new Vertex(new Vector3(Mathf.Sin((i+1) * -angleDelta), -1, Mathf.Cos((i+1) * -angleDelta)), Vector3.down, new Vector2(Mathf.Sin((i+1) * angleDelta), Mathf.Cos((i+1) * angleDelta)));

				Vertex[] capVertices = new Vertex[] { vertex1, vertex2, capCenterVertex.DeepCopy() };
				polygons[sideCount * 2 + i] = new Polygon(capVertices, null, false, false);
			}

			return polygons;
		}

		public static Polygon[] GenerateSphere(int count = 6, int count2 = 12)
	    {
//			int count = 6; // lateral
//			int count2 = 12; // longitudinal

	        Polygon[] polygons = new Polygon[count * count2];

	        float angleDelta = 1f / count;
	        float longitudinalDelta = 1f / count2;

	        // TODO: Right now this uses quads for the top and bottom, should use tris to join up to polar points

	        for (int i = 0; i < count; i++)
	        {
	            for (int j = 0; j < count2; j++)
	            {
					Vertex[] vertices;

					if(i == count-1)
					{
						vertices = new Vertex[] {
							
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * i * angleDelta),
							                       Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * i * angleDelta),
							            Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2(i * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							                       Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							            Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2((i+1) * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * i * angleDelta),
							                       Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							                       ), 
							           new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * i * angleDelta),
							            Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							            ), 
							           new Vector2(i * (1f/count), j * (1f/count2))),
						};
					}
					else if(i > 0)
					{
	                	vertices = new Vertex[] {
						
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * i * angleDelta),
							                       Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * i * angleDelta),
							            Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2(i * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							                       Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							            Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2((i+1) * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							                       Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							                       ), 
							           new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							            Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							            ), 
							           new Vector2((i+1) * (1f/count), j * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * i * angleDelta),
							                       Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							                       ), 
							           new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * i * angleDelta),
							            Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							            ), 
							           new Vector2(i * (1f/count), j * (1f/count2))),
						};
					}
					else
					{
						vertices = new Vertex[] {
							
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * i * angleDelta),
							                       Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * i * angleDelta),
							            Mathf.Sin(Mathf.PI * i * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2(i * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							                       Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							                       ),
							           new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * (j+1) * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							            Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * (j+1) * longitudinalDelta)
							            ),
							           new Vector2((i+1) * (1f/count), (j+1) * (1f/count2))),
							new Vertex(new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							                       Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							                       Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							                       ), 
							           new Vector3(Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Cos(2 * Mathf.PI * j * longitudinalDelta),
							            Mathf.Cos(Mathf.PI * (i+1) * angleDelta),
							            Mathf.Sin(Mathf.PI * (i+1) * angleDelta) * Mathf.Sin(2 * Mathf.PI * j * longitudinalDelta)
							            ), 
							           new Vector2((i+1) * (1f/count), j * (1f/count2))),
						};
					}

	                for (int d = 0; d < vertices.Length; d++)
	                {
	                    vertices[d].UV = new Vector2(vertices[d].UV.y, 1 - vertices[d].UV.x);
	                }

					polygons[i + j * count] = new Polygon(vertices, null, false, false);
	            }
	        }

	        return polygons;
	    }

	    public static void Displace(ref Mesh mesh, float displacement)
	    {
	        Vector3[] vertices = mesh.vertices;
	        for (int i = 0; i < mesh.vertices.Length; i++)
	        {
	            vertices[i] += mesh.normals[i] * displacement;
	        }
	        mesh.vertices = vertices;
	    }

		public static void Invert(ref Mesh mesh)
		{
			Vector3[] normals = mesh.normals;
			for (int i = 0; i < mesh.normals.Length; i++)
			{
				normals[i] *= -1;
			}
			int[] triangles = mesh.triangles;

			for (int i = 0; i < triangles.Length; i+=3) 
			{
				int x1 = triangles[i+0];
				int x2 = triangles[i+1];
				int x3 = triangles[i+2];

				triangles[i+2] = x1;
				triangles[i+1] = x2;
				triangles[i+0] = x3;
			}

			mesh.triangles = triangles;
			mesh.normals = normals;
		}

		public static List<Polygon> GeneratePolygonsFromMesh(Mesh sourceMesh)
		{
			List<Polygon> generatedPolygons = new List<Polygon>();
			// Each sub mesh can have a different topology, i.e. triangles and quads
			for (int subMeshIndex = 0; subMeshIndex < sourceMesh.subMeshCount; subMeshIndex++) 
			{
				MeshTopology meshTopology = sourceMesh.GetTopology(subMeshIndex);
				// The vertex count per polygon that we need to walk through the indices at
				int stride = 1;
				if(meshTopology == MeshTopology.Quads)
				{
					stride = 4;
				}
				else if(meshTopology == MeshTopology.Triangles)
				{
					stride = 3;
				}
				else
				{
					Debug.LogError("Unhandled sub mesh topology " + meshTopology + ". Ignoring sub mesh.");
					continue;
				}

				// Grab this sub mesh's index buffer
				int[] indices = sourceMesh.GetIndices(subMeshIndex);

				// Walk through the polygons in the index buffer
				for (int j = 0; j < indices.Length/stride; j++) 
				{
					// Create a new vertex buffer for each polygon
					Vertex[] vertices = new Vertex[stride];

					// Pull out all the vertices for this source polygon
					for (int k = 0; k < stride; k++) 
					{
						int vertexIndex = indices[j*stride+k];
						
						vertices[k] = new Vertex(sourceMesh.vertices[vertexIndex], 
						                         sourceMesh.normals[vertexIndex], 
						                         sourceMesh.uv[vertexIndex]);
					}
					// Generate a new polygon using these vertices and add it to the output polygon list
					Polygon polygon = new Polygon(vertices, null, false, false);
					generatedPolygons.Add(polygon);
				}
			}
			// Finally return all the converted polygons
			return generatedPolygons;
		}
	    

	    public static void GenerateMeshFromPolygons(Polygon[] polygons, ref Mesh mesh, out List<int> polygonIndices)
	    {
			if(mesh == null)
			{
				mesh = new Mesh();
			}
			mesh.Clear();
//	        mesh = new Mesh();
	        List<Vector3> vertices = new List<Vector3>();
	        List<Vector3> normals = new List<Vector3>();
	        List<Vector2> uvs = new List<Vector2>();
	        List<Color> colors = new List<Color>();
	        List<int> triangles = new List<int>();

	        // Maps triangle index (input) to polygon index (output). i.e. int polyIndex = polygonIndices[triIndex];
	        polygonIndices = new List<int>();

	        // Set up an indexer that tracks unique vertices, so that we reuse vertex data appropiately
	        VertexList vertexList = new VertexList();

	        // Iterate through every polygon and triangulate
	        for (int i = 0; i < polygons.Length; i++)
	        {
	            Polygon polygon = polygons[i];
	            List<int> indices = new List<int>();

	            for (int j = 0; j < polygon.Vertices.Length; j++)
	            {
	                // Each vertex must know about its shared data for geometry tinting
	                //polygon.Vertices[j].Shared = polygon.SharedBrushData;
	                // If the vertex is already in the indexer, fetch the index otherwise add it and get the added index
					int index = vertexList.AddOrGet(polygon.Vertices[j]);
	                // Put each vertex index in an array for use in the triangle generation
	                indices.Add(index);
	            }

	            // Triangulate the n-sided polygon and allow vertex reuse by using indexed geometry
	            for (int j = 2; j < indices.Count; j++)
	            {
	                triangles.Add(indices[0]);
	                triangles.Add(indices[j - 1]);
	                triangles.Add(indices[j]);

	                // Map that this triangle is from the specified polygon (so we can map back from triangles to polygon)
	                polygonIndices.Add(i);
	            }
	        }

	        // Create the relevant buffers from the vertex array
	        for (int i = 0; i < vertexList.Vertices.Count; i++)
	        {
	            vertices.Add(vertexList.Vertices[i].Position);
	            normals.Add(vertexList.Vertices[i].Normal);
	            uvs.Add(vertexList.Vertices[i].UV);
	            //	                colors.Add(((SharedBrushData)indexer.Vertices[i].Shared).BrushTintColor);
	        }

	        // Set the mesh buffers
	        mesh.vertices = vertices.ToArray();
	        mesh.normals = normals.ToArray();
	        mesh.colors = colors.ToArray();
	        mesh.uv = uvs.ToArray();
	        mesh.triangles = triangles.ToArray();
	    }

		public static bool IsMeshConvex(Polygon[] polygons)
		{
			for (int n = 0; n < polygons.Length; n++) 
			{
				for (int k = 0; k < polygons[n].Vertices.Length; k++) 
				{
					// Test every vertex against every plane, if the vertex is front of the plane then the brush is concave
					for (int i = 0; i < polygons.Length; i++) 
					{
						Polygon polygon = polygons[i];
						for (int z = 2; z < polygon.Vertices.Length; z++) 
						{
							Plane polygonPlane = new Plane(polygon.Vertices[0].Position, 
							                               polygon.Vertices[z-1].Position, 
							                               polygon.Vertices[z].Position);


							float dot = Vector3.Dot(polygonPlane.normal, polygons[n].Vertices[k].Position) + polygonPlane.distance;
							
							if(dot > CONVEX_EPSILON)
							{
								return false;
							}
						}

						for (int z = 0; z < polygon.Vertices.Length; z++) 
						{
							Plane polygonPlane = new Plane(polygon.Vertices[z].Position, 
								polygon.Vertices[(z+1)%polygon.Vertices.Length].Position, 
								polygon.Vertices[(z+2)%polygon.Vertices.Length].Position);


							float dot = Vector3.Dot(polygonPlane.normal, polygons[n].Vertices[k].Position) + polygonPlane.distance;

							if(dot > CONVEX_EPSILON)
							{
								return false;
							}
						}
					}

				}
			}
			
			return true;
		}

		public static bool SplitPolygonsByPlane(List<Polygon> polygons, // Source polygons that will be split
		                                        Plane splitPlane, 
		                                        bool excludeNewPolygons, // Whether new polygons should be marked as excludeFromBuild
		                                        out List<Polygon> polygonsFront, 
		                                        out List<Polygon> polygonsBack)
		{
			polygonsFront = new List<Polygon>();
			polygonsBack = new List<Polygon>();

			// First of all make sure splitting actually needs to occur (we'll get bad issues if
			// we try splitting geometry when we don't need to)
			if(!PolygonsIntersectPlane(polygons, splitPlane))
			{
				return false;
			}

			Material newMaterial = polygons[0].Material;
			
			// These are the vertices that will be used in the new caps
			List<Vertex> newVertices = new List<Vertex>();
			
			for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++) 
			{
				Polygon.PolygonPlaneRelation planeRelation = Polygon.TestPolygonAgainstPlane(polygons[polygonIndex], splitPlane);
				
				// Polygon has been found to span both sides of the plane, attempt to split into two pieces
				if(planeRelation == Polygon.PolygonPlaneRelation.Spanning)
				{
					Polygon frontPolygon;
					Polygon backPolygon;
					Vertex newVertex1;
					Vertex newVertex2;
					
					// Attempt to split the polygon
					if(Polygon.SplitPolygon(polygons[polygonIndex], out frontPolygon, out backPolygon, out newVertex1, out newVertex2, splitPlane))
					{
						// If the split algorithm was successful (produced two valid polygons) then add each polygon to 
						// their respective points and track the intersection points
						polygonsFront.Add(frontPolygon);
						polygonsBack.Add(backPolygon);
						
						newVertices.Add(newVertex1);
						newVertices.Add(newVertex2);

						newMaterial = polygons[polygonIndex].Material;
					}
					else
					{
						// Two valid polygons weren't generated, so use the valid one
						if(frontPolygon != null)
						{
							planeRelation = Polygon.PolygonPlaneRelation.InFront;
						}
						else if(backPolygon != null)
						{
							planeRelation = Polygon.PolygonPlaneRelation.Behind;
						}
						else
						{
							Debug.LogError("Polygon splitting has resulted in two zero area polygons. This is unhandled.");
							//							Polygon.PolygonPlaneRelation secondplaneRelation = Polygon.TestPolygonAgainstPlane(polygons[polygonIndex], splitPlane);
						}
					}
				}
				
				// If the polygon is on one side of the plane or the other
				if(planeRelation != Polygon.PolygonPlaneRelation.Spanning)
				{
					// Make sure any points that are coplanar on non-straddling polygons are still used in polygon 
					// construction
					for (int vertexIndex = 0; vertexIndex < polygons[polygonIndex].Vertices.Length; vertexIndex++) 
					{
						if(Polygon.ComparePointToPlane(polygons[polygonIndex].Vertices[vertexIndex].Position, splitPlane) == Polygon.PointPlaneRelation.On)
						{
							newVertices.Add(polygons[polygonIndex].Vertices[vertexIndex]);
						}
					}
					
					if(planeRelation == Polygon.PolygonPlaneRelation.Behind)
					{
						polygonsBack.Add(polygons[polygonIndex]);
					}
					else 
					{
						polygonsFront.Add(polygons[polygonIndex]);
					}
				}
			}
			
			// If any splits occured or coplanar vertices are found. (For example if you're splitting a sphere at the
			// equator then no polygons will be split but there will be a bunch of coplanar vertices!)
			if(newVertices.Count > 0)
			{
				// HACK: This code is awful, because we end up with lots of duplicate vertices
				List<Vector3> positions = newVertices.Select(item => item.Position).ToList ();
				
				Polygon newPolygon = PolygonFactory.ConstructPolygon(positions, true);
				
				// Assuming it was possible to create a polygon
				if(newPolygon != null)
				{
					if(!MathHelper.PlaneEqualsLooser(newPolygon.Plane, splitPlane))
					{
						// Polygons are sometimes constructed facing the wrong way, possibly due to a winding order
						// mismatch. If the two normals are opposite, flip the new polygon
						if(Vector3.Dot(newPolygon.Plane.normal, splitPlane.normal) < -0.9f)
						{
							newPolygon.Flip();
						}
					}
					
					newPolygon.ExcludeFromFinal = excludeNewPolygons;
					newPolygon.Material = newMaterial;
					
					polygonsFront.Add(newPolygon);
					
					newPolygon = newPolygon.DeepCopy();
					newPolygon.Flip();
					
					newPolygon.ExcludeFromFinal = excludeNewPolygons;
					newPolygon.Material = newMaterial;
					
					
					if(newPolygon.Plane.normal == Vector3.zero)
					{
						Debug.LogError("Invalid Normal! Shouldn't be zero. This is unexpected since extraneous positions should have been removed!");
						//						Polygon fooNewPolygon = PolygonFactory.ConstructPolygon(positions, true);
					}
					
					polygonsBack.Add(newPolygon);
				}
				return true;
			}
			else
			{
				// It wasn't possible to create the polygon, for example the constructed polygon was too small
				// This could happen if you attempt to clip the tip off a long but thin brush, the plane-polyhedron test
				// would say they intersect but in reality the resulting polygon would be near zero area
				return false;
			}
		}
		
		/// <summary>
		/// Constructs a polygon from an unordered coplanar set of positions
		/// </summary>
		public static Polygon ConstructPolygon(List<Vector3> sourcePositions, bool removeExtraPositions)
		{
			List<Vector3> positions;
			
			if(removeExtraPositions)
			{
				Polygon.Vector3ComparerEpsilon equalityComparer = new Polygon.Vector3ComparerEpsilon();
				positions = sourcePositions.Distinct(equalityComparer).ToList();
			}
			else
			{
				positions = sourcePositions;
			}
			
			// If positions is smaller than 3 then we can't construct a polygon. This could happen if you try to cut the
			// tip off a very, very thin brush. While the plane and the brushes would intersect, the actual
			// cross-sectional area is near zero and too small to create a valid polygon. In this case simply return
			// null to indicate polygon creation was impossible
			if(positions.Count < 3)
			{
				return null;
			}
			
			// Find center point, so we can sort the positions around it
			Vector3 center = positions[0];
			
			for (int i = 1; i < positions.Count; i++)
			{
				center += positions[i];
			}
			
			center *= 1f / positions.Count;
			
			if(positions.Count < 3)
			{
				Debug.LogError("Position count is below 3, this is probably unhandled");
			}
			
			// Find the plane
			UnityEngine.Plane plane = new UnityEngine.Plane(positions[0], positions[1], positions[2]);
			
			
			
			// Rotation to go from the polygon's plane to XY plane (for sorting)
			Quaternion cancellingRotation = Quaternion.Inverse(Quaternion.LookRotation(plane.normal));
			
			// Rotate the center point onto the plane too
			Vector3 rotatedCenter = cancellingRotation * center;
			
			// Sort the positions, passing the rotation to put the positions on XY plane and the rotated center point
			IComparer<Vector3> comparer = new SortVectorsClockwise(cancellingRotation, rotatedCenter);
			positions.Sort(comparer);
			
			
			// Create the vertices from the positions
			Vertex[] newPolygonVertices = new Vertex[positions.Count];
			for (int i = 0; i < positions.Count; i++)
			{
				newPolygonVertices[i] = new Vertex(positions[i], -plane.normal, (cancellingRotation * positions[i]) * 0.5f);
			}
			Polygon newPolygon = new Polygon(newPolygonVertices, null, false, false);
			
			if(newPolygon.Plane.normal == Vector3.zero)
			{
				Debug.LogError("Zero normal found, this leads to invalid polyhedron-point tests");
				
				// hacky
				//				if(removeExtraPositions)
				//				{
				//					Polygon.Vector3ComparerEpsilon equalityComparer = new Polygon.Vector3ComparerEpsilon();
				//					List<Vector3> testFoo = newPolygonVertices.Select(item => item.Position).Distinct(equalityComparer).ToList();
				//				}
			}
			return newPolygon;
		}
		
		const float TEST_EPSILON = 0.003f;

		// This basically tests against a really thick plane to see if some of the points are on each side of the thick 
		// plane. This makes sure we only split if we definitely need to (protecting against issues 
		// related to splitting very small polygons breaking other code).
		private static bool PolygonsIntersectPlane (List<Polygon> polygons, Plane splitPlane)
		{
			int numberInFront = 0;
			int numberBehind = 0;
			
			float distanceInFront = 0f;
			float distanceBehind = 0f;
			
			for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++) 
			{
				for (int vertexIndex = 0; vertexIndex < polygons[polygonIndex].Vertices.Length; vertexIndex++) 
				{
					Vector3 point = polygons[polygonIndex].Vertices[vertexIndex].Position;
					
					float distance = splitPlane.GetDistanceToPoint(point);
					
					if (distance < -TEST_EPSILON)
					{
						numberInFront++;
						
						distanceInFront = Mathf.Min(distanceInFront, distance);
					}
					else if (distance > TEST_EPSILON)
					{
						numberBehind++;
						
						distanceBehind = Mathf.Max(distanceBehind, distance);
					}
				}
			}
			
			if(numberInFront > 0 && numberBehind > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		// Used to sort a collection of Vectors in a clockwise direction
		internal class SortVectorsClockwise : IComparer<Vector3>
		{
			Quaternion cancellingRotation; // Used to transform the positions from an arbitrary plane to the XY plane
			Vector3 rotatedCenter; // Transformed center point, used as the center point to find the angles around
			
			public SortVectorsClockwise(Quaternion cancellingRotation, Vector3 rotatedCenter)
			{
				this.cancellingRotation = cancellingRotation;
				this.rotatedCenter = rotatedCenter;
			}
			
			public int Compare(Vector3 position1, Vector3 position2)
			{
				// Rotate the positions and subtract the center, so they become vectors from the center point on the plane
				Vector3 vector1 = (cancellingRotation * position1) - rotatedCenter;
				Vector3 vector2 = (cancellingRotation * position2) - rotatedCenter;
				
				// Find the angle of each vector on the plane
				float angle1 = Mathf.Atan2(vector1.x, vector1.y);
				float angle2 = Mathf.Atan2(vector2.x, vector2.y);
				
				// Compare the angles
				return angle1.CompareTo(angle2);
			}
		}
	}
}
#endif