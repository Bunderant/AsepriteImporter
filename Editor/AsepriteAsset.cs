using UnityEngine;

namespace Miscreant.Aseprite.Editor
{
	public sealed class AsepriteAsset : ScriptableObject
	{
		public Texture2D packedSpriteTexture;
		public int clipCount;
	}
}