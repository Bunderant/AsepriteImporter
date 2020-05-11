﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Collections.Generic;

namespace Miscreant.Aseprite.Editor
{
	using Object = UnityEngine.Object;

#if ASEPRITE_FULL_EXT_ONLY
	[ScriptedImporter(AsepriteImporter.VERSION, new string[] { "aseprite" }, AsepriteImporter.QUEUE_OFFSET, AllowCaching = AsepriteImporter.IS_CACHING_ALLOWED)]
#else
	[ScriptedImporter(AsepriteImporter.VERSION, new string[] { "ase", "aseprite" }, AsepriteImporter.QUEUE_OFFSET, AllowCaching = AsepriteImporter.IS_CACHING_ALLOWED)]
#endif
	public sealed class AsepriteImporter : ScriptedImporter
	{
		public const int VERSION = 4;
		public const int QUEUE_OFFSET = 10000;
		public const bool IS_CACHING_ALLOWED = true;

		public bool generateAnimationClips;
		public ClipSettings clipSettings = ClipSettings.Default;

		[SerializeField]
		private GeneratedClip[] m_generatedClips;

		[SerializeField]
		private Texture2D m_atlas;
		public Texture2D Atlas { get { return m_atlas; } }

		[SerializeField]
		private int m_spriteCount;
		public int SpriteCount { get { return m_spriteCount; } }

		public int clipCount;


		public override void OnImportAsset(AssetImportContext ctx)
		{
			Settings settings = Settings.LoadSettingsAsset();

			var main = ScriptableObject.CreateInstance<AsepriteAsset>();
			ctx.AddObjectToAsset("Main", main, settings.GetIcon(generateAnimationClips));
			ctx.SetMainObject(main);

			var fileInfo = string.IsNullOrEmpty(userData) ? new AsepriteFileInfo() : JsonUtility.FromJson<AsepriteFileInfo>(userData);
			fileInfo.Initialize(assetPath);

			GenerateAssets(settings, fileInfo, ctx);
		}

		private void GenerateAssets(Settings settings, AsepriteFileInfo aseInfo, AssetImportContext ctx)
		{
			string atlasPath = $"{aseInfo.absoluteDirectoryPath}/{aseInfo.title}_aseprite.png";
			string dataPath = $"{aseInfo.absoluteDirectoryPath}/{aseInfo.title}_aseprite.json";

			// Create a temporary valid JSON file so Aseprite has something to write into.
			// The file will be deleted after the json data is transferred to the meta file of the generated atlas. 
			File.WriteAllText(dataPath, "{}");

			AsepriteCLI.Run(
				"--batch",
				"--debug",
				$"\"{aseInfo.absolutePath}\"",
				"--filename-format {title}_{tag}-{tagframe}",
				"--sheet-type packed",
				"--inner-padding 1",
				"--trim",
				$"--sheet \"{atlasPath}\"",
				"--list-tags",
				"--format json-array",
				$"--data \"{dataPath}\""
			);

			aseInfo.UpdateAsepriteData(JsonUtility.FromJson<SpriteSheetData>(File.ReadAllText(dataPath)));

			m_atlas = CreateSpriteAtlasTextureAsset(
				aseInfo.spriteSheetData.meta.size.w,
				aseInfo.spriteSheetData.meta.size.h,
				File.ReadAllBytes(atlasPath)
			);
			ctx.AddObjectToAsset(m_atlas.name, m_atlas);

			List<Sprite> sprites = CreateSpritesForAtlas(m_atlas, aseInfo.spriteSheetData);
			m_spriteCount = sprites.Count;
			foreach (Sprite sprite in sprites)
			{
				ctx.AddObjectToAsset(sprite.name, sprite);
			}

			if (generateAnimationClips)
			{
				m_generatedClips = CreateAnimationClips(settings, aseInfo, sprites);
				foreach (GeneratedClip clipData in m_generatedClips)
				{
					ctx.AddObjectToAsset(clipData.name, clipData.clip);
				}
			}
			else
			{
				clipCount = 0;
				m_generatedClips = new GeneratedClip[clipCount];
			}

			// Now that we have all generated assets saved as sub-objects, delete the temp files created by Aseprite. 
			File.Delete(atlasPath);
			File.Delete(dataPath);
		}

		/// <summary>
		/// Given an absolute path to an asset under the 'Assets' directory, return the path relative to the project.
		/// </summary>
		/// <param name="absolutePath">The absolute path to the asset.</param>
		/// <returns>The project-relative path, beginning with 'Assets'</returns>
		private static string GetAssetPath(string absolutePath)
		{
			if (!absolutePath.Contains(Application.dataPath))
			{
				throw new System.Exception("Path must be part of Application.dataPath");
			}

			// Trim Application.dataPath from the front, rather than search for the "Assets" folder, 
			// just in case there are other subfolders with that name. 
			return absolutePath.Remove(0, Application.dataPath.Length - "Assets".Length);
		}

		private Texture2D CreateSpriteAtlasTextureAsset(int width, int height, byte[] rawData)
		{
			var atlas = new Texture2D(width, height, TextureFormat.RGBA32, false);

			atlas.LoadImage(rawData, true);
			atlas.filterMode = FilterMode.Point;
			atlas.name = "Packed Sprites";
			atlas.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

			return atlas;
		}

		private List<Sprite> CreateSpritesForAtlas(Texture2D atlas, SpriteSheetData sheetData)
		{
			var sprites = new List<Sprite>(sheetData.frames.Length);

			foreach (SpriteSheetData.Frame frame in sheetData.frames)
			{
				Vector2 pivot = new Vector2(
					(frame.sourceSize.w * 0.5f - frame.spriteSourceSize.x) / frame.spriteSourceSize.w,
					(frame.sourceSize.h * 0.5f - (frame.sourceSize.h - frame.spriteSourceSize.y - frame.spriteSourceSize.h)) / frame.spriteSourceSize.h
				);

				UnityEngine.Rect rect = frame.GetUnityTextureRect(sheetData.meta.size.h);

				Sprite sprite = Sprite.Create(
					atlas,
					rect,
					pivot,
					16
				);

				sprite.name = frame.filename;

				sprites.Add(sprite);
			}

			return sprites;
		}

		private Dictionary<string, Sprite> GetSpriteLookup(string atlasAssetPath)
		{
			Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(atlasAssetPath);

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

		private GeneratedClip[] CreateAnimationClips(Settings settings, AsepriteFileInfo aseInfo, List<Sprite> sprites)
		{
			SpriteSheetData sheetData = aseInfo.spriteSheetData;

			var clipInfoLookup = new Dictionary<string, List<(Sprite sprite, int duration)>>();
			foreach (SpriteSheetData.FrameTag frameTag in sheetData.meta.frameTags)
			{
				var framesInfo = new List<(Sprite sprite, int duration)>();

				for (int i = frameTag.from; i <= frameTag.to; i++)
				{
					SpriteSheetData.Frame frame = sheetData.frames[i];
					Sprite sprite = sprites.Find((s) => { return frame.filename.Equals(s.name); });

					if (ReferenceEquals(sprite, null))
					{
						continue;
					}

					int duration = frame.duration;
					framesInfo.Add((sprite, duration));
				}

				string clipName = $"aseprite_{aseInfo.title}_{frameTag.name}";
				clipInfoLookup.Add(clipName, framesInfo);
			}

			var clips = new List<AnimationClip>(clipInfoLookup.Count);

			foreach (var kvp in clipInfoLookup)
			{
				string clipName = kvp.Key;
				List<(Sprite sprite, int duration)> clipInfo = kvp.Value;

				AnimationClip clip = new AnimationClip();
				clip.wrapMode = WrapMode.Loop;
				clip.name = clipName;

				int[] frameTimesInMilliseconds = new int[clipInfo.Count];
				for (int iFrame = 0; iFrame < frameTimesInMilliseconds.Length; iFrame++)
				{
					frameTimesInMilliseconds[iFrame] = clipInfo[iFrame].duration;
				}
				clip.frameRate = ClipSettings.CalculateAutoFrameRate(settings.MaxSampleRate, frameTimesInMilliseconds);

				AnimationClipSettings currentClipSettings = AnimationUtility.GetAnimationClipSettings(clip);
				currentClipSettings.loopTime = true;
				AnimationUtility.SetAnimationClipSettings(clip, currentClipSettings);

				var spriteBinding = new EditorCurveBinding();
				spriteBinding.type = typeof(SpriteRenderer);
				spriteBinding.path = clipSettings.spriteRendererPath;
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
					float keyDuration = clipInfo[i].duration / 1000f;
					currentDuration += keyDuration;

					// TODO: Miscreant: Do these calculations before any sec/msec conversions for more precision
					// Tack on a duplicate of the last keyframe to ensure the last frame gets its full duration
					if (i == clipLength - 1 && keyDuration > (1.0f / clip.frameRate))
					{
						keyframe = new ObjectReferenceKeyframe();
						// The last frame will persist for one full sample, so subtract that from the current time
						keyframe.time = currentDuration - (1.0f / clip.frameRate);
						keyframe.value = clipInfo[i].sprite;
						keyframes.Add(keyframe);
					}
				}

				AnimationUtility.SetObjectReferenceCurve(
					clip,
					spriteBinding,
					keyframes.ToArray()
				);

				clips.Add(clip);
			}

			return FinalizeAnimationClips(aseInfo, clips);
		}

		private GeneratedClip[] FinalizeAnimationClips(AsepriteFileInfo aseInfo, List<AnimationClip> clips)
		{
			clipCount = clips.Count;

			var previousGeneratedClips = new Dictionary<string, GeneratedClip>();
			foreach (var generatedClip in m_generatedClips)
			{
				previousGeneratedClips.Add(generatedClip.name, generatedClip);
			}

			var newGeneratedClips = new List<GeneratedClip>(clipCount);

			for (int i = 0; i < clipCount; i++)
			{
				AnimationClip clip = clips[i];
				string tagName = aseInfo.spriteSheetData.meta.frameTags[i].name;

				bool clipDoesExist = previousGeneratedClips.TryGetValue(tagName, out GeneratedClip clipData);

				if (clipDoesExist)
				{
					clipData.name = tagName;
					clipData.clip = clip;
				}
				else
				{
					clipData = GeneratedClip.Create(tagName, clip);
				}

				newGeneratedClips.Add(clipData);
			}

			return newGeneratedClips.ToArray();
		}
	}
}
