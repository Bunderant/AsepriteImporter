using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct ClipSettings
	{
		public const string DEFAULT_RENDERER_PATH = "";

		[Tooltip("Path from the root Animator to the SpriteRenderer's GameObject.")]
		public string spriteRendererPath;

		[Tooltip("(Frame Rate) If set too low, could lead to rounding errors for keyframe timing if driven by Aseprite.")]
		public int sampleRate;

		public static ClipSettings Default
		{
			get 
			{
				return new ClipSettings {
					spriteRendererPath = DEFAULT_RENDERER_PATH,
					sampleRate = 60
				};
			}
		}
	}
}
