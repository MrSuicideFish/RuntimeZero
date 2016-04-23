#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEditor;

namespace Sabresaurus.SabreCSG
{
	/// <summary>
	/// Central store for SabreCSG key mappings, change these constants to change the key bindings
	/// </summary>
	public static class KeyMappings
	{
		// See http://unity3d.com/support/documentation/ScriptReference/MenuItem.html for shortcut format

		public const string ToggleMode = "Space";
		public const string ToggleModeBack = "#Space";

		// Main Toolbar
		public const string TogglePosSnapping = "/";
		public const string DecreasePosSnapping = ",";
		public const string IncreasePosSnapping = ".";

		public const string ToggleAngSnapping = "#/";
		public const string DecreaseAngSnapping = "#,";
		public const string IncreaseAngSnapping = "#.";

		public const string ToggleBrushesHidden = "h";

		// General
		public const string ChangeBrushToAdditive = "a";
		public const string ChangeBrushToAdditive2 = "KeypadPlus";

		public const string ChangeBrushToSubtractive = "s";
		public const string ChangeBrushToSubtractive2 = "KeypadMinus";

		public const string Group = "g";
		public const string Ungroup = "#g";

		// Clip Tool
		public const string ApplyClip = "Return";
		public const string ApplySplit = "#Return";
		public const string InsertEdgeLoop = "l";
		public const string FlipPlane = "r";

		// Resize Tool
		public const KeyCode CancelMove = KeyCode.Escape;

		// Surface Tool
		public const string CopyMaterial = "c";

		// Used in UtilityShortcuts.cs with MenuItem attribute
		public const string Rebuild = "%#r";


		/// <summary>
		/// Helper method to determine if two keyboard events match
		/// </summary>
		public static bool EventsMatch(Event event1, Event event2, bool ignoreShift = false)
		{
			EventModifiers modifiers1 = event1.modifiers;
			EventModifiers modifiers2 = event2.modifiers;

			// Ignore capslock from either modifier
			modifiers1 &= (~EventModifiers.CapsLock);
			modifiers2 &= (~EventModifiers.CapsLock);

			if(ignoreShift)
			{
				// Ignore shift from either modifier
				modifiers1 &= (~EventModifiers.Shift);
				modifiers2 &= (~EventModifiers.Shift);
			}

			// If key code and modifier match
			if(event1.keyCode == event2.keyCode
				&& (modifiers1 == modifiers2))
			{
				return true;
			}

			return false;
		}
	}
}
#endif