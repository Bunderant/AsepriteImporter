using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct MergedClip
	{
		public string rendererPathOverride;
		public AnimationClip clip;
	}
}