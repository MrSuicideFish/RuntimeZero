#if UNITY_EDITOR || RUNTIME_CSG
using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

namespace Sabresaurus.SabreCSG
{
    public enum PrimitiveBrushType { Cube, Sphere, Cylinder, Prism, Custom };

    [ExecuteInEditMode]
    public class PrimitiveBrush : Brush
    {
        //		[SerializeField]
        //	    protected PrimitiveBrushType primitiveBrushType = PrimitiveBrushType.Cube;

        [SerializeField]
		Polygon[] polygons;

        // Maps triangle index (input) to polygon index (output). i.e. int polyIndex = polygonIndices[triIndex];
//        List<int> polygonIndices;

		[SerializeField,HideInInspector]
		int prismSideCount = 6;

		[SerializeField,HideInInspector]
		PrimitiveBrushType brushType = PrimitiveBrushType.Cube;

		[SerializeField,HideInInspector]
		bool tracked = false;

		int cachedInstanceID = 0;

		private CSGModelBase parentCsgModel;

		[SerializeField]
		TransformData cachedTransform;

		[SerializeField]
		int objectVersionSerialized;

		int objectVersionUnserialized;

		public PrimitiveBrushType BrushType {
			get {
				return brushType;
			}
			set {
				brushType = value;
			}
		}

		public void SetPolygons(Polygon[] polygons, bool breakTypeRelation = true)
        {
            this.polygons = polygons;

            Invalidate(true);

			if(breakTypeRelation)
			{
				BreakTypeRelation();
			}
        }

		public void BreakTypeRelation()
		{
			// Brushes retain knowledge of what they were made from, so it's easy to adjust the side count on a prism
			// for example, while retaining some of its transform information. If you start cutting away at a prism
			// using the clip tool for instance, it should stop tracking it as following the initial form
			brushType = PrimitiveBrushType.Custom;
		}

#if UNITY_EDITOR
		[UnityEditor.Callbacks.DidReloadScripts]
		static void OnReloadedScripts()
		{
			PrimitiveBrush[] brushes = FindObjectsOfType<PrimitiveBrush>();

			for (int i = 0; i < brushes.Length; i++) 
			{
				brushes[i].UpdateVisibility();
			}
		}
#endif

        void Start()
        {
			cachedTransform = new TransformData(transform);
			EnsureWellFormed();

			Invalidate(false);

			if(brushCache == null || brushCache.Polygons == null || brushCache.Polygons.Length == 0)
			{
				RecachePolygons(true);
			}

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetSelectedWireframeHidden(GetComponent<Renderer>(), true);
#endif

			objectVersionUnserialized = objectVersionSerialized;
        }

		public void ResetPolygons()
		{
			if (brushType == PrimitiveBrushType.Cube)
			{
				polygons = PolygonFactory.GenerateCube();
			}
			else if (brushType == PrimitiveBrushType.Cylinder)
			{
				polygons = PolygonFactory.GenerateCylinder();
			}
			else if (brushType == PrimitiveBrushType.Sphere)
			{
				polygons = PolygonFactory.GenerateSphere();
			}
			else if (brushType == PrimitiveBrushType.Prism)
			{
				if(prismSideCount < 3)
				{
					prismSideCount = 3;
				}
				polygons = PolygonFactory.GeneratePrism(prismSideCount);
			}
			else if(brushType == Sabresaurus.SabreCSG.PrimitiveBrushType.Custom)
			{
				// Do nothing
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		void DrawPolygons(Color color, params Polygon[] polygons)
		{
			GL.Begin(GL.TRIANGLES);
			color.a = 0.7f;
			GL.Color(color);
			
			for (int j = 0; j < polygons.Length; j++) 
			{
				Polygon polygon = polygons[j];
				Vector3 position1 = polygon.Vertices[0].Position;
				
				for (int i = 1; i < polygon.Vertices.Length - 1; i++)
				{
					GL.Vertex(transform.TransformPoint(position1));
					GL.Vertex(transform.TransformPoint(polygon.Vertices[i].Position));
					GL.Vertex(transform.TransformPoint(polygon.Vertices[i + 1].Position));
				}
			}
			GL.End();
		}

#if UNITY_EDITOR
        public void OnRepaint(UnityEditor.SceneView sceneView, Event e)
        {
            // Selected brush green outline
			if(!isBrushConvex)
			{
				SabreGraphics.GetSelectedBrushMaterial().SetPass(0);
				DrawPolygons(Color.red, polygons);
			}
        }
#endif

		public override Polygon[] GenerateTransformedPolygons()
		{
			Polygon[] polygonsCopy = polygons.DeepCopy<Polygon>();

			Vector3 center = transform.position;
			Quaternion rotation = transform.rotation;
			Vector3 scale = transform.localScale;

			for (int i = 0; i < polygons.Length; i++)
			{
				for (int j = 0; j < polygons[i].Vertices.Length; j++)
				{
					polygonsCopy[i].Vertices[j].Position = rotation * polygonsCopy[i].Vertices[j].Position.Multiply(scale) + center;
					polygonsCopy[i].Vertices[j].Normal = rotation * polygonsCopy[i].Vertices[j].Normal;
				}

				// Just updated a load of vertex positions, so make sure the cached plane is updated
				polygonsCopy[i].CalculatePlane();
			}

			return polygonsCopy;
		}

		public override void RecalculateBrushCache ()
		{
			RecachePolygons(true);

			RecalculateIntersections();
		}

		public override void RecachePolygons(bool markUnbuilt)
		{
			if(brushCache == null)
			{
				brushCache = new BrushCache();
			}
			Polygon[] cachedTransformedPolygons = GenerateTransformedPolygons();
			Bounds cachedTransformedBounds = GetBoundsTransformed();
			brushCache.Set(mode, cachedTransformedPolygons, cachedTransformedBounds, markUnbuilt);
		}

		public override void RecalculateIntersections()
		{
			List<Brush> brushes = GetCSGModel().GetBrushes();

			// Tracked brushes at edit time can be added in any order, so sort them
			IComparer<Brush> comparer = new BrushOrderComparer();
			for (int i = 0; i < brushes.Count; i++) 
			{
				if(brushes[i] == null)
				{
					brushes.RemoveAt(i);
					i--;
				}
			}
			brushes.Sort(comparer);

			RecalculateIntersections(brushes, true);	
		}

		public override void RecalculateIntersections(List<Brush> brushes, bool isRootChange)
		{
			List<Brush> previousVisualIntersections = brushCache.IntersectingVisualBrushes;
			List<Brush> previousCollisionIntersections = brushCache.IntersectingCollisionBrushes;

			List<Brush> intersectingVisualBrushes = CalculateIntersectingBrushes(this, brushes, false);
			List<Brush> intersectingCollisionBrushes = CalculateIntersectingBrushes(this, brushes, true);

			brushCache.SetIntersection(intersectingVisualBrushes, intersectingCollisionBrushes);

			if(isRootChange)
			{
				// Brushes that are either newly intersecting or no longer intersecting, they need to recalculate their
				// intersections, but also rebuild
				List<Brush> brushesToRecalcAndRebuild = new List<Brush>();

				// Brushes that are still intersecting, these should recalculate their intersections any way in case 
				// sibling order has changed to make sure their intersection order is still correct
				List<Brush> brushesToRecalculateOnly = new List<Brush>();

				// Brushes that are either new or existing intersections
				for (int i = 0; i < intersectingVisualBrushes.Count; i++) 
				{
					if(intersectingVisualBrushes[i] != null)
					{
						if(!previousVisualIntersections.Contains(intersectingVisualBrushes[i]))
						{
							// It's a newly intersecting brush
							if(!brushesToRecalcAndRebuild.Contains(intersectingVisualBrushes[i]))
							{
								brushesToRecalcAndRebuild.Add(intersectingVisualBrushes[i]);
							}
						}
						else
						{
							// Intersection was already present
							if(!brushesToRecalculateOnly.Contains(intersectingVisualBrushes[i]))
							{
								brushesToRecalculateOnly.Add(intersectingVisualBrushes[i]);
							}
						}
					}
				}

				// Find any brushes that no longer intersect
				for (int i = 0; i < previousVisualIntersections.Count; i++) 
				{
					if(previousVisualIntersections[i] != null && !intersectingVisualBrushes.Contains(previousVisualIntersections[i]))
					{
						if(!brushesToRecalcAndRebuild.Contains(previousVisualIntersections[i]))
						{
							brushesToRecalcAndRebuild.Add(previousVisualIntersections[i]);
						}
					}
				}

				// Collision Pass

				// Brushes that are either new or existing intersections
				for (int i = 0; i < intersectingCollisionBrushes.Count; i++) 
				{
					if(intersectingCollisionBrushes[i] != null)
					{
						if(!previousCollisionIntersections.Contains(intersectingCollisionBrushes[i]))
						{
							// It's a newly intersecting brush
							if(!brushesToRecalcAndRebuild.Contains(intersectingCollisionBrushes[i]))
							{
								brushesToRecalcAndRebuild.Add(intersectingCollisionBrushes[i]);
							}
						}
						else
						{
							// Intersection was already present
							if(!brushesToRecalculateOnly.Contains(intersectingCollisionBrushes[i]))
							{
								brushesToRecalculateOnly.Add(intersectingCollisionBrushes[i]);
							}
						}
					}
				}

				// Find any brushes that no longer intersect
				for (int i = 0; i < previousCollisionIntersections.Count; i++) 
				{
					if(previousCollisionIntersections[i] != null && !intersectingCollisionBrushes.Contains(previousCollisionIntersections[i]))
					{
						if(!brushesToRecalcAndRebuild.Contains(previousCollisionIntersections[i]))
						{
							brushesToRecalcAndRebuild.Add(previousCollisionIntersections[i]);
						}
					}
				}

				// Notify brushes that are either newly intersecting or no longer intersecting that they need to recalculate and rebuild
				for (int i = 0; i < brushesToRecalcAndRebuild.Count; i++) 
				{
					// Brush intersection has changed
					brushesToRecalcAndRebuild[i].RecalculateIntersections(brushes, false);
					// Brush needs to be built
					brushesToRecalcAndRebuild[i].BrushCache.SetUnbuilt();
				}

				// Brushes that remain intersecting should recalc their intersection lists just in case sibling order has changed
				for (int i = 0; i < brushesToRecalculateOnly.Count; i++) 
				{
					// Brush intersection has changed
					brushesToRecalculateOnly[i].RecalculateIntersections(brushes, false);
				}
			}
		}


		// Fired by the CSG Model
        public override void OnUndoRedoPerformed()
        {			
			if(objectVersionSerialized != objectVersionUnserialized)
			{
	            Invalidate(true);
			}
        }

        void EnsureWellFormed()
        {
            if (polygons == null || polygons.Length == 0)
            {
				// Reset custom brushes back to a cube
				if(brushType == PrimitiveBrushType.Custom)
				{
					brushType = PrimitiveBrushType.Cube;
				}

				ResetPolygons();
            }
        }
			
//        public void OnDrawGizmosSelected()
//        {
//            // Ensure Edit Mode is on
//            GetCSGModel().EditMode = true;
//        }
//
//        public void OnDrawGizmos()
//        {
//            EnsureWellFormed();
//
//            //			Gizmos.color = Color.green;
//            //			for (int i = 0; i < PolygonFactory.hackyDisplay1.Count; i++) 
//            //			{
//            //				Gizmos.DrawSphere(PolygonFactory.hackyDisplay1[i], 0.2f);
//            //			}
//            //
//            //			Gizmos.color = Color.red;
//            //			for (int i = 0; i < PolygonFactory.hackyDisplay2.Count; i++) 
//            //			{
//            //				Gizmos.DrawSphere(PolygonFactory.hackyDisplay2[i], 0.2f);
//            //			}
//        }


		void OnDisable()
		{
			// OnDisable is called on recompilation, so make sure we only process when needed
			if(this.enabled == false || gameObject.activeInHierarchy == false)
			{
				GetCSGModel().OnBrushDisabled(this);
				for (int i = 0; i < brushCache.IntersectingVisualBrushes.Count; i++) 
				{
					if(brushCache.IntersectingVisualBrushes[i] != null)
					{
						brushCache.IntersectingVisualBrushes[i].RecalculateIntersections();
						brushCache.IntersectingVisualBrushes[i].BrushCache.SetUnbuilt();
					}
				}
			}
		}

		void UpdateTracking()
		{
			CSGModelBase parentCSGModel = GetCSGModel();

			// Make sure the CSG Model knows about this brush. If they duplicated a brush in the hierarchy then this
			// allows us to make sure the CSG Model knows about it
			if(parentCSGModel != null)
			{
				bool newBrush = parentCSGModel.TrackBrush(this);

				if(newBrush)
				{
					MeshFilter meshFilter = gameObject.AddOrGetComponent<MeshFilter>();

					meshFilter.sharedMesh = new Mesh();
					brushCache = new BrushCache();
					EnsureWellFormed();
					RecalculateBrushCache();
				}
				Invalidate(false);
				tracked = true;
			}
			else
			{
				tracked = false;
			}
		}

		void OnEnable()
		{
			UpdateTracking();
		}

		void Update()
		{
			if(!tracked)
			{
				UpdateTracking();
			}

			// If the transform has changed, needs rebuild
			if(cachedTransform.SetFromTransform(transform))
			{
				Invalidate(true);
			}
		}


        public override void Invalidate(bool polygonsChanged)
        {
			if(!gameObject.activeInHierarchy)
			{
				return;
			}

			// Make sure there is a mesh filter on this object
			MeshFilter meshFilter = gameObject.AddOrGetComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddOrGetComponent<MeshRenderer>();

			// Used to use mesh colliders for ray collision, but not any more so clean them up
			MeshCollider[] meshColliders = GetComponents<MeshCollider>();

			if(meshColliders.Length > 0)
			{
				for (int i = 0; i < meshColliders.Length; i++) 
				{
					DestroyImmediate(meshColliders[i]);
				}
			} 

			bool requireRegen = false;

			// If the cached ID hasn't been set or we mismatch
			if(cachedInstanceID == 0
				|| gameObject.GetInstanceID() != cachedInstanceID)
			{
				requireRegen = true;
				cachedInstanceID = gameObject.GetInstanceID();
			}


			Mesh renderMesh = meshFilter.sharedMesh;

			if(requireRegen)
			{
				renderMesh = new Mesh();
			}

			if(polygons != null)
			{
				List<int> polygonIndices;
	            PolygonFactory.GenerateMeshFromPolygons(polygons, ref renderMesh, out polygonIndices);
			}

			if(mode == CSGMode.Subtract)
			{
				PolygonFactory.Invert(ref renderMesh);
			}
			// Displace the triangles for display along the normals very slightly (this is so we can overlay built
			// geometry with semi-transparent geometry and avoid depth fighting)
			PolygonFactory.Displace(ref renderMesh, 0.001f);

			meshFilter.sharedMesh = renderMesh;
				
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			meshFilter.hideFlags = HideFlags.NotEditable;// | HideFlags.HideInInspector;
			meshRenderer.hideFlags = HideFlags.NotEditable;// | HideFlags.HideInInspector;

#if UNITY_EDITOR
			meshRenderer.sharedMaterial = UnityEditor.AssetDatabase.LoadMainAssetAtPath(CSGModel.GetSabreCSGPath() + "Materials/" + this.mode.ToString() + ".mat") as Material;
#endif
			isBrushConvex = PolygonFactory.IsMeshConvex(polygons);

			if(polygonsChanged)
			{
				RecalculateBrushCache();
			}

			UpdateVisibility();

			objectVersionSerialized++;
			objectVersionUnserialized = objectVersionSerialized;
        }

		public override void UpdateVisibility()
        {
			bool isVisible = GetCSGModel().AreBrushesVisible;
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isVisible;
            }
        }

//		public Polygon GetPolygonFromTriangle(int triangleIndex)
//        {
//            int polygonIndex = polygonIndices[triangleIndex];
//            return polygons[polygonIndex];
//        }

        public override Bounds GetBounds()
        {
			if (polygons.Length > 0)
			{
				Bounds bounds = new Bounds(polygons[0].Vertices[0].Position, Vector3.zero);
				
				for (int i = 0; i < polygons.Length; i++)
				{
					for (int j = 0; j < polygons[i].Vertices.Length; j++)
					{
						bounds.Encapsulate(polygons[i].Vertices[j].Position);
					}
				}
				return bounds;
			}
			else
			{
				return new Bounds(Vector3.zero, Vector3.zero);
			}
        }

		public override Bounds GetBoundsTransformed()
		{
			if (polygons.Length > 0)
			{
				Bounds bounds = new Bounds(transform.TransformPoint(polygons[0].Vertices[0].Position), Vector3.zero);

				for (int i = 0; i < polygons.Length; i++)
				{
					for (int j = 0; j < polygons[i].Vertices.Length; j++)
					{
						bounds.Encapsulate(transform.TransformPoint(polygons[i].Vertices[j].Position));
					}
				}
				return bounds;
			}
			else
			{
				return new Bounds(Vector3.zero, Vector3.zero);
			}
		}

		public Bounds GetBoundsLocalTo(Transform otherTransform)
		{
			if (polygons.Length > 0)
			{
				Bounds bounds = new Bounds(otherTransform.InverseTransformPoint(transform.TransformPoint(polygons[0].Vertices[0].Position)), Vector3.zero);

				for (int i = 0; i < polygons.Length; i++)
				{
					for (int j = 0; j < polygons[i].Vertices.Length; j++)
					{
						bounds.Encapsulate(otherTransform.InverseTransformPoint(transform.TransformPoint(polygons[i].Vertices[j].Position)));
					}
				}
				return bounds;
			}
			else
			{
				return new Bounds(Vector3.zero, Vector3.zero);
			}
		}

		public override int[] GetPolygonIDs ()
		{
			int[] ids = new int[polygons.Length];
			for (int i = 0; i < polygons.Length; i++) 
			{
				ids[i] = polygons[i].UniqueIndex;
			}
			return ids;
		}

		public override Polygon[] GetPolygons ()
		{
			return polygons;
		}

		public override int AssignUniqueIDs (int startingIndex)
		{
			for (int i = 0; i < polygons.Length; i++) 
			{
				int uniqueIndex = startingIndex + i;
				polygons[i].UniqueIndex = uniqueIndex;
			}

			int assignedCount = polygons.Length;
			
			return assignedCount;
		}

		public void ResetPivot()
		{			
			Vector3 delta = GetBounds().center;

			for (int i = 0; i < polygons.Length; i++) 
			{
				for (int j = 0; j < polygons[i].Vertices.Length; j++) 
				{
					polygons[i].Vertices[j].Position -= delta;
				}
			}

			// Bounds is aligned with the object
			transform.Translate(delta);

			// Counter the delta offset
			Transform[] childTransforms = transform.GetComponentsInChildren<Transform>(true);

			for (int i = 0; i < childTransforms.Length; i++) 
			{
				if(childTransforms[i] != transform)
				{
					childTransforms[i].Translate(-delta);
				}
			}

			// Only invalidate if it's actually been realigned
			if(delta != Vector3.zero)
			{
				Invalidate(true);
			}
		}
			
		public void Rescale (float rescaleValue)
		{
			Rescale(new Vector3(rescaleValue,rescaleValue,rescaleValue));
		}

		public void Rescale (Vector3 rescaleValue)
		{
			for (int i = 0; i < polygons.Length; i++) 
			{
				Polygon polygon = polygons[i];

				polygons[i].CalculatePlane();
				Vector3 previousPlaneNormal = polygons[i].Plane.normal;

				int vertexCount = polygon.Vertices.Length;

				Vector3[] newPositions = new Vector3[vertexCount];
				Vector2[] newUV = new Vector2[vertexCount];

				for (int j = 0; j < vertexCount; j++) 
				{
					newPositions[j] = polygon.Vertices[j].Position;
					newUV[j] = polygon.Vertices[j].UV;
				}

				for (int j = 0; j < vertexCount; j++) 
				{
					Vertex vertex = polygon.Vertices[j];

					Vector3 newPosition = vertex.Position.Multiply(rescaleValue);
					newPositions[j] = newPosition;

					newUV[j] = GeometryHelper.GetUVForPosition(polygon, newPosition);
				}

				// Apply all the changes to the polygon
				for (int j = 0; j < vertexCount; j++) 
				{
					Vertex vertex = polygon.Vertices[j];
					vertex.Position = newPositions[j];
					vertex.UV = newUV[j];
				}

				// Polygon geometry has changed, inform the polygon that it needs to recalculate its cached plane
				polygons[i].CalculatePlane();

				Vector3 newPlaneNormal = polygons[i].Plane.normal;

				// Find the rotation from the original polygon plane to the new polygon plane
				Quaternion normalRotation = Quaternion.FromToRotation(previousPlaneNormal, newPlaneNormal);

				// Rotate all the vertex normals by the new rotation
				for (int j = 0; j < vertexCount; j++) 
				{
					Vertex vertex = polygon.Vertices[j];
					vertex.Normal = normalRotation * vertex.Normal;
				}
			}
#if UNITY_EDITOR
			EditorHelper.SetDirty(this);
#endif
			Invalidate(true);
		}

		public GameObject Duplicate()
		{
			GameObject newObject = Instantiate(this.gameObject);

			newObject.transform.parent = this.transform.parent;

			return newObject;
		}

		public override void PrepareToBuild(List<Brush> brushes, bool forceRebuild)
		{
			if(forceRebuild)
			{
				brushCache.SetUnbuilt();
				RecalculateIntersections(brushes, true);
			}
		}

		public CSGModelBase GetCSGModel()
		{
			if (parentCsgModel == null)
			{
				CSGModelBase[] models = transform.GetComponentsInParent<CSGModelBase>(true);
				if(models.Length > 0)
				{
					parentCsgModel = models[0];
				}
			}
			return parentCsgModel;
		}

		public override BrushOrder GetBrushOrder ()
		{
			Transform csgModelTransform = GetCSGModel().transform;

			List<int> reversePositions = new List<int>();

			Transform traversedTransform = transform;

			reversePositions.Add(traversedTransform.GetSiblingIndex());

			while(traversedTransform.parent != null && traversedTransform.parent != csgModelTransform)
			{
				traversedTransform = traversedTransform.parent;
				reversePositions.Add(traversedTransform.GetSiblingIndex());
			}

			BrushOrder brushOrder = new BrushOrder();
			int count = reversePositions.Count;
			brushOrder.Position = new int[count];
			for (int i = 0; i < count; i++) 
			{
				brushOrder.Position[i] = reversePositions[count-1-i];
			}

			return brushOrder;
		}

#if (UNITY_5_0 || UNITY_5_1)
		void OnDrawGizmosSelected()
		{
			CSGModel parentCSGModel = GetCSGModel() as CSGModel;

			if(parentCSGModel != null)
			{
				// Ensure Edit Mode is on
				parentCSGModel.EditMode = true;
			}
		}
#endif
    }
}

#endif