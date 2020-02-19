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
		}

		[Serializable]
		public struct Rect
		{
			public int x;
			public int y;
			public int w;
			public int h;
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
		}

		public Frame[] frames;
		public MetaData meta;
	}
}