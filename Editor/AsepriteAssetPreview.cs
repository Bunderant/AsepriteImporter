using UnityEngine;
using UnityEditor;

namespace Miscreant.Aseprite.Editor
{
	[CustomPreview(typeof(AsepriteAsset))]
	public class AsepriteAssetPreview : ObjectPreview
	{
		public override bool HasPreviewGUI()
		{
			return ((AsepriteAsset)target).packedSpriteTexture != null;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			var mainAsset = (AsepriteAsset)target;

			Texture2D texture = mainAsset.packedSpriteTexture;

			GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
			EditorGUI.DropShadowLabel(
				r, 
				$"Packed Size: {texture.width}x{texture.height} - Animation Clips: {mainAsset.clipCount}"
			);
		}
	}
}