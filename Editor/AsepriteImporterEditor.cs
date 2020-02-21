using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Miscreant.Aseprite.Editor
{
	[CustomEditor(typeof(AsepriteImporter))]
	public class AsepriteImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			SerializedProperty serializedAtlasDirectory = serializedObject.FindProperty("_targetAtlasDirectory");
			if (serializedAtlasDirectory.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(
					$"Target atlas directory is not set. Will create atlas at \"Assets/{AsepriteImporter.DEFAULT_ATLAS_FOLDER}\"", MessageType.Warning
				);
			}

			SerializedProperty serializedAnimDirectory = serializedObject.FindProperty("_targetAnimationDirectory");
			if (serializedAnimDirectory.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox(
					$"Target animation directory is not set. Will create atlas at \"Assets/{AsepriteImporter.DEFAULT_ANIMATION_FOLDER}\"", MessageType.Warning
				);
			}

			base.ApplyRevertGUI();
		}
	}
}