using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct GeneratedClip
	{
		public enum CreateMode
		{
			SubassetOnly,
			MergeIntoExistingOnly,
			SubassetAndMergeExisting
		}

		[Tooltip("Determines whether new clips are created, keyframes are merged into existing clips, or both.")]
		public CreateMode createMode;

		[Tooltip("Path from the root Animator to the SpriteRenderer's GameObject.")]
		public string rendererPathOverride;

		[Tooltip("The AnimationClip subasset (read only).")]
		public AnimationClip clip;

		[Tooltip("List of clips to merge the generated SpriteRenderer keyframes into.")]
		public MergedClip[] mergeTargetClips;

		public static GeneratedClip Default
		{
			get
			{
				return new GeneratedClip()
				{
					createMode = CreateMode.SubassetOnly,
					rendererPathOverride = string.Empty,
					clip = null,
					mergeTargetClips = new MergedClip[] { MergedClip.Default } // Include one so the user doesn't have to add it manually 
				};
			}
		}
	}
}