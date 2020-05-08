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

			// Show placeholer text if override path isn't specified
			if (string.IsNullOrEmpty(rendererPathProp.stringValue))
			{
				var placeholderTextStyle = new GUIStyle(EditorStyles.label);
				placeholderTextStyle.fontStyle = FontStyle.Italic;

				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.TextArea(
					new Rect(
						pos.x + EditorGUIUtility.labelWidth + 5,
						pos.y,
						pos.width,
						EditorGUIUtility.singleLineHeight
					), 
					"Root",
					placeholderTextStyle
				);
				EditorGUI.EndDisabledGroup();
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
		}
	}
}