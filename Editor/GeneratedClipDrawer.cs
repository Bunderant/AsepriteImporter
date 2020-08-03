using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Miscreant.Aseprite.Editor
{
	[CustomPropertyDrawer(typeof(GeneratedClip))]
	public sealed class GeneratedClipDrawer : PropertyDrawer
	{
		private bool _isInitialized;
		private Dictionary<string, bool> _foldoutLookup;

		private void Initialize(SerializedProperty serializedProperty)
		{
			_foldoutLookup = new Dictionary<string, bool>();
			_isInitialized = true;
		}

		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			if (!_isInitialized)
			{
				Initialize(property);
			}

			var nameProp = property.FindPropertyRelative("name");
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);
			EditorGUI.BeginChangeCheck();

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;

			_foldoutLookup.TryGetValue(nameProp.propertyPath, out bool isFoldoutOpen);
			_foldoutLookup[nameProp.propertyPath] = EditorGUI.Foldout(
				new Rect(
					pos.x + 10, // Accounts for foldout arrow indentation.
					pos.y,
					pos.width - 20, // Accounts for foldout arrow indentation.
					EditorGUIUtility.singleLineHeight
				),
				isFoldoutOpen,
				nameProp.stringValue,
				true,
				foldoutStyle
			);

			pos.y += EditorGUIUtility.standardVerticalSpacing;
			pos.y += EditorGUIUtility.singleLineHeight;

			if (isFoldoutOpen)
			{
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
			if (!_isInitialized)
			{
				Initialize(property);
			}

			var nameProp = property.FindPropertyRelative("name");
			_foldoutLookup.TryGetValue(nameProp.propertyPath, out bool isFoldoutOpen);
			if (!isFoldoutOpen)
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