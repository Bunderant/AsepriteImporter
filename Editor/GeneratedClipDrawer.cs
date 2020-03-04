using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CustomPropertyDrawer(typeof(GeneratedClip))]
	public sealed class GeneratedClipDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				clipProp
			);

			EditorGUI.EndDisabledGroup();

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y + EditorGUIUtility.singleLineHeight,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				rendererPathProp
			);

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;
		}
	}
}