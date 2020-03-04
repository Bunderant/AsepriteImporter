using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Experimental.AssetImporters;

namespace Miscreant.Aseprite.Editor
{
	[CustomEditor(typeof(AsepriteImporter))]
	[CanEditMultipleObjects]
	public sealed class AsepriteImporterEditor : ScriptedImporterEditor
	{
		private ReorderableList _generatedClipList;

		private static bool _bIsClipSettingsOpen;

		protected override bool useAssetDrawPreview
		{
			get { return false; }
		}

		public override void OnEnable()
		{
			base.OnEnable();

			InitializeGeneratedClipList();

			MergedClip.OnInvalidClipAssigned.AddListener(ShowInvalidClipWarning);
		}

		public override void OnDisable()
		{
			base.OnDisable();

			MergedClip.OnInvalidClipAssigned.RemoveListener(ShowInvalidClipWarning);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var importer = (AsepriteImporter)target;

			var clipSettingsProp = serializedObject.FindProperty("clipSettings");

			EditorGUI.BeginChangeCheck();

			bool shouldGenerateClips = EditorGUILayout.Toggle("Generates Animation Clips", importer.generateAnimationClips);

			EditorGUI.BeginDisabledGroup(!shouldGenerateClips);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space(12, false); // Gross code to force the foldout to be indented within the box bounds. 
			_bIsClipSettingsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_bIsClipSettingsOpen && shouldGenerateClips, "Clip Settings");
			EditorGUILayout.Space(2, false); 
			EditorGUILayout.EndHorizontal();

			if (_bIsClipSettingsOpen)
			{
				EditorGUILayout.PropertyField(clipSettingsProp);
				_generatedClipList.DoLayoutList();
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
			EditorGUILayout.EndVertical();
			EditorGUI.EndDisabledGroup();

			if (EditorGUI.EndChangeCheck())
			{
				var generateAnimationClipsProp = serializedObject.FindProperty("generateAnimationClips");
				generateAnimationClipsProp.boolValue = shouldGenerateClips;
			}

			serializedObject.ApplyModifiedProperties();
			base.ApplyRevertGUI();
		}

		public override bool HasPreviewGUI()
		{
			return ((AsepriteImporter)target).packedSpriteTexture != null;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			var importer = (AsepriteImporter)target;

			Texture2D texture = importer.packedSpriteTexture;

			GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
			EditorGUI.DropShadowLabel(
				r,
				$"Packed Size: {texture.width}x{texture.height} - Animation Clips: {importer.clipCount}"
			);
		}

		private void InitializeGeneratedClipList()
		{
			var generatedClipsProp = serializedObject.FindProperty("generatedClips");

			_generatedClipList = new ReorderableList(
				generatedClipsProp.serializedObject,
				generatedClipsProp,
				false,
				true,
				false,
				false
			);

			_generatedClipList.elementHeightCallback = (
				index => { return EditorGUI.GetPropertyHeight(_generatedClipList.serializedProperty.GetArrayElementAtIndex(index)); }
			);

			_generatedClipList.drawHeaderCallback = (
				rect => { EditorGUI.LabelField(rect, "Generated Clips", EditorStyles.boldLabel); }
			);

			_generatedClipList.drawElementCallback = (
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					var element = _generatedClipList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(rect, element, GUIContent.none);
				}
			);
		}

		private void ShowInvalidClipWarning(AnimationClip clip)
		{
			var message = new GUIContent("Merge target clip cannot be an Aseprite subasset.");
			double fadeoutWait = 2f;

			// If the object picker isn't open, show the notification in the inspector window. 
			if (EditorGUIUtility.GetObjectPickerObject() == null)
				EditorWindowUtility.ShowInspectorNotification(message, fadeoutWait);
			else
				EditorWindow.focusedWindow.ShowNotification(message, fadeoutWait);
		}
	}
}