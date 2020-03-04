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

		private ReorderableList _mergeTargetsList;

		private void Initialize(SerializedProperty serializedProperty)
		{
			InitializeMergeTargetsList(serializedProperty);
			_foldoutLookup = new Dictionary<string, bool>();

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

			var nameProp = property.FindPropertyRelative("name");
			var createModeProp = property.FindPropertyRelative("createMode");
			var clipProp = property.FindPropertyRelative("clip");
			var rendererPathProp = property.FindPropertyRelative("rendererPathOverride");

			EditorGUI.BeginProperty(pos, label, property);

			pos.y += EditorGUIUtility.standardVerticalSpacing;

			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;

			_foldoutLookup.TryGetValue(nameProp.stringValue, out bool isFoldoutOpen);
			_foldoutLookup[nameProp.stringValue] = EditorGUI.Foldout(
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
					_mergeTargetsList.DoList(
						new Rect(
							pos.x,
							pos.y,
							pos.width,
							_mergeTargetsList.GetHeight()
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
			_foldoutLookup.TryGetValue(nameProp.stringValue, out bool isFoldoutOpen);
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