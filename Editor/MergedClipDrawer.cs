using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CustomPropertyDrawer(typeof(MergedClip))]
	public sealed class MergedClipDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.BeginChangeCheck();

			// Cache the previous clip to be reverted in case an invalid assignment is attempted.
			var previousClip = (AnimationClip)clipProp.objectReferenceValue;

			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				clipProp
			);

			if (EditorGUI.EndChangeCheck())
			{
				// Revert the clip assignment if it was an invalid selection.
				var assignedClip = (AnimationClip)clipProp.objectReferenceValue;
				if (assignedClip != null && AssetDatabase.IsSubAsset(assignedClip))
				{
					string mainAssetPath = AssetDatabase.GetAssetPath(assignedClip);
					var clipImporter = AssetImporter.GetAtPath(mainAssetPath);
					if (clipImporter is AsepriteImporter)
					{
						MergedClip.OnInvalidClipAssigned?.Invoke(assignedClip);
						clipProp.objectReferenceValue = previousClip;
					}
				}
			}

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