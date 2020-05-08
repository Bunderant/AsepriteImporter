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

			EditorGUILayout.Space();
			bool shouldGenerateClips = EditorGUILayout.Toggle("Generates Animation Clips", importer.generateAnimationClips);

			// Horizontal line:
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

			EditorGUI.BeginDisabledGroup(!shouldGenerateClips);

			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;
			_bIsClipSettingsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(
				_bIsClipSettingsOpen && shouldGenerateClips,
				"Clip Settings",
				foldoutStyle
			);

			if (_bIsClipSettingsOpen)
			{
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(clipSettingsProp);
				_generatedClipList.DoLayoutList();	
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
			EditorGUI.EndDisabledGroup();

			// Horizontal line:
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

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
			return ((AsepriteImporter)target).Atlas != null;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			var importer = (AsepriteImporter)target;

			Texture2D texture = importer.Atlas;

			GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
			EditorGUI.DropShadowLabel(
				r,
				$"Packed Size: {texture.width}x{texture.height} - Animation Clips: {importer.clipCount}"
			);
		}

		private void InitializeGeneratedClipList()
		{
			var generatedClipsProp = serializedObject.FindProperty("m_generatedClips");

			_generatedClipList = new ReorderableList(
				generatedClipsProp.serializedObject,
				generatedClipsProp,
				false,
				true,
				false,
				false
			);

			_generatedClipList.showDefaultBackground = false;
			_generatedClipList.headerHeight = 0f;
			_generatedClipList.footerHeight = 0f; 

			_generatedClipList.elementHeightCallback = (
				index => { return EditorGUI.GetPropertyHeight(_generatedClipList.serializedProperty.GetArrayElementAtIndex(index)); }
			);

			_generatedClipList.drawElementCallback = (
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					var element = _generatedClipList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(rect, element, GUIContent.none);
				}
			);

			_generatedClipList.drawElementBackgroundCallback = (
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					// Zebra striping for clip list elements.
					if (index % 2 != 0)
					{
						EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.1f) : new Color(0, 0, 0, 0.1f));
					}
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