﻿using UnityEngine;
using UnityEditor;
using Miscreant.Aseprite.Editor;
using System.Collections.Generic;

using AseFileInfo = Miscreant.Aseprite.Editor.AsepriteImporter.AseFileInfo;

/// <summary>
/// Automatically adds sprite data to packed atlases generated by importing Aseprite files
/// </summary>
class AsepriteAtlasPostprocessor : AssetPostprocessor // TODO: Miscreant: Move this to a precompiled .dll to prevent modification once it's stable
{
	void OnPreprocessTexture()
	{
		// Sprite slicing data will be stored in the importer's user data.
		// On create, it will be empty on the first import, but immediately reimported after the data is added. 
		if (assetPath.Contains(AsepriteImporter.ATLAS_SUFFIX) && !string.IsNullOrEmpty(assetImporter.userData))
		{
			AseFileInfo aseInfo = JsonUtility.FromJson<AseFileInfo>(assetImporter.userData);
			UpdatePackedSprites(aseInfo);

			// Delay animation clip creation to ensure all sprites are established in the asset database. 
			EditorApplication.delayCall += CreateAnimationClipsDelayed;
			void CreateAnimationClipsDelayed()
			{
				CreateAnimationClips(aseInfo);
				EditorApplication.delayCall -= CreateAnimationClipsDelayed;
			}
		}
	}

	private void UpdatePackedSprites(AseFileInfo aseInfo)
	{
		TextureImporter ti = (TextureImporter)assetImporter;

		List<SpriteMetaData> existingSpriteData = new List<SpriteMetaData>(ti.spritesheet);
		List<SpriteMetaData> newSpriteData = new List<SpriteMetaData>(aseInfo.spriteSheetData.frames.Length);

		int atlasHeight = aseInfo.spriteSheetData.meta.size.h;
		foreach (SpriteSheetData.Frame frame in aseInfo.spriteSheetData.frames)
		{
			var newSprite = new SpriteMetaData();

			int matchIndex = existingSpriteData.FindIndex((sprite) => {
				return sprite.name.Equals(frame.filename);
			});

			if (matchIndex >= 0)
			{
				var existingSprite = existingSpriteData[matchIndex];
				
				newSprite.border = existingSprite.border;
				newSprite.name = existingSprite.name;
			}
			else
			{
				newSprite.border = Vector4.zero;
				newSprite.name = frame.filename;
			}

			newSprite.alignment = (int)SpriteAlignment.Custom;
			newSprite.pivot = new Vector2(
				(frame.sourceSize.w * 0.5f - frame.spriteSourceSize.x) / frame.spriteSourceSize.w,
				(frame.sourceSize.h * 0.5f - (frame.sourceSize.h - frame.spriteSourceSize.y - frame.spriteSourceSize.h)) / frame.spriteSourceSize.h
			);
			
			var textureRect = frame.GetUnityTextureRect(atlasHeight);
			newSprite.rect = new Rect(
				textureRect.x,
				textureRect.y,
				textureRect.w,
				textureRect.h
			);

			newSpriteData.Add(newSprite);
		}

		ti.spritesheet = newSpriteData.ToArray();
	}

	private Dictionary<string, Sprite> GetSpriteLookup()
	{
		Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

		var spriteLookup = new Dictionary<string, Sprite>(spriteObjects.Length);
		foreach (var obj in spriteObjects)
		{
			if (!(obj is Sprite))
			{
				continue;
			}

			var sprite = (Sprite)obj;
			spriteLookup.Add(sprite.name, sprite);
		}

		return spriteLookup;
	}

	private void CreateAnimationClips(AseFileInfo aseInfo)
	{
		SpriteSheetData sheetData = aseInfo.spriteSheetData;

		Dictionary<string, Sprite> spriteLookup = GetSpriteLookup();
		var clipInfoLookup = new Dictionary<string, List<(Sprite sprite, int duration)>>();
		foreach (SpriteSheetData.FrameTag frameTag in sheetData.meta.frameTags)
		{
			var framesInfo = new List<(Sprite sprite, int duration)>();

			for (int i = frameTag.from; i <= frameTag.to; i++)
			{
				SpriteSheetData.Frame frame = sheetData.frames[i];

				if (!spriteLookup.ContainsKey(frame.filename))
				{
					continue;
				}

				Sprite sprite = spriteLookup[frame.filename];
				int duration = frame.duration;

				framesInfo.Add((sprite, duration));
			}

			string clipName = $"aseprite_{aseInfo.title}_{frameTag.name}";
			clipInfoLookup.Add(clipName, framesInfo);
		}

		foreach (var kvp in clipInfoLookup)
		{
			string clipName = kvp.Key;
			List<(Sprite sprite, int duration)> clipInfo = kvp.Value;

			// TODO: should try to load based on the clip's GUID first. 
			string clipPath = $"{aseInfo.defaultClipAssetPath}/{clipName}.anim";
			var clip = AssetDatabase.LoadAssetAtPath(clipPath, typeof(AnimationClip)) as AnimationClip;

			if (clip == null)
			{
				clip = new AnimationClip();
				clip.wrapMode = WrapMode.Loop;
				clip.frameRate = 60;

				AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
				clipSettings.loopTime = true;
				AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

				AssetDatabase.CreateAsset(clip, clipPath);
				// TODO: Register this clip's GUID with the importer's AseFileInfo (will require an import to get the GUID from the path).
			}

			var spriteBinding = new EditorCurveBinding();
			spriteBinding.type = typeof(SpriteRenderer);
			spriteBinding.path = "Renderer"; // TODO: Miscreant: this should be configurable in the importer. 
			spriteBinding.propertyName = "m_Sprite";

			int clipLength = clipInfo.Count;
			var keyframes = new List<ObjectReferenceKeyframe>(clipLength + 1);

			float currentDuration = 0f;

			for (int i = 0; i < clipLength; i++)
			{
				var keyframe = new ObjectReferenceKeyframe();

				keyframe.value = clipInfo[i].sprite;
				keyframe.time = currentDuration;
				keyframes.Add(keyframe);

				// Divide frame duration by 1000 because it is specified by Aseprite in milliseconds. 
				currentDuration += clipInfo[i].duration / 1000f;

				// Tack on a duplicate of the last keyframe to ensure the last frame gets its full duration
				if (i == clipLength - 1)
				{
					keyframe = new ObjectReferenceKeyframe();
					keyframe.time = currentDuration;
					keyframe.value = clipInfo[i].sprite;
					keyframes.Add(keyframe);
				}
			}

			AnimationUtility.SetObjectReferenceCurve(
				clip,
				spriteBinding,
				keyframes.ToArray());

			EditorUtility.SetDirty(clip);
			// TODO: Miscreant: Make sure the database doesn't need to be refreshed if the clips are modified rather than freshly created. 
		}
	}
}