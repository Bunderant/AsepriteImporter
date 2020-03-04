using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct GeneratedClip
	{
		[Tooltip("Determines whether new clips are created, keyframes are merged into existing clips, or both.")]
		public ClipSettings.CreateMode createMode;

		public string rendererPathOverride;
		public AnimationClip clip;

		public MergedClip[] mergeTargetClips;
	}
}