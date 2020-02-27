using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Miscreant.Aseprite.Editor
{
	[CustomEditor(typeof(AsepriteImporter))]
	[CanEditMultipleObjects]
	public class AsepriteImporterEditor : ScriptedImporterEditor
	{
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
	}
}