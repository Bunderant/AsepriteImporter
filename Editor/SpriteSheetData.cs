using System;
using UnityEngine;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public sealed class SpriteSheetData : ISerializationCallbackReceiver
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

			public UnityEngine.Rect GetUnityTextureRect(int textureHeight)
			{
				int xPad = (frame.w - spriteSourceSize.w) / 2;
				int yPad = (frame.h - spriteSourceSize.h) / 2;

				return new UnityEngine.Rect {
					x = frame.x + xPad,
					y = textureHeight - (frame.y + frame.h - yPad),
					width = spriteSourceSize.w,
					height = spriteSourceSize.h
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
		}

		[Serializable]
		public struct FrameTag
		{
			public string name;
			public int from;
			public int to;
			public string direction;

			/// <summary>
			/// Checks whether a name change is allowed from an existing frame to an updated one.
			/// </summary>
			/// <param name="existingTag">The existing tag before the reimport.</param>
			/// <returns>true if the name has changed, false otherwise. </returns>
			public static bool IsNameChangeAllowed(FrameTag existing, FrameTag updated)
			{
				return (
					existing.from == updated.from &&
					existing.to == updated.to
				);
			}
		}

		public Frame[] frames;
		public MetaData meta;

		public void OnBeforeSerialize()
		{
			// Do nothin' but keep the ISerializationCallbackReceiver interface happy.
		}

		public void OnAfterDeserialize()
		{
			Array.Sort(
				meta.frameTags,
				new Comparison<FrameTag>((a, b) => { 
					int value = a.name.CompareTo(b.name);
					if (value == 0)
					{
						throw new System.Exception("All frame tags must have unique names.");
					}
					return value;
				})
			);
		}
	}
}