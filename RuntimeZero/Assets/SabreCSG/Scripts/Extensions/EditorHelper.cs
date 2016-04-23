#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;

#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Sabresaurus.SabreCSG
{
	public static class EditorHelper
	{
	    // Threshold for raycasting vertex clicks, in screen space (should match half the icon dimensions)
	    const float CLICK_THRESHOLD = 15;

	    // Used for offseting mouse position
	    const int TOOLBAR_HEIGHT = 37;

		public static bool HasDelegate (System.Delegate mainDelegate, System.Delegate targetListener)
		{
			if (mainDelegate != null)
			{
				if (mainDelegate.GetInvocationList().Contains(targetListener))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

	    public static bool SceneViewHasDelegate(SceneView.OnSceneFunc targetDelegate)
	    {
			return HasDelegate(SceneView.onSceneGUIDelegate, targetDelegate);
	    }

	    public enum SceneViewCamera { Top, Bottom, Left, Right, Front, Back, Other };

		public static SceneViewCamera GetSceneViewCamera(SceneView sceneView)
		{
			return GetSceneViewCamera(sceneView.camera);
		}
		public static SceneViewCamera GetSceneViewCamera(Camera camera)
	    {
	        Vector3 cameraForward = camera.transform.forward;

	        if (cameraForward == new Vector3(0, -1, 0))
	        {
	            return SceneViewCamera.Top;
	        }
	        else if (cameraForward == new Vector3(0, 1, 0))
	        {
	            return SceneViewCamera.Bottom;
	        }
	        else if (cameraForward == new Vector3(1, 0, 0))
	        {
	            return SceneViewCamera.Left;
	        }
	        else if (cameraForward == new Vector3(-1, 0, 0))
	        {
	            return SceneViewCamera.Right;
	        }
	        else if (cameraForward == new Vector3(0, 0, -1))
	        {
	            return SceneViewCamera.Front;
	        }
	        else if (cameraForward == new Vector3(0, 0, 1))
	        {
	            return SceneViewCamera.Back;
	        }
	        else
	        {
	            return SceneViewCamera.Other;
	        }
	    }

	    /// <summary>
	    /// Whether the mouse position is within the bounds of the axis snapping gizmo that appears in the top right
	    /// </summary>
	    public static bool IsMousePositionNearSceneGizmo(Vector2 mousePosition)
	    {
			float scale = 1;

#if UNITY_5_4_OR_NEWER
			mousePosition = EditorGUIUtility.PointsToPixels(mousePosition);
			scale = EditorGUIUtility.pixelsPerPoint;
#endif
			
	        mousePosition.x = Screen.width - mousePosition.x;

			if (mousePosition.x > 14 * scale 
				&& mousePosition.x < 89 * scale 
				&& mousePosition.y > 14 * scale 
				&& mousePosition.y < 105 * scale)
	        {
	            return true;
	        }
	        else
	        {
	            return false;
	        }
	    }

		public static Vector2 ConvertMousePointPosition(Vector2 sourceMousePosition, bool convertPointsToPixels = true)
	    {
#if UNITY_5_4_OR_NEWER
			if(convertPointsToPixels)
			{
				sourceMousePosition = EditorGUIUtility.PointsToPixels(sourceMousePosition);
			}
			// Flip the direction of Y and remove the Scene View top toolbar's height
			sourceMousePosition.y = Screen.height - sourceMousePosition.y - (TOOLBAR_HEIGHT * EditorGUIUtility.pixelsPerPoint);
#else
			// Flip the direction of Y and remove the Scene View top toolbar's height
			sourceMousePosition.y = Screen.height - sourceMousePosition.y - TOOLBAR_HEIGHT;
#endif
	        return sourceMousePosition;
	    }

		public static Vector2 ConvertMousePixelPosition(Vector2 sourceMousePosition, bool convertPixelsToPoints = true)
		{
#if UNITY_5_4_OR_NEWER
			if(convertPixelsToPoints)
			{
				sourceMousePosition = EditorGUIUtility.PixelsToPoints(sourceMousePosition);
			}
			// Flip the direction of Y and remove the Scene View top toolbar's height
			sourceMousePosition.y = (Screen.height / EditorGUIUtility.pixelsPerPoint) - sourceMousePosition.y - (TOOLBAR_HEIGHT);
#else
			// Flip the direction of Y and remove the Scene View top toolbar's height
			sourceMousePosition.y = Screen.height - sourceMousePosition.y - TOOLBAR_HEIGHT;
#endif
			return sourceMousePosition;
		}

		public static bool IsMousePositionInIMGUIRect(Vector2 mousePosition, Rect rect)
		{
			// This works in point space, not pixel space
			mousePosition += new Vector2(0, EditorStyles.toolbar.fixedHeight);

			return rect.Contains(mousePosition);
		}

	    public static bool InClickZone(Vector2 mousePosition, Vector3 worldPosition)
	    {
	        mousePosition = ConvertMousePointPosition(mousePosition);
	        Vector3 targetScreenPosition = Camera.current.WorldToScreenPoint(worldPosition);

	        if (targetScreenPosition.z < 0)
	        {
	            return false;
	        }

	        float distance = Vector2.Distance(mousePosition, targetScreenPosition);


			// When z is 6 then click threshold is 15
			// when z is 20 then click threshold is 5
			float threshold = Mathf.Lerp(15, 5, Mathf.InverseLerp(6,20,targetScreenPosition.z));

#if UNITY_5_4_OR_NEWER
			threshold *= EditorGUIUtility.pixelsPerPoint;
#endif

			if (distance <= threshold)
	        {
	            return true;
	        }
	        else
	        {
	            return false;
	        }
	    }

		public static bool InClickRect(Vector2 mousePosition, Vector3 worldPosition1, Vector3 worldPosition2)
		{
			mousePosition = ConvertMousePointPosition(mousePosition);
			Vector3 targetScreenPosition1 = Camera.current.WorldToScreenPoint(worldPosition1);
			Vector3 targetScreenPosition2 = Camera.current.WorldToScreenPoint(worldPosition2);

			if (targetScreenPosition1.z < 0)
			{
				return false;
			}

			// When z is 6 then click threshold is 15
			// when z is 20 then click threshold is 5
			float threshold = Mathf.Lerp(15, 5, Mathf.InverseLerp(6,20,targetScreenPosition1.z));

#if UNITY_5_4_OR_NEWER
			threshold *= EditorGUIUtility.pixelsPerPoint;
#endif

			Vector3 closestPoint = MathHelper.ProjectPointOnLineSegment(targetScreenPosition1, targetScreenPosition2, mousePosition);
			closestPoint.z = 0;
			if(Vector3.Distance(closestPoint, mousePosition) < threshold)
//			if(mousePosition.y > Mathf.Min(targetScreenPosition1.y, targetScreenPosition2.y - threshold)
//				&& mousePosition.y < Mathf.Max(targetScreenPosition1.y, targetScreenPosition2.y) + threshold
//				&& mousePosition.x > Mathf.Min(targetScreenPosition1.x, targetScreenPosition2.x - threshold)
//				&& mousePosition.x < Mathf.Max(targetScreenPosition1.x, targetScreenPosition2.x) + threshold)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	    public static Vector3 CalculateWorldPoint(SceneView sceneView, Vector3 screenPoint)
	    {
	        screenPoint = ConvertMousePointPosition(screenPoint);

	        return sceneView.camera.ScreenToWorldPoint(screenPoint);
	    }

//		public static string GetCurrentSceneGUID()
//		{
//			string currentScenePath = EditorApplication.currentScene;
//			if(!string.IsNullOrEmpty(currentScenePath))
//			{
//				return AssetDatabase.AssetPathToGUID(currentScenePath);
//			}
//			else
//			{
//				// Scene hasn't been saved
//				return null;
//			}
//		}

		public static void SetDirty(Object targetObject)
		{
			EditorUtility.SetDirty(targetObject);

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			// As of Unity 5, SetDirty no longer marks the scene as dirty. Need to use the new call for that.
			EditorApplication.MarkSceneDirty();
#else // 5.3 and above introduce multiple scene management via EditorSceneManager
			Scene activeScene = EditorSceneManager.GetActiveScene();
			EditorSceneManager.MarkSceneDirty(activeScene);
#endif
		}

		public class TransformIndexComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return ((Transform) x).GetSiblingIndex().CompareTo(((Transform) y).GetSiblingIndex());
			}
		}
	}
}
#endif