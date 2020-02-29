using UnityEngine;

namespace Miscreant.Aseprite.Editor
{
	public sealed class AsepriteAsset : ScriptableObject
	{
		[SerializeField]
		private Texture2D _icon = null;
		public Texture2D icon { get { return _icon; } }	
	}
}