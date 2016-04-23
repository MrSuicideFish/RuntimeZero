#if UNITY_EDITOR || RUNTIME_CSG

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace Sabresaurus.SabreCSG
{
	[ExecuteInEditMode, RequireComponent(typeof(CSGModelRuntime))]
	public class CSGModelBase : MonoBehaviour
	{
		public const string VERSION_STRING = "1.3.1";
		protected const string DEFAULT_MATERIAL_PATH = "Materials/Default_Map";

		// Limit to how many vertices a Unity mesh can hold, before it must be split into a second mesh (just under 2^16)
		protected const int MESH_VERTEX_LIMIT = 65500; 

		[SerializeField,HideInInspector] 
		protected List<Brush> brushes = new List<Brush>(); // Store the sequence of brushes and their operation (e.g. add, subtract)

		[SerializeField,HideInInspector]
		protected List<Brush> builtBrushes = new List<Brush>();

		[SerializeField,HideInInspector]
		protected MaterialMeshDictionary materialMeshDictionary = new MaterialMeshDictionary();

		[SerializeField,HideInInspector]
		protected List<Mesh> collisionMeshDictionary = new List<Mesh>();

		// An additional hint to the builder to tell it rebuilding is required
		[SerializeField,HideInInspector]
		protected bool polygonsRemoved = false;

		[SerializeField]
		protected CSGBuildSettings buildSettings = new CSGBuildSettings();

		[NonSerialized]
		protected CSGBuildContext buildContextBehaviour;

		// A reference to a component which holds a lot of build time data that helps change built geometry on the fly
		// This is used by the surface tools heavily.
		[NonSerialized]
		protected CSGBuildContext.BuildContext buildContext;

		public CSGBuildContext.BuildContext BuildContext
		{
			get
			{
				if(buildContext == null)
				{
					SetUpBuildContext();
				}
				return buildContext;
			}
		}

		public BuildMetrics BuildMetrics
		{
			get
			{
				return BuildContext.buildMetrics;
			}
		}

		public int BrushCount 
		{
			get
			{
				int brushCount = 0;
				for (int i = 0; i < brushes.Count; i++) 
				{
					if(brushes[i] != null)
					{
						brushCount++;
					}
				}
				return brushCount;
			}
		}


		public PolygonEntry GetVisualPolygonEntry(int index)
		{
			int entryCount = BuildContext.VisualPolygonIndex.Length;

			if(entryCount == 0 || index >= entryCount || index < 0)
			{
				// Return null if no polygons have been built or the index is out of range
				return null;
			}
			else
			{
				return BuildContext.VisualPolygonIndex[index];
			}
		}

		public PolygonEntry GetCollisionPolygonEntry(int index)
		{
			int entryCount = BuildContext.CollisionPolygonIndex.Length;

			if(entryCount == 0 || index >= entryCount || index < 0)
			{
				// Return null if no polygons have been built or the index is out of range
				return null;
			}
			else
			{
				return BuildContext.CollisionPolygonIndex[index];
			}
		}

		public List<Brush> GetBrushes()
		{
			return brushes;
		}

		void Awake()
		{
			SetUpBuildContext();	
		}

		void SetUpBuildContext()
		{
			// Get a reference to the build context (which holds post build helper data)
			buildContextBehaviour = this.AddOrGetComponent<CSGBuildContext>();
			buildContext = buildContextBehaviour.GetBuildContext();
		}

		protected virtual void Start()
		{
			UpdateBrushVisibility();
		}

		public virtual bool Build (bool forceRebuild)
		{
			brushes = new List<Brush>(transform.GetComponentsInChildren<Brush>(false));


			// Let each brush know it's about to be built
			for (int i = 0; i < brushes.Count; i++)
			{
				brushes[i].PrepareToBuild(brushes, forceRebuild);
			}

			Material defaultMaterial = GetDefaultMaterial();

			if(defaultMaterial == null)
			{
				Debug.LogError("Default material file is missing, try reimporting SabreCSG");
			}

			bool buildOccurred = CSGFactory.Build(brushes, 
				buildSettings, 
				buildContext, 
				this.transform, 
				GetDefaultMaterial(), 
				ref materialMeshDictionary, 
				ref collisionMeshDictionary,
				polygonsRemoved,
				forceRebuild,
				OnBuildProgressChanged);

			polygonsRemoved = false;

			if(buildOccurred)
			{
				UpdateBrushVisibility();

				// Mark the brushes that have been built (so we can differentiate later if new brushes are built or not)
				builtBrushes.Clear();
				builtBrushes.AddRange(brushes);

				FirePostBuildEvents();
			}

			return buildOccurred;
		}

		void FirePostBuildEvents()
		{
			Transform meshGroupTransform = this.transform.FindChild("MeshGroup");

			// Inform all methods with the PostProcessCSGBuildAttribute that a build just finished
			Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in allAssemblies) 
			{
				if(assembly.FullName.StartsWith("Assembly-CSharp"))
				{
					Type[] types = assembly.GetTypes();

					for (int i = 0; i < types.Length; i++) 
					{
						MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
						for (int j = 0; j < methods.Length; j++) 
						{
							if(Attribute.IsDefined(methods[j], typeof(PostProcessCSGBuildAttribute)))
							{
								methods[j].Invoke(null, new object[] { meshGroupTransform } );
							}
						}
					}
				}
			}

			// Inform all the scripts implementing IPostBuildListener on this model and inside it that a build finished
			IPostBuildListener[] postBuildListeners = this.transform.GetComponentsInChildren<IPostBuildListener>();

			for (int i = 0; i < postBuildListeners.Length; i++) 
			{
				postBuildListeners[i].OnBuildFinished(meshGroupTransform);
			}
		}

		public Mesh GetMeshForMaterial(Material sourceMaterial, int fitVertices = 0)
		{
			if(materialMeshDictionary.Contains(sourceMaterial))
			{
				List<MaterialMeshDictionary.MeshObjectMapping> mappings = materialMeshDictionary[sourceMaterial];

				Mesh lastMesh = mappings[mappings.Count-1].Mesh;

				if(lastMesh.vertices.Length + fitVertices < MESH_VERTEX_LIMIT)
				{
					return lastMesh;
				}
			}

			Mesh mesh = new Mesh();

			materialMeshDictionary.Add(sourceMaterial, mesh, null);

			if(sourceMaterial == null)
			{
				CSGFactory.CreateMaterialMesh(this.transform, GetDefaultMaterial(), mesh);
			}
			else
			{
				CSGFactory.CreateMaterialMesh(this.transform, sourceMaterial, mesh);
			}

			return mesh;
		}

		public Mesh GetMeshForCollision(int fitVertices = 0)
		{
			Mesh lastMesh = collisionMeshDictionary.Last();
			if(lastMesh.vertices.Length + fitVertices < MESH_VERTEX_LIMIT)
			{
				return lastMesh;
			}

			Mesh mesh = new Mesh();
			collisionMeshDictionary.Add(mesh);
			CSGFactory.CreateCollisionMesh(this.transform, mesh);
			return mesh;
		}

		/// <summary>
		/// Called to alert the CSG Model that a new brush has been created
		/// </summary>
		public bool TrackBrush(Brush brush)
		{
			// If we don't already know about the brush, add it
			if (!brushes.Contains(brush))
			{
				brushes.Add(brush);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void OnBrushDisabled(PrimitiveBrush brush)
		{
			polygonsRemoved = true;
		}

		public virtual bool AreBrushesVisible
		{
			get
			{
				return false;
			}
		}

		public Polygon RaycastBuiltPolygons(Ray ray)
		{
			if(BuildContext.VisualPolygons != null)
			{
				float distance = 0;
				return GeometryHelper.RaycastPolygons(BuildContext.VisualPolygons, ray, out distance);
			}
			else
			{
				return null;
			}
		}

		public Brush FindBrushFromPolygon(Polygon sourcePolygon)
		{
			// Find which brush contains the source polygon
			for (int i = 0; i < brushes.Count; i++) 
			{
				if(brushes[i] != null)
				{
					if(brushes[i].GetPolygonIDs().Contains(sourcePolygon.UniqueIndex))
					{
						return brushes[i];
					}
				}
			}

			// None found
			return null;
		}

		// Consider getting rid of this accessor!
		public List<Polygon> VisualPolygons
		{
			get
			{
				return BuildContext.VisualPolygons;
			}
		}

		public List<Polygon> GetAllSourcePolygons()
		{
			// Find the source polygon unique indexes of all the visual polygons
			List<Polygon> visualPolygons = BuildContext.VisualPolygons;
			List<int> visualPolygonIndexes = new List<int>();

			for (int i = 0; i < visualPolygons.Count; i++) 
			{
				if(!visualPolygonIndexes.Contains(visualPolygons[i].UniqueIndex))
				{
					visualPolygonIndexes.Add(visualPolygons[i].UniqueIndex);
				}
			}

			List<Polygon> sourcePolygons = new List<Polygon>(visualPolygonIndexes.Count);

			for (int i = 0; i < visualPolygonIndexes.Count; i++) 
			{
				Polygon sourcePolygon = GetSourcePolygon(visualPolygonIndexes[i]);
				sourcePolygons.Add(sourcePolygon);
			}
			return sourcePolygons;
		}

		public Polygon[] BuiltPolygonsByIndex(int uniquePolygonIndex)
		{
//			if(CurrentSettings.NewBuildEngine)
//			{
//				// TODO: Optimise this once Nova 1 is removed
//				List<Polygon> foundPolygons = new List<Polygon>();
//				for (int i = 0; i < brushes.Count; i++) 
//				{
//					if(brushes[i] != null)
//					{
//						List<Polygon> brushPolygons = brushes[i].BrushCache.BuiltPolygons;
//						for (int j = 0; j < brushPolygons.Count; j++) 
//						{
//							if(brushPolygons[j].UniqueIndex == uniquePolygonIndex)
//							{
//								foundPolygons.Add(brushPolygons[j]);
//							}
//						}
//					}
//				}
//				return foundPolygons.ToArray();
//			}
//			else
			{
				if(BuildContext == null || BuildContext.VisualPolygons == null)
				{
					return new Polygon[0];
				}


				return BuildContext.VisualPolygons.Where(poly => (poly.UniqueIndex == uniquePolygonIndex && !poly.ExcludeFromFinal)).ToArray();
			}
		}

//		public Polygon[] BuiltCollisionPolygonsByIndex(int uniquePolygonIndex)
//		{
//			if(buildContext == null || buildContext.collisionPolygons == null)
//			{
//				return new Polygon[0];
//			}
//
//			return buildContext.collisionPolygons.Where(poly => (poly.UniqueIndex == uniquePolygonIndex && !poly.ExcludeFromFinal)).ToArray();
//		}

		public List<RaycastHit> RaycastBrushesAllOld(Ray ray)
		{
			int layerMask = 1 << LayerMask.NameToLayer("CSGMesh");
			// Invert the layer mask
			layerMask = ~layerMask;

			List<RaycastHit> hits = Physics.RaycastAll(ray, float.PositiveInfinity, layerMask).ToList();

			// Trim out any calculated collision meshes that have been hit
			for (int i = 0; i < hits.Count; i++) 
			{
				if(hits[i].collider.name == "CollisionMesh")
				{
					hits.RemoveAt (i);
					i--;
				}
			}

			// Trim out duplicate colliders on the same game object
			for (int j = 0; j < hits.Count; j++) 
			{
				for (int i = 0; i < hits.Count; i++) 
				{
					if(i != j)
					{
						if(hits[i].collider.gameObject == hits[j].collider.gameObject)
						{
							hits.RemoveAt (j);
							j--;
							break;
						}
					}
				}
			}

			hits.Sort((x,y) => x.distance.CompareTo(y.distance));
			return hits;
		}

		public List<PolygonRaycastHit> RaycastBrushesAll(Ray ray)
		{
			List<PolygonRaycastHit> hits = new List<PolygonRaycastHit>();

			for (int i = 0; i < brushes.Count; i++)
			{
				if(brushes[i] == null)
				{
					continue;
				}
//				Bounds bounds = brushes[i].GetBoundsTransformed();
//				if(bounds.IntersectRay(ray))
				{
					Polygon[] polygons = brushes[i].GenerateTransformedPolygons();
					float hitDistance;
					Polygon hitPolygon = GeometryHelper.RaycastPolygons(polygons.ToList(), ray, out hitDistance);
					if(hitPolygon != null)
					{
						hits.Add(new PolygonRaycastHit() 
							{ 
								Distance = hitDistance,
								Point = ray.GetPoint(hitDistance),
								Normal = hitPolygon.Plane.normal,
								GameObject = brushes[i].gameObject,
							}
						);
					}
				}
			}

			hits.Sort((x,y) => x.Distance.CompareTo(y.Distance));
			return hits;
		}

		public virtual void UpdateBrushVisibility()
		{
			for (int i = 0; i < brushes.Count; i++)
			{
				if (brushes[i] != null)
				{
					brushes[i].UpdateVisibility();
				}
			}
		}

		public bool HasBrushBeenBuilt(Brush candidateBrush)
		{
			return builtBrushes.Contains(candidateBrush);
		}

		public GameObject CreateBrush(PrimitiveBrushType brushType)
		{
			GameObject brushObject = new GameObject("AppliedBrush");
			brushObject.transform.parent = this.transform;
			PrimitiveBrush primitiveBrush = brushObject.AddComponent<PrimitiveBrush>();
			primitiveBrush.BrushType = brushType;
			primitiveBrush.ResetPolygons();

			return brushObject;
		}

		public GameObject CreateCustomBrush(Polygon[] polygons)
		{
			GameObject brushObject = new GameObject("AppliedBrush");
			brushObject.transform.parent = this.transform;
			PrimitiveBrush primitiveBrush = brushObject.AddComponent<PrimitiveBrush>();
			primitiveBrush.SetPolygons(polygons, true);

			return brushObject;
		}

		public Polygon GetSourcePolygon(int uniqueIndex)
		{
			for (int i = 0; i < brushes.Count; i++) 
			{
				if(brushes[i] != null)
				{
					Polygon[] polygons = brushes[i].GetPolygons();
					for (int j = 0; j < polygons.Length; j++) 
					{
						if(polygons[j].UniqueIndex == uniqueIndex)
						{
							return polygons[j];
						}
					}
				}
			}

			// None found
			return null;
		}

		static CSGModel FindCSGModel()
		{
			CSGModel[] models = UnityEngine.Object.FindObjectsOfType<CSGModel>();
			if (models.Length > 0)
			{
				if (models.Length > 1)
				{
					Debug.LogWarning("Multiple CSGModels detected in scene");
				}
				return models[0];
			}
			else
			{
				Debug.LogError("Couldn't find a CSGModel in the scene");
				return null;
			}
		}

		public void RefreshMeshGroup()
		{
			// For some reason mesh colliders don't update when you change the mesh, you have to flush them by
			// either setting the mesh null and resetting it, or turning the object off and on again
			Transform meshGroup = transform.FindChild("MeshGroup");

			if(meshGroup != null && meshGroup.gameObject.activeInHierarchy)
			{
				meshGroup.gameObject.SetActive(false);
				meshGroup.gameObject.SetActive(true);
			}
		}

		public virtual void OnBuildProgressChanged(float progress)
		{
		}

		public void NotifyPolygonsRemoved()
		{
			polygonsRemoved = true;
		}

		public virtual Material GetDefaultMaterial()
		{
			return Resources.Load(DEFAULT_MATERIAL_PATH) as Material;
		}

		public class RayHitComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return ((RaycastHit) x).distance.CompareTo(((RaycastHit) y).distance);
			}
		}
	}
}
#endif