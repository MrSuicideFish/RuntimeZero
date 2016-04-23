#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sabresaurus.SabreCSG
{
	public class SabreCSGPreferences : EditorWindow
	{
		const string RUNTIME_CSG_DEFINE = "RUNTIME_CSG";
		static readonly Vector2 WINDOW_SIZE = new Vector2(370,360);

		static Event cachedEvent;

		public static void CreateAndShow()
		{
			// Unity API doens't allow us to bring up the preferences, so just create a window that will display it
			SabreCSGPreferences window = EditorWindow.GetWindow<SabreCSGPreferences>(true, "SabreCSG Preferences", true);

			// By setting both sizes to the same, even the resize cursor hover is automatically disabled
			window.minSize = WINDOW_SIZE;
			window.maxSize = WINDOW_SIZE;

			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Label("SabreCSG Preferences", SabreGUILayout.GetTitleStyle(20));
			PreferencesGUI();

		}

		[PreferenceItem("SabreCSG")]
		public static void PreferencesGUI()
		{
// 			Commented out for 1.2.5 release as runtime code is currently not finished
//			GUIStyle style = SabreGUILayout.GetForeStyle();
//			style.wordWrap = true;
//			GUILayout.Label("Runtime CSG allows you to create, alter and build brushes at runtime in your built applications.", style);
//			BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
//			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
//			List<string> definesSplit = defines.Split(';').ToList();
//			bool enabled = definesSplit.Contains(RUNTIME_CSG_DEFINE);
//
//			if(enabled)
//			{
//				if(GUILayout.Button("Disable Runtime CSG"))
//				{
//					definesSplit.Remove(RUNTIME_CSG_DEFINE);
//					defines = string.Join(";", definesSplit.ToArray());
//					PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
//				}
//			}
//			else
//			{
//				if(GUILayout.Button("Enable Runtime CSG"))
//				{
//					definesSplit.Add(RUNTIME_CSG_DEFINE);
//					defines = string.Join(";", definesSplit.ToArray());
//					PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
//				}
//			}

//			Event.current.GetTypeForControl
//
//			if(Event.current.type == EventType.KeyDown)
//			{
//				cachedEvent = new Event(Event.current);
////				this.Repaint();
//			}
//
//			GUILayout.TextField("");
//
//			if(cachedEvent != null)
//			{
//				GUILayout.Label(cachedEvent.ToString());
//			}
//			else
//			{
//				GUILayout.Label("No event");
//			}

			bool newHideGridInPerspective = GUILayout.Toggle(CurrentSettings.HideGridInPerspective, "Hide grid in perspective scene views");

			if(newHideGridInPerspective != CurrentSettings.HideGridInPerspective)
			{
				SceneView.RepaintAll();			
				CurrentSettings.HideGridInPerspective = newHideGridInPerspective;
			}


			CurrentSettings.OverrideFlyCamera = GUILayout.Toggle(CurrentSettings.OverrideFlyCamera, "Linear fly camera");

			GUILayout.FlexibleSpace();

			GUILayout.Label("SabreCSG Version " + CSGModel.VERSION_STRING, SabreGUILayout.GetForeStyle());
		}
	}
}
#endif