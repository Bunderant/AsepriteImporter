using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public sealed class SpriteSheetData
	{
		[Serializable]
		public struct Frame
		{
			public string filename;
			public Rect frame;
			public bool rotated;
			public bool trimmed;
			public Rect spriteSourceSize;
			public Rect sourceSize;
			public int duration;

			public Rect GetUnityTextureRect(int textureHeight)
			{
				int xPad = (frame.w - spriteSourceSize.w) / 2;
				int yPad = (frame.h - spriteSourceSize.h) / 2;

				return new Rect {
					x = frame.x + xPad,
					y = textureHeight - (frame.y + frame.h - yPad),
					w = spriteSourceSize.w,
					h = spriteSourceSize.h
				};
			}
		}

		[Serializable]
		public struct Rect
		{
			public int x;
			public int y;
			public int w;
			public int h;

			public int Left 	{ get { return x; } }
			public int Right 	{ get { return x + w - 1; } }
			public int Bottom 	{ get { return y; } }
			public int Top 		{ get { return y + h - 1; } }

			public override string ToString()
			{
				return $"x: {x}, y: {y}, w: {w}, h: {h}";
			}
		}

		[Serializable]
		public struct MetaData
		{
			public string app;
			public string version;
			public string image;
			public string format;
			public Rect size;
			public string scale;
			public FrameTag[] frameTags;
			public string animationDirectoryAssetPath;
		}

		[Serializable]
		public struct FrameTag
		{
			public string name;
			public int from;
			public int to;
			public string direction;
		}

		public Frame[] frames;
		public MetaData meta;
	}
}