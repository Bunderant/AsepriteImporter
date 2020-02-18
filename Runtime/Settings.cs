using UnityEngine;

namespace Miscreant.AsepriteImporter
{
	[CreateAssetMenu(menuName = nameof(Miscreant) + "/" + nameof(Miscreant.AsepriteImporter) + "/" + nameof(Settings))]
	public sealed class Settings : ScriptableObject
	{
		public const string PATH = "Packages/com.miscreant.aseprite-importer/Settings/GlobalSettings.asset";

		[SerializeField]
		private string _asepritePath;
		public string asepritePath { get { return _asepritePath; } }
	}
}