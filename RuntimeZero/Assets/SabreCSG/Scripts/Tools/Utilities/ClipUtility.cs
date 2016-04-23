#if UNITY_EDITOR || RUNTIME_CSG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sabresaurus.SabreCSG
{
	public static class ClipUtility
	{
		public static void InsertEdgeLoop(PrimitiveBrush brush, Plane localClipPlane)
		{
			// Clip the polygons against the plane
			List<Polygon> polygonsFront;
			List<Polygon> polygonsBack;

			if(PolygonFactory.SplitPolygonsByPlane(new List<Polygon>(brush.GetPolygons()), localClipPlane, true, out polygonsFront, out polygonsBack))
			{
				List<Polygon> allPolygons = new List<Polygon>();
				// Concat back into one list
				allPolygons.AddRange(polygonsFront);
				allPolygons.AddRange(polygonsBack);
				// Remove the inserted polygons
				for (int i = 0; i < allPolygons.Count; i++) 
				{
					if(allPolygons[i].ExcludeFromFinal)
					{
						allPolygons.RemoveAt(i);
						i--;
					}
				}
				// Update the brush with the new polygons
				brush.SetPolygons(allPolygons.ToArray(), true);
			}
		}
	}
}
#endif