using UnityEngine;
using UnityEditor;
using Miscreant.Aseprite.Editor;
using System.IO;
using Unity.Collections;

using Rect = Miscreant.Aseprite.Editor.SpriteSheetData.Rect;

/// <summary>
/// Automatically extrudes packed sprites in any texture/json data pairs with "_extruded" in their filenames
/// </summary>
class ExtrudedSpritePostprocessor : AssetPostprocessor // TODO: Miscreant: Move this to a precompiled .dll to prevent modification once it's stable
{
	const string SUFFIX = "_extruded";

	void OnPostprocessTexture(Texture2D texture)
	{
		Debug.Log(assetPath);

		if (assetPath.Contains(SUFFIX))
		{
			// Check for associated JSON data, must be in the same folder.
			string dataAssetPath = assetPath.Substring(0, assetPath.LastIndexOf(SUFFIX)) + $"{SUFFIX}.json";

			// Load the sheet data via File IO since there's a chance Unity may not have imported it as an asset yet
			string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
			string dataAbsolutePath = projectPath + "/" + dataAssetPath;
			var sheetData = JsonUtility.FromJson<SpriteSheetData>(File.ReadAllText(dataAbsolutePath));

			NativeArray<Color32> pixelData = texture.GetRawTextureData<Color32>();

			foreach (SpriteSheetData.Frame frameData in sheetData.frames)
			{
				Rect spriteRect = frameData.GetUnityTextureRect(texture.height);

				ExtrudeCorners(texture, spriteRect);

				ExtrudeVerticalEdge(
					spriteRect.x,
					spriteRect.y,
					spriteRect.y + spriteRect.h,
					-1);

				ExtrudeVerticalEdge(
					spriteRect.x + spriteRect.w - 1,
					spriteRect.y,
					spriteRect.y + spriteRect.h,
					1);

				ExtrudeHorizontalEdge(
					spriteRect.y,
					spriteRect.x,
					spriteRect.x + spriteRect.w,
					-1);

				ExtrudeHorizontalEdge(
					spriteRect.y + spriteRect.h - 1,
					spriteRect.x,
					spriteRect.x + spriteRect.w,
					1);
			}

			void ExtrudeVerticalEdge(int x, int yMin, int yMax, int direction)
			{
				for (int y = yMin; y < yMax; y++)
				{
					int fromIndex = x + y * texture.width;
					int toIndex = x + direction + y * texture.width;

					pixelData[toIndex] = pixelData[fromIndex];
				}
			}

			void ExtrudeHorizontalEdge(int y, int xMin, int xMax, int direction)
			{
				for (int x = xMin; x < xMax; x++)
				{
					int fromIndex = x + y * texture.width;
					int toIndex = x + (y + direction) * texture.width;

					pixelData[toIndex] = pixelData[fromIndex];
				}
			}

			texture.SetPixelData<Color32>(pixelData, 0);
		}
	}

	private static void ExtrudeCorners(Texture2D texture, Rect spriteRect)
	{
		// TL
		texture.SetPixel(
			spriteRect.Left - 1,
			spriteRect.Top + 1,
			texture.GetPixel(spriteRect.Left, spriteRect.Top)
		);

		// TR
		texture.SetPixel(
			spriteRect.Right + 1,
			spriteRect.Top + 1,
			texture.GetPixel(spriteRect.Right, spriteRect.Top)
		);

		// BR
		texture.SetPixel(
			spriteRect.Right + 1,
			spriteRect.Bottom - 1,
			texture.GetPixel(spriteRect.Right, spriteRect.Bottom)
		);

		// BL
		texture.SetPixel(
			spriteRect.Left - 1,
			spriteRect.Bottom - 1,
			texture.GetPixel(spriteRect.Left, spriteRect.Bottom)
		);
	}
}