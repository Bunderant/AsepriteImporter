using UnityEngine;

namespace Miscreant.Aseprite.Editor
{
	public sealed class AsepriteAsset : ScriptableObject
	{
		[SerializeField]
		private Texture2D _icon = null;
		
		[SerializeField]
		private Texture2D _iconWithClips = null;

		public Texture2D GetIcon(bool importerGeneratesAnimationClips)
		{
			return importerGeneratesAnimationClips ? _iconWithClips : _icon;
		}
	}
}