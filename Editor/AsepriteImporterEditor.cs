using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Miscreant.Aseprite.Editor
{
	[CustomEditor(typeof(AsepriteImporter))]
	public class AsepriteImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			base.ApplyRevertGUI();
		}
	}
}