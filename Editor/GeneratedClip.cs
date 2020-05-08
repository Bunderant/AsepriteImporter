using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct GeneratedClip
	{
		[Tooltip("Name of the associated Aseprite tag.")]
		public string name;

		[Tooltip("Path from the root Animator to the SpriteRenderer's GameObject.")]
		public string rendererPathOverride;

		[Tooltip("The AnimationClip subasset (read only).")]
		public AnimationClip clip;

		public static GeneratedClip Create(string name, AnimationClip generatedClip)
		{
			return new GeneratedClip()
			{
				name = name,
				rendererPathOverride = string.Empty,
				clip = generatedClip
			};
		}
	}
}