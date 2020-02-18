using UnityEngine;

namespace Miscreant.Aseprite
{
	[CreateAssetMenu(menuName = nameof(Miscreant) + "/" + nameof(Miscreant.Aseprite) + "/" + nameof(Settings))]
	public sealed class Settings : ScriptableObject
	{
		public const string PATH = "Packages/com.miscreant.aseprite-importer/Settings/GlobalSettings.asset";

		[SerializeField]
		private string _asepritePath = null;
		public string asepritePath { get { return _asepritePath; } }
	}
}