using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEngine.Events;
#endif

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct MergedClip
	{
		public string rendererPathOverride;
		public AnimationClip clip;

		#if UNITY_EDITOR
		public class InvalidClipEvent : UnityEvent<AnimationClip> { }
		public static InvalidClipEvent OnInvalidClipAssigned = new InvalidClipEvent();
		#endif
	}
}