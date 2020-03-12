using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using System.Collections.Generic;

namespace Miscreant.Aseprite.Editor
{
	using CreateMode = GeneratedClip.CreateMode;

	[CustomPropertyDrawer(typeof(GeneratedClip))]
	public sealed class GeneratedClipDrawer : PropertyDrawer
	{
		private bool _isInitialized;
		private Dictionary<string, bool> _foldoutLookup;
		private Dictionary<string, ReorderableList> _mergeTargetListLookup;

		private void Initialize(SerializedProperty serializedProperty)
		{
			InitializeMergeTargetLists(serializedProperty);
			_foldoutLookup = new Dictionary<string, bool>();

			_isInitialized = true;
		}

		private void InitializeMergeTargetLists(SerializedProperty serializedProperty)
		{
			var generatedClipsProp = serializedProperty.serializedObject.FindProperty("generatedClips");
			int clipCount = generatedClipsProp.arraySize;

			_mergeTargetListLookup = new Dictionary<string, ReorderableList>(clipCount);

			for (int i = 0; i < clipCount; i++)
			{
				var generatedClipProp = generatedClipsProp.GetArrayElementAtIndex(i);
				var mergeTargetsProp = generatedClipProp.FindPropertyRelative("mergeTargetClips");

				var mergeTargetsList = new ReorderableList(
					mergeTargetsProp.serializedObject,
					mergeTargetsProp,
					true,
					true,
					true,
					true
				); 

				mergeTargetsList.elementHeightCallback = (
					index => { return EditorGUI.GetPropertyHeight(mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index)); }
				);

				mergeTargetsList.drawHeaderCallback = (
					rect => { EditorGUI.LabelField(rect, "Merge Target Clips", EditorStyles.boldLabel); }
				);

				mergeTargetsList.drawElementCallback = (
					(Rect rect, int index, bool isActive, bool isFocused) =>
					{
						var element = mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index);
						EditorGUI.PropertyField(rect, element, GUIContent.none);
					}
				);

				var nameProp = generatedClipProp.FindPropertyRelative("name");
				_mergeTargetListLookup[nameProp.propertyPath] = mergeTargetsList;
			}
		}

		public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
		{
			if (!_isInitialized)
			{
				Initialize(property);
			}

			var nameProp = property.FindPropertyRelative("name");
			var createModeProp = property.FindPropertyRelative("createMode");
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
					var mergeList = _mergeTargetListLookup[nameProp.propertyPath];
					mergeList.DoList(
						new Rect(
							pos.x,
							pos.y,
							pos.width,
							mergeList.GetHeight()
						)
					);
				}
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

			var createModeProp = property.FindPropertyRelative("createMode");
			var createMode = (CreateMode)createModeProp.enumValueIndex;

			switch (createMode)
			{
				case CreateMode.SubassetOnly:
					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					break;
				case CreateMode.MergeIntoExistingOnly:
					height += _mergeTargetListLookup[nameProp.propertyPath].GetHeight();
					break;
				case CreateMode.SubassetAndMergeExisting:
					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					height += _mergeTargetListLookup[nameProp.propertyPath].GetHeight();
					break;
			}

			// Add a little bit more pading to the bottom of the element
			height += 5;

			return height;
		}
	}
}