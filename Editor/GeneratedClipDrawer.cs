using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	using CreateMode = GeneratedClip.CreateMode;

	[CustomPropertyDrawer(typeof(GeneratedClip))]
	public sealed class GeneratedClipDrawer : PropertyDrawer
	{
		private bool _isInitialized;

		private ReorderableList _mergeTargetsList;

		private void Initialize(SerializedProperty serializedProperty)
		{
			InitializeMergeTargetsList(serializedProperty);

			_isInitialized = true;
		}

		private void InitializeMergeTargetsList(SerializedProperty serializedProperty)
		{
			var mergeTargetsProp = serializedProperty.FindPropertyRelative("mergeTargetClips");

			_mergeTargetsList = new ReorderableList(
				mergeTargetsProp.serializedObject,
				mergeTargetsProp,
				true,
				true,
				true,
				true
			);

			_mergeTargetsList.elementHeightCallback = (
				index => { return EditorGUI.GetPropertyHeight(_mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index)); }
			);

			_mergeTargetsList.drawHeaderCallback = (
				rect => { EditorGUI.LabelField(rect, "Merge Target Clips", EditorStyles.boldLabel); }
			);

			_mergeTargetsList.drawElementCallback = (
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					var element = _mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(rect, element, GUIContent.none);
				}
			);
		}

		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			if (!_isInitialized)
			{
				Initialize(property);
			}

			var createModeProp = property.FindPropertyRelative("createMode");
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				createModeProp
			);

			pos.y += EditorGUIUtility.standardVerticalSpacing;
			pos.y += EditorGUIUtility.singleLineHeight;

			if (createModeProp.enumValueIndex != (int)CreateMode.MergeIntoExistingOnly)
			{
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

			EditorGUI.PropertyField(
				new Rect(
					pos.x,
					pos.y,
					pos.width,
					EditorGUIUtility.singleLineHeight
				),
				rendererPathProp
			);

			pos.y += EditorGUIUtility.standardVerticalSpacing;
			pos.y += EditorGUIUtility.singleLineHeight;

			if (createModeProp.enumValueIndex != (int)CreateMode.SubassetOnly)
			{
				_mergeTargetsList.DoList(
					new Rect(
						pos.x,
						pos.y,
						pos.width,
						_mergeTargetsList.GetHeight()
					)
				);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!_isInitialized)
			{
				Initialize(property);
			}

			float height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;

			var createModeProp = property.FindPropertyRelative("createMode");
			var createMode = (CreateMode)createModeProp.enumValueIndex;

			switch (createMode)
			{
				case CreateMode.SubassetOnly:
					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					break;
				case CreateMode.MergeIntoExistingOnly:
					height += _mergeTargetsList.GetHeight();
					break;
				case CreateMode.SubassetAndMergeExisting:
					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					height += _mergeTargetsList.GetHeight();
					break;
			}

			return height;
		}
	}
}