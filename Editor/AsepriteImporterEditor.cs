using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Miscreant.Aseprite.Editor
{
	[CustomEditor(typeof(AsepriteImporter))]
	[CanEditMultipleObjects]
	public class AsepriteImporterEditor : ScriptedImporterEditor
	{
		protected override bool useAssetDrawPreview
		{
			get { return false; }
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var importer = (AsepriteImporter)target;

			var clipSettingsProp = serializedObject.FindProperty("clipSettings");

			EditorGUI.BeginChangeCheck();

			bool shouldGenerateClips = EditorGUILayout.Toggle("Generates Animation Clips", importer.generateAnimationClips);

			EditorGUI.BeginDisabledGroup(!shouldGenerateClips);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(clipSettingsProp);
			EditorGUI.indentLevel--;
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
	}
}