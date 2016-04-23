﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Sabresaurus.SabreCSG
{
    public class ClipEditor : Tool
    {
        // Rotation and position is used with the handles to construct the clip plane
        Vector3 planePosition;

		Plane displayPlane = new Plane(Vector3.up, 0);

		bool displayPoint = false;

		// These three points are used to define the clip plane
		Vector3[] points = new Vector3[3];

		int pointSelected = -1;
		Vector3 startPosition;

		bool planeEstablished = false;

		// Whether the plane has been reversed by the user
		bool isFlipped = false;

        public override void ResetTool()
        {
			if(primaryTargetBrush != null)
			{
				planePosition = primaryTargetBrush.transform.TransformPoint(primaryTargetBrush.GetBounds().center);
			}

			pointSelected = -1;

			displayPoint = false;

			points[0] = Vector3.zero;
			points[1] = Vector3.zero;
			points[2] = Vector3.zero;

            displayPlane = new UnityEngine.Plane(Vector3.up, planePosition);

			isFlipped = false;

			planeEstablished = false;
        }

        public override void OnSceneGUI(SceneView sceneView, Event e)
        {
			base.OnSceneGUI(sceneView, e); // Allow the base logic to calculate first
			
			// If any points are selected let the handles move them
			if(pointSelected > -1)
			{
				EditorGUI.BeginChangeCheck();
				// Display a handle and allow the user to determine a new position in world space
				Vector3 newWorldPosition = Handles.PositionHandle(points[pointSelected], Quaternion.identity);
				
				if(EditorGUI.EndChangeCheck())
				{
					Vector3 newPosition = newWorldPosition;
					
					Vector3 accumulatedDelta = newPosition - startPosition;
					
					if(CurrentSettings.PositionSnappingEnabled)
					{
						float snapDistance = CurrentSettings.PositionSnapDistance;
						accumulatedDelta = MathHelper.RoundVector3(accumulatedDelta, snapDistance);
					}
					
					newPosition = startPosition + accumulatedDelta;

					points[pointSelected] = newPosition;

					e.Use();
				}
			}



			// First let's see if we can select any existing points
			if(e.type == EventType.MouseDown || e.type == EventType.MouseUp)
			{
				if(!EditorHelper.IsMousePositionNearSceneGizmo(e.mousePosition))
				{
					OnMouseSelection(sceneView, e);
				}
			}

			if(e.button == 0 && e.type == EventType.MouseDrag && !CameraPanInProgress)
			{
				planeEstablished = false;
			}

            // Forward specific events on to handlers
			if (e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.MouseDrag || e.type == EventType.MouseMove)
            {
				if(sceneView.camera.orthographic && EditorHelper.GetSceneViewCamera(sceneView) != EditorHelper.SceneViewCamera.Other)
				{
					OnMouseActionOrthographic(sceneView, e);
				}
				else
				{
					OnMouseAction3D(sceneView, e);
				}
            }
            else if (e.type == EventType.Repaint || e.type == EventType.Layout)
            {
                OnRepaint(sceneView, e);
            }
            else if (e.type == EventType.KeyDown || e.type == EventType.KeyUp)
            {
                OnKeyAction(sceneView, e);
            }
        }

		void OnMouseSelection (SceneView sceneView, Event e)
		{
			if(e.button != 0 || CameraPanInProgress)
			{
				return;
			}

			pointSelected = -1; 

			if(planeEstablished)
			{
//				Vector3 sceneViewPosition = sceneView.camera.transform.position;
				
				Vector2 mousePosition = e.mousePosition;

				for (int i = 0; i < points.Length; i++) 
				{
//					float vertexDistanceSquare = (sceneViewPosition - points[i]).sqrMagnitude;
					
					if(EditorHelper.InClickZone(mousePosition, points[i]))
					{
						if(e.type == EventType.MouseUp)
						{
							pointSelected = i;
							startPosition = points[pointSelected];
						}
						e.Use();
					}
				}
			}
		}

		void OnMouseActionOrthographic(SceneView sceneView, Event e)
		{
			if(primaryTargetBrush == null || CameraPanInProgress || e.button != 0)
			{
				return;
			}

			
			if(EditorHelper.IsMousePositionNearSceneGizmo(e.mousePosition))
			{
				return;
			}

			Vector2 mousePosition = e.mousePosition;
			mousePosition = EditorHelper.ConvertMousePointPosition(mousePosition);
			
			Ray ray = Camera.current.ScreenPointToRay(mousePosition);

			Plane plane = new Plane(sceneView.camera.transform.forward, primaryTargetBrushTransform.position);
			Vector3 worldPoint; // This is the point on the plane that is perpendicular to the camera
			float distance = 0;
			if(plane.Raycast(ray, out distance))
			{
				worldPoint = ray.GetPoint(distance);
			}
			else
			{
				return;
			}
				
			if(e.type == EventType.MouseDown || e.type == EventType.MouseMove)
			{
				if(e.type == EventType.MouseMove && planeEstablished)
				{
					return;
				}

				points[0] = worldPoint;
				displayPoint = true;

				if(CurrentSettings.PositionSnappingEnabled)
				{
					float snapDistance = CurrentSettings.PositionSnapDistance;
					points[0] = MathHelper.RoundVector3(points[0], snapDistance);
				}
				
				points[1] = points[0];
				points[2] = points[0];
				
				isFlipped = false;
			}
			else
			{
				points[1] = worldPoint;
				
				if(CurrentSettings.PositionSnappingEnabled)
				{
					float snapDistance = CurrentSettings.PositionSnapDistance;
					points[1] = MathHelper.RoundVector3(points[1], snapDistance);
				}
				points[2] = points[0] + sceneView.camera.transform.forward;

				if(e.type == EventType.MouseUp)
				{
					planeEstablished = true;

					if(points[1] == points[0])
					{
						ResetTool();
					}
				}
			}
			SceneView.RepaintAll();
		}

        void OnMouseAction3D(SceneView sceneView, Event e)
        {
			if(primaryTargetBrush == null || CameraPanInProgress || e.button != 0)
			{
				return;
			}

			Vector2 mousePosition = e.mousePosition;
			mousePosition = EditorHelper.ConvertMousePointPosition(mousePosition);
			
			Ray ray = Camera.current.ScreenPointToRay(mousePosition);
			float bestDistance = float.PositiveInfinity;

			Polygon bestPolygon = null;

			float testDistance;
			foreach (Brush brush in targetBrushes) 
			{
				List<Polygon> polygons = brush.GenerateTransformedPolygons().ToList();
				Polygon testPolygon = GeometryHelper.RaycastPolygons(polygons, ray, out testDistance, 0.1f);
				if(testPolygon != null && testDistance < bestDistance)
				{
					bestDistance = testDistance;
					bestPolygon = testPolygon;
				}
			}

			if(bestPolygon != null)
			{
//				VisualDebug.ClearAll();
//				VisualDebug.AddPolygon(hitPolygon, Color.red);
				Vector3 hitPoint = ray.GetPoint(bestDistance);

				if(e.type == EventType.MouseDown || e.type == EventType.MouseMove)
				{
					if(e.type == EventType.MouseMove && planeEstablished)
					{
						return;
					}

					points[0] = hitPoint;
					displayPoint = true;

					if(CurrentSettings.PositionSnappingEnabled)
					{
						float snapDistance = CurrentSettings.PositionSnapDistance;
						points[0] = MathHelper.RoundVector3(points[0], snapDistance);
					}

					points[1] = points[0];
					points[2] = points[0];

					isFlipped = false;
				}
				else
				{
					points[1] = hitPoint;

					if(CurrentSettings.PositionSnappingEnabled)
					{
						float snapDistance = CurrentSettings.PositionSnapDistance;
						points[1] = MathHelper.RoundVector3(points[1], snapDistance);
					}
					points[2] = points[0] - bestPolygon.Plane.normal;

					if(e.type == EventType.MouseUp)
					{
						planeEstablished = true;

						if(points[1] == points[0])
						{
							ResetTool();
						}
					}
				}
				SceneView.RepaintAll();
			}

		}
			
		void OnKeyAction(SceneView sceneView, Event e)
        {
			if (KeyMappings.EventsMatch(e, Event.KeyboardEvent(KeyMappings.ApplyClip)))
            {
                if (e.type == EventType.KeyUp)
                {
                    ApplyClipPlane(false);
                }
                e.Use();
            }
			else if (KeyMappings.EventsMatch(e, Event.KeyboardEvent(KeyMappings.ApplySplit)))
			{
				if (e.type == EventType.KeyUp)
				{
					ApplyClipPlane(true);
				}
				e.Use();
			}
			else if (KeyMappings.EventsMatch(e, Event.KeyboardEvent(KeyMappings.InsertEdgeLoop)))
			{
				if (e.type == EventType.KeyUp)
				{
					InsertEdgeLoop();
				}
				e.Use();
			}
			else if (KeyMappings.EventsMatch(e, Event.KeyboardEvent(KeyMappings.FlipPlane)))
            {
                if (e.type == EventType.KeyUp)
                {
					isFlipped = !isFlipped;
                }
                e.Use();
            }
			else if (KeyMappings.EventsMatch(e, Event.KeyboardEvent("Escape")))
			{
				if (e.type == EventType.KeyUp)
				{
					ResetTool();
				}
				e.Use();
			}
        }

        void OnRepaint(SceneView sceneView, Event e)
        {
			if(primaryTargetBrush != null)
			{
	            // Use a helper method to draw a visualisation of the clipping plane
				float largestExtent = GetBounds().GetLargestExtent();
				float planeSize = largestExtent * 4f;
//				clipPlane.
	            SabreGraphics.DrawPlane(displayPlane, planePosition, new Color(0f, 1f, 0f, .3f), new Color(1f, 0f, 0f, .3f), planeSize);

				// Selected brush green outline
				SabreGraphics.GetSelectedBrushMaterial().SetPass(0);

				// Draw black lines where the clip plane intersects the brushes
				if(!sceneView.camera.orthographic || EditorHelper.GetSceneViewCamera(sceneView) == EditorHelper.SceneViewCamera.Other)
				{
					GL.Begin(GL.LINES);
					GL.Color(Color.black);

					foreach (PrimitiveBrush brush in targetBrushes) 
					{
						Polygon[] polygons = brush.GenerateTransformedPolygons();
						foreach (Polygon polygon in polygons) 
						{
							Vector3 position1;
							Vector3 position2;
							if(Polygon.PlanePolygonIntersection(polygon, out position1, out position2, displayPlane))
							{
								GL.Vertex(position1);
								GL.Vertex(position2);
							}
						}
					}

					GL.End();
				}

				if(displayPoint)
				{
					Camera sceneViewCamera = sceneView.camera;
				
					SabreGraphics.GetVertexMaterial().SetPass (0);
					GL.PushMatrix();
					GL.LoadPixelMatrix();
						
					GL.Begin(GL.QUADS);

					
					// Draw points in reverse order because we want the precedence to be Red, Green, Blue
					
					GL.Color(Color.blue);
					
					Vector3 target = sceneViewCamera.WorldToScreenPoint(points[2]);
					
					if(target.z > 0)
					{
						// Make it pixel perfect
						target = MathHelper.RoundVector3(target);
						SabreGraphics.DrawBillboardQuad(target, 8, 8);
					}

					GL.Color(Color.green);
					
					target = sceneViewCamera.WorldToScreenPoint(points[1]);
					
					if(target.z > 0)
					{
						// Make it pixel perfect
						target = MathHelper.RoundVector3(target);
						SabreGraphics.DrawBillboardQuad(target, 8, 8);
					}

					GL.Color(Color.red);
					
					target = sceneViewCamera.WorldToScreenPoint(points[0]);
					
					if(target.z > 0)
					{
						// Make it pixel perfect
						target = MathHelper.RoundVector3(target);
						SabreGraphics.DrawBillboardQuad(target, 8, 8);
					}

					GL.End();
					GL.PopMatrix();
				}
			}

            // Draw UI specific to this editor
//			GUI.backgroundColor = Color.red;
            Rect rectangle = new Rect(0, 50, 210, 130);
			GUIStyle toolbar = new GUIStyle(EditorStyles.toolbar);
			toolbar.normal.background = SabreGraphics.ClearTexture;
			toolbar.fixedHeight = rectangle.height;
			GUILayout.Window(140006, rectangle, OnToolbarGUI, "",toolbar);
        }

        void OnToolbarGUI(int windowID)
        {
			GUI.enabled = (primaryTargetBrush != null);
            EditorGUILayout.BeginHorizontal();

			if (SabreGUILayout.Button("Clip"))
            {                
                ApplyClipPlane(false);
            }

			if (SabreGUILayout.Button("Split"))
			{
                ApplyClipPlane(true);
			}

			if (SabreGUILayout.Button("Edge Loop"))
			{                
				InsertEdgeLoop();
			}

            EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();

			if (SabreGUILayout.Button("Flip Plane"))
			{
				isFlipped = !isFlipped;
			}

			GUI.enabled = (pointSelected > -1);
			if(SabreGUILayout.Button("Snap To Grid"))
			{
				float snapDistance = CurrentSettings.PositionSnapDistance;
				Vector3 newPosition = points[pointSelected];
				newPosition = primaryTargetBrush.transform.TransformPoint(newPosition);
				newPosition = MathHelper.RoundVector3(newPosition, snapDistance);
				newPosition = primaryTargetBrush.transform.InverseTransformPoint(newPosition);

				points[pointSelected] = newPosition;
			}

			EditorGUILayout.EndHorizontal();

			if(isFlipped)
			{
				displayPlane = new Plane(points[0], points[1], points[2]);
			}
			else
			{
				displayPlane = new Plane(points[2], points[1], points[0]);
			}

			planePosition = (points[0] + points[1] + points[2]) / 3f;
        }

        void ApplyClipPlane(bool keepBothSides)
        {
			List<Object> newObjects = new List<Object>();
			foreach (PrimitiveBrush brush in targetBrushes) 
			{
				if(brush != null)
				{
					if(keepBothSides)
					{
						Undo.RecordObject(brush, "Split Brush");
					}
					else
					{
						Undo.RecordObject(brush, "Clipped Brush");
					}

					// Recalculate the clip plane from the world points, converting to local space for this transform
					Plane localClipPlane;

					// If the user has specified to flip the plane, flip the plane we just calculated
					if(isFlipped)
					{
						localClipPlane = new Plane(brush.transform.InverseTransformPoint(points[0]),
							brush.transform.InverseTransformPoint(points[1]), 
							brush.transform.InverseTransformPoint(points[2]));
					}
					else
					{
						localClipPlane = new Plane(brush.transform.InverseTransformPoint(points[2]),
							brush.transform.InverseTransformPoint(points[1]), 
							brush.transform.InverseTransformPoint(points[0]));
					}

		            // Clip the polygons against the plane
					List<Polygon> polygonsFront;
					List<Polygon> polygonsBack;
					
					if(PolygonFactory.SplitPolygonsByPlane(brush.GetPolygons().ToList(), localClipPlane, false, out polygonsFront, out polygonsBack))
					{
						// Update the brush with the new polygons
						brush.SetPolygons(polygonsFront.ToArray(), true);

						// If they have decided to split instead of clip, create a second brush with the other side
						if(keepBothSides)
						{
							GameObject newObject = brush.Duplicate();

							// Finally give the new brush the other set of polygons
							newObject.GetComponent<PrimitiveBrush>().SetPolygons(polygonsBack.ToArray(), true);
							newObject.transform.SetSiblingIndex(brush.transform.GetSiblingIndex());
							Undo.RegisterCreatedObjectUndo(newObject, "Split Brush");
							newObjects.Add(newObject);
						}
					}
				}
			}

			// Add any new objects to the selection
			if(newObjects.Count > 0)
			{
				// First of all add the existing selection to the start of the new objects list
				newObjects.InsertRange(0, Selection.objects);
				// Then replace the selected objects with the new objects list in array form
				Selection.objects = newObjects.ToArray();
			}
        }


		void InsertEdgeLoop()
		{
			foreach (PrimitiveBrush brush in targetBrushes) 
			{
				if(brush != null)
				{
					Undo.RecordObject(brush, "Insert Edge Loop");

					// Recalculate the clip plane from the world points, converting to local space for this transform
					Plane localClipPlane = new Plane(brush.transform.InverseTransformPoint(points[2]),
						brush.transform.InverseTransformPoint(points[1]), 
						brush.transform.InverseTransformPoint(points[0]));

					ClipUtility.InsertEdgeLoop(brush, localClipPlane);
				}
			}
		}

		public override void Deactivated ()
		{

		}
    }
}
#endif