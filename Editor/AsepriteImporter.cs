using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System;
using System.Collections.Generic;

namespace Miscreant.Aseprite.Editor
{
	using Debug = UnityEngine.Debug;
	using Object = UnityEngine.Object;

	#if ASEPRITE_FULL_EXT_ONLY
	[ScriptedImporter(3, new string[] { "aseprite" }, 10000, AllowCaching = true)]
	#else
	[ScriptedImporter(3, new string[] { "ase", "aseprite" }, 10000, AllowCaching = true)]
	#endif
    public sealed class AsepriteImporter : ScriptedImporter
    {
		[Serializable]
		public sealed class AseFileInfo
		{
			public string absolutePath;
			public string absoluteDirectoryPath;
			public string fileName;
			public string title;
			public string extension;
			public string assetGUID;

			public SpriteSheetData spriteSheetData;

			public void Initialize(string asepriteAssetPath)
			{
				this.absolutePath = GetAbsolutePath(asepriteAssetPath);
				this.absoluteDirectoryPath = absolutePath.Substring(0, absolutePath.LastIndexOf('/'));
				this.fileName = asepriteAssetPath.Substring(asepriteAssetPath.LastIndexOf('/') + 1);
				this.title = fileName.Substring(0, fileName.LastIndexOf('.'));
				this.extension = fileName.Substring(title.Length + 1);
				this.assetGUID = AssetDatabase.AssetPathToGUID(asepriteAssetPath);
			}

			public void UpdateAsepriteData(SpriteSheetData newData)
			{
				// If the data hasn't initialized yet, just set it directly and return. 
				if (ReferenceEquals(spriteSheetData, null))
				{
					this.spriteSheetData = newData;
					return;
				}

				this.spriteSheetData = newData;
			}

			public override string ToString()
			{
				return $"{nameof(AseFileInfo)}:\n\t{nameof(title)}: {title}\n\t{nameof(extension)}: {extension}\n\t{nameof(fileName)}: {fileName}\n" + 
					$"\t{nameof(absolutePath)}: {absolutePath}";
			}
		}

		[SerializeField, HideInInspector]
		private AsepriteAsset _mainObject;
		
		public override void OnImportAsset(AssetImportContext ctx)
		{
			List<Object> existing = new List<Object>();
			ctx.GetObjects(existing);
			foreach(var obj in existing)
			{
				Object.DestroyImmediate(obj);
			}

			_mainObject = ScriptableObject.CreateInstance<AsepriteAsset>();
			_mainObject.name = "Main";
			ctx.AddObjectToAsset(_mainObject.name, _mainObject);
			ctx.SetMainObject(_mainObject);

			var fileInfo = string.IsNullOrEmpty(userData) ? new AseFileInfo() : JsonUtility.FromJson<AseFileInfo>(userData);
			fileInfo.Initialize(assetPath);

			GenerateAssets(fileInfo, ctx);

			// TODO: Miscreant: This shouldn't be done once per import. Wait til everything's been imported. 
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		private void GenerateAssets(AseFileInfo aseInfo, AssetImportContext ctx)
		{
			string atlasPath = $"{aseInfo.absoluteDirectoryPath}/{aseInfo.title}_aseprite.png";
			string dataPath = $"{aseInfo.absoluteDirectoryPath}/{aseInfo.title}_aseprite.json";

			// Create a temporary valid JSON file so Aseprite has something to write into.
			// The file will be deleted after the json data is transferred to the meta file of the generated atlas. 
			File.WriteAllText(dataPath, "{}");

			RunAsepriteProcess(
				"--batch",
				"--debug",
				$"\"{aseInfo.absolutePath}\"",
				"--filename-format {title}_{tag}-{tagframe}",
				"--sheet-type packed",
				"--trim",
				$"--sheet \"{atlasPath}\"",
				"--list-tags",
				"--format json-array",
				$"--data \"{dataPath}\""
			);

			aseInfo.UpdateAsepriteData(JsonUtility.FromJson<SpriteSheetData>(File.ReadAllText(dataPath)));

			Texture2D atlasTexture = new Texture2D(aseInfo.spriteSheetData.meta.size.w, aseInfo.spriteSheetData.meta.size.h, TextureFormat.RGBA32, false);

			atlasTexture.LoadImage(File.ReadAllBytes(atlasPath), true);
			atlasTexture.filterMode = FilterMode.Point;
			atlasTexture.name = "Packed Sprites";
			ctx.AddObjectToAsset(atlasTexture.name, atlasTexture);
			_mainObject.packedSpriteTexture = atlasTexture;

			List<Sprite> sprites = CreateSpritesForAtlas(atlasTexture, aseInfo.spriteSheetData);
			foreach (Sprite sprite in sprites)
			{
				ctx.AddObjectToAsset(sprite.name, sprite);
			}

			_mainObject.clipCount = 0;
			foreach (AnimationClip clip in CreateAnimationClips(aseInfo, sprites))
			{
				ctx.AddObjectToAsset(clip.name, clip);
				_mainObject.clipCount++;
			}

			// Now that we have all generated assets saved as sub-objects, delete the temp files created by Aseprite. 
			File.Delete(atlasPath);
			File.Delete(dataPath);
		}

		/// <summary>
		/// Runs Aseprite with the given command line interface arguments. <see href="https://www.aseprite.org/docs/cli/">Aseprite Docs</see>.
		/// </summary>
		/// <param name="args">Arguments to the CLI.</param>
		private static void RunAsepriteProcess(params string[] args)
		{
			var settings = (Settings)AssetDatabase.LoadAssetAtPath(Settings.PATH, typeof(Settings));
			var processStartInfo = new ProcessStartInfo(settings.asepritePath, string.Join(" ", args));

			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.UseShellExecute = false; // Required to be set to 'false' to read output.

			using(Process process = Process.Start(processStartInfo))
			{
				process.WaitForExit();

				TryLogStandardOutput(process);
				TryLogStandardError(process);

				process.Close();
			}
		}

		[MenuItem("Miscreant/Aseprite Importer/Validate Aseprite Path")]
		/// <summary>
		/// Prints the currently installed version of Aseprite. 
		/// </summary>
		public static void CheckAsepriteInstallation()
		{
			RunAsepriteProcess(
				"--batch",
				"--version"
			);
		}

		/// <summary>
		/// Logs the standard output for the given process.
		/// </summary>
		/// <param name="process">The external process.</param>
		private static void TryLogStandardOutput(Process process)
		{
			using (StreamReader streamReader = process.StandardOutput)
			{
				string output = streamReader.ReadToEnd();
				if (!string.IsNullOrEmpty(output))
				{
					Debug.Log(output);
				}
			}
		}

		/// <summary>
		/// Logs the error output of the given process.
		/// </summary>
		/// <param name="process">The external procss.</param>
		private static void TryLogStandardError(Process process)
		{
			using (StreamReader streamReader = process.StandardError)
			{
				string output = streamReader.ReadToEnd();
				if (!string.IsNullOrEmpty(output))
				{
					Debug.LogError(output);
				}
			}
		}

		/// <summary>
		/// Given an asset path relative to the project directory, returns the absolute path.
		/// </summary>
		/// <param name="assetPath">The project-relative path, beginning with 'Assets'</param>
		/// <returns>The absolute path.</returns>
		private static string GetAbsolutePath(string assetPath)
		{
			if (assetPath.Substring(0, "Assets".Length) != "Assets")
			{
				throw new System.Exception("Path must begin with the project's \"Assets\" directory.");
			}

			assetPath = assetPath.Substring("Assets".Length);
			return Application.dataPath + "/" + assetPath;
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

		private List<AnimationClip> CreateAnimationClips(AseFileInfo aseInfo, List<Sprite> sprites)
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
				clip.frameRate = 60; // TODO: Miscreant: Should be configurable in the inspector. 
				clip.name = clipName;

				AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
				clipSettings.loopTime = true;
				AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

				var spriteBinding = new EditorCurveBinding();
				spriteBinding.type = typeof(SpriteRenderer);
				spriteBinding.path = "Renderer"; // TODO: Miscreant: Should be configurable in the inspector. 
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
					keyframes.ToArray()
				);

				clips.Add(clip);
			}

			return clips;
		}
    }
}
