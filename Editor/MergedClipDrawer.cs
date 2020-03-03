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
				var clip = (AnimationClip)clipProp.objectReferenceValue;
				if (clip != null && AssetDatabase.IsSubAsset(clip))
				{
					string mainAssetPath = AssetDatabase.GetAssetPath(clipProp.objectReferenceValue);
					var clipImporter = AssetImporter.GetAtPath(mainAssetPath);
					if (clipImporter is AsepriteImporter)
					{
						Debug.LogWarning(
							$"Cannot merge into clips that are Aseprite subassets: {clip.name}",
							clipProp.objectReferenceValue
						);

						clipProp.objectReferenceValue = null;
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