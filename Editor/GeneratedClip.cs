using UnityEngine;
using UnityEditor;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct GeneratedClip
	{
		public enum CreateMode
		{
			Subasset,
			Merge,
			SubassetAndMerge
		}

		[Tooltip("Name of the associated Aseprite tag.")]
		public string name;

		[Tooltip("Determines whether new clips are created, keyframes are merged into existing clips, or both.")]
		public CreateMode createMode;

		[Tooltip("Path from the root Animator to the SpriteRenderer's GameObject.")]
		public string rendererPathOverride;

		[Tooltip("The AnimationClip subasset (read only).")]
		public AnimationClip clip;

		[Tooltip("List of clips to merge the generated SpriteRenderer keyframes into.")]
		public MergedClip[] mergeTargetClips;

		public static GeneratedClip Create(string name, AnimationClip generatedClip)
		{
			return new GeneratedClip()
			{
				name = name,
				rendererPathOverride = string.Empty,
				clip = generatedClip,
				mergeTargetClips = new MergedClip[] { MergedClip.Default } // Include one so the user doesn't have to add it manually 
			};
		}

		public void TryMerge()
		{
			if (createMode == CreateMode.Subasset)
			{
				return;
			}

			// Get the sprite renderer curves from the base clip
			EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

			// The base clip's binding will always be the only binding for the generated curve
			ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);

			string rendererPath = !string.IsNullOrEmpty(rendererPathOverride) ?
				rendererPathOverride :
				bindings[0].path;

			foreach (MergedClip mergeTarget in mergeTargetClips)
			{
				string currentRendererPath = !string.IsNullOrEmpty(mergeTarget.rendererPathOverride) ?
					mergeTarget.rendererPathOverride :
					rendererPath;

				mergeTarget.SetKeyframes(keyframes, currentRendererPath);
			}
		}
	}
}