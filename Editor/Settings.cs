using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CreateAssetMenu(menuName = nameof(Miscreant) + "/" + nameof(Miscreant.Aseprite) + "/" + nameof(Settings))]
	public sealed class Settings : ScriptableObject
	{
		public const string PATH = "Packages/com.miscreant.aseprite-importer/Settings/GlobalSettings.asset";

		[SerializeField]
		private string _asepritePath = null;
		public string asepritePath { get { return _asepritePath; } }

		[SerializeField]
		private Texture2D _icon = null;
		
		[SerializeField]
		private Texture2D _iconWithClips = null;

		public Texture2D GetIcon(bool importerGeneratesAnimationClips)
		{
			return importerGeneratesAnimationClips ? _iconWithClips : _icon;
		}

		public static Settings LoadSettingsAsset()
		{
			return AssetDatabase.LoadAssetAtPath<Settings>(PATH);
		}
	}
}