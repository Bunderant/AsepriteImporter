using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CustomPropertyDrawer(typeof(ClipSettings))]
	public class ClipSettingsDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			var rendererPathProp = property.FindPropertyRelative("spriteRendererPath");

			EditorGUI.BeginProperty(pos, label, property);

			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				rendererPathProp
			);

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
		}
	}
}