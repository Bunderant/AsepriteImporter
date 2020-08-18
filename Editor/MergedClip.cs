using UnityEngine;
using UnityEditor;
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

		public static MergedClip Default { get { return new MergedClip(); } }

		public void SetKeyframes(ObjectReferenceKeyframe[] keyframes, string rendererPath)
		{
			EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

			Type targetType = typeof(SpriteRenderer);
			string targetBindingPath = !string.IsNullOrEmpty(rendererPathOverride) ? 
				rendererPathOverride : 
				rendererPath; 
			string targetPropertyName = "m_Sprite";

			EditorCurveBinding targetBinding = new EditorCurveBinding{ 
				type = targetType,
				path = targetBindingPath,
				propertyName = targetPropertyName
			};

			AnimationUtility.SetObjectReferenceCurve(clip, targetBinding, keyframes);
		}
	}
}