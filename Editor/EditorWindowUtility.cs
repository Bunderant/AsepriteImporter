using UnityEditor;
using UnityEngine;

namespace Miscreant.Aseprite.Editor
{
	using Editor = UnityEditor.Editor;

	internal static class EditorWindowUtility
	{
		public static void ShowInspectorNotification(GUIContent content, double fadeoutWait)
		{
			EditorWindow inspector = EditorWindow.GetWindow(
				typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow")
			);

			inspector.ShowNotification(content, fadeoutWait);
		}
	}
}
