using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CustomPropertyDrawer(typeof(GeneratedClip))]
	public sealed class GeneratedClipDrawer : PropertyDrawer
	{
		private string _activeClipPath;

		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			var nameProp = property.FindPropertyRelative("name");
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);
			EditorGUI.BeginChangeCheck();

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;

			bool foldoutOpen = EditorGUI.Foldout(
				new Rect(
					pos.x + 10, // Accounts for foldout arrow indentation.
					pos.y,
					pos.width - 20, // Accounts for foldout arrow indentation.
					EditorGUIUtility.singleLineHeight
				),
				_activeClipPath == nameProp.propertyPath,
				nameProp.stringValue,
				true,
				foldoutStyle
			);

			pos.y += EditorGUIUtility.standardVerticalSpacing;
			pos.y += EditorGUIUtility.singleLineHeight;

			if (!foldoutOpen && _activeClipPath == nameProp.propertyPath)
			{
				_activeClipPath = null;
			}
			else if (foldoutOpen)
			{
				_activeClipPath = nameProp.propertyPath;

				// Indent everything within the foldout scope.
				pos.x += 10;
				pos.xMax -= 10;

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
						"None",
						placeholderTextStyle
					);
					EditorGUI.EndDisabledGroup();
				}

				pos.y += EditorGUIUtility.standardVerticalSpacing;
				pos.y += EditorGUIUtility.singleLineHeight;

				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.PropertyField(
					new Rect(
						pos.x,
						pos.y,
						pos.width,
						EditorGUIUtility.singleLineHeight
					),
					clipProp,
					new GUIContent("Clip Subasset")
				);

				EditorGUI.EndDisabledGroup();

				pos.y += EditorGUIUtility.standardVerticalSpacing;
				pos.y += EditorGUIUtility.singleLineHeight;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var nameProp = property.FindPropertyRelative("name");
			if (nameProp.propertyPath != _activeClipPath)
			{
				return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
			}

			float height = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 4;

			// Add a little bit more pading to the bottom of the element
			height += 5;

			return height;
		}
	}
}