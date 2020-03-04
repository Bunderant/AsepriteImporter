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
		private ReorderableList _mergeTargetsList;

		protected override bool useAssetDrawPreview
		{
			get { return false; }
		}

		public override void OnEnable()
		{
			base.OnEnable();

			var mergeTargetsProp = serializedObject.FindProperty("mergeTargetClips");

			_mergeTargetsList = new ReorderableList(
				serializedObject,
				mergeTargetsProp,
				true,
				true,
				true,
				true
			);

			_mergeTargetsList.elementHeightCallback = (
				index =>
				{
					return EditorGUI.GetPropertyHeight(_mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index));
				}
			);

			_mergeTargetsList.drawHeaderCallback = (
				rect =>
				{
					EditorGUI.LabelField(rect, "Merge Target Clips", EditorStyles.boldLabel);
				}
			);

			_mergeTargetsList.drawElementCallback = (
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					var element = _mergeTargetsList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(
						rect,
						element,
						GUIContent.none
					);
				}
			);

			_mergeTargetsList.onChangedCallback = (
				rect =>
				{
					Debug.LogWarning("TODO: Miscreant: Make sure selected clip is not a subasset of this object (crashes editor on import).");
				}
			);

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
			EditorGUILayout.PropertyField(clipSettingsProp, new GUIContent("Default Clip Settings"));
			EditorGUI.EndDisabledGroup();
			
			if (shouldGenerateClips && importer.clipSettings.createMode != ClipSettings.CreateMode.CreateNewAsset)
			{
				_mergeTargetsList.DoLayoutList();
			}

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