using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System;

namespace Miscreant.Aseprite.Editor
{
	using Debug = UnityEngine.Debug;

	#if ASEPRITE_FULL_EXT_ONLY
	[ScriptedImporter(2, new string[] { "aseprite" }, 10000, AllowCaching = true)]
	#else
	[ScriptedImporter(2, new string[] { "ase", "aseprite" }, 10000, AllowCaching = true)]
	#endif
    public sealed class AsepriteImporter : ScriptedImporter
    {
		public const string ATLAS_SUFFIX = "_aseprite";
		public const string DEFAULT_ATLAS_FOLDER = "(Generated) Miscreant - Aseprite Atlases";
		public const string DEFAULT_ANIMATION_FOLDER = "(Generated) Miscreant - Aseprite Animation";

		[SerializeField]
		private DefaultAsset _targetAtlasDirectory = null;
		[SerializeField]
		private DefaultAsset _targetAnimationDirectory = null;

		[Serializable]
		public sealed class AseFileInfo
		{
			public string absolutePath;
			public string fileName;
			public string title;
			public string extension;
			public string assetGUID;

			public SpriteSheetData spriteSheetData;

			public string defaultClipAssetPath;

			public void Initialize(string asepriteAssetPath)
			{
				this.absolutePath = GetAbsolutePath(asepriteAssetPath);
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

		public override void OnImportAsset(AssetImportContext ctx)
		{
			string defaultPath = "Assets/" + DEFAULT_ATLAS_FOLDER;
			string defaultAnimationPath = "Assets/" + DEFAULT_ANIMATION_FOLDER;

			if (!_targetAtlasDirectory  && !AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultPath))
			{
				AssetDatabase.CreateFolder("Assets", DEFAULT_ATLAS_FOLDER);
				AssetDatabase.ImportAsset(defaultPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
			}

			if (!_targetAnimationDirectory && !AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultAnimationPath))
			{
				AssetDatabase.CreateFolder("Assets", DEFAULT_ANIMATION_FOLDER);
				AssetDatabase.ImportAsset(defaultAnimationPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
			}

			var fileInfo = string.IsNullOrEmpty(userData) ? new AseFileInfo() : JsonUtility.FromJson<AseFileInfo>(userData);
			fileInfo.Initialize(assetPath);

			// Delay asset generation so we can access the AssetImporter for the newly created atlas. 
			EditorApplication.delayCall += Delayed;
			void Delayed()
			{
				if (_targetAtlasDirectory == null)
				{
					// TODO: Miscreant: Properly apply changes to importer when fields are set via script
					_targetAtlasDirectory = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultPath);
				}

				if (_targetAtlasDirectory == null || !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_targetAtlasDirectory)))
				{
					Debug.LogError("Target atlas directory must be a valid folder under 'Assets'.");
					return;
				}

				if (_targetAnimationDirectory == null)
				{
					// TODO: Miscreant: Properly apply changes to importer when fields are set via script
					_targetAnimationDirectory = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultAnimationPath);
				}

				if (_targetAnimationDirectory == null || !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_targetAnimationDirectory)))
				{
					Debug.LogError("Target animation directory must be a valid folder under 'Assets'.");
					return;
				}

				EditorApplication.delayCall -= Delayed;
				GenerateSpriteSheet(fileInfo);

				// TODO: Miscreant: This shouldn't be done once per import. Wait til everything's been imported. 
				AssetDatabase.Refresh();
			}
		}

		private void GenerateSpriteSheet(AseFileInfo aseInfo)
		{
			string atlasDirectoryAssetPath = AssetDatabase.GetAssetPath(_targetAtlasDirectory);
			string atlasDirectoryAbsolutePath = GetAbsolutePath(atlasDirectoryAssetPath);

			string atlasPath = $"{atlasDirectoryAbsolutePath}/{aseInfo.title}{ATLAS_SUFFIX}.png";
			string dataPath = $"{atlasDirectoryAbsolutePath}/{aseInfo.title}{ATLAS_SUFFIX}.json";

			// Create a temporary valid JSON file so Aseprite has something to write into.
			// The file will be deleted after the json data is transferred to the meta file of the generated atlas. 
			File.WriteAllText(dataPath, "{}");

			RunAsepriteProcess(
				"--batch",
				"--debug",
				$"\"{aseInfo.absolutePath}\"",
				"--filename-format {title}_{tag}-{tagframe}",
				"--sheet-type packed",
				"--inner-padding 1", // Add space for the sprites to be extruded by 1px later (no native Aseprite CLI support)
				"--trim",
				$"--sheet \"{atlasPath}\"",
				"--list-tags",
				"--format json-array",
				$"--data \"{dataPath}\""
			);

			string atlasAssetPath = GetAssetPath(atlasPath);

			// Import the atlas immediately so we can access its importer, then store JSON data in its meta file. 
			AssetDatabase.ImportAsset(atlasAssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

			aseInfo.defaultClipAssetPath = AssetDatabase.GetAssetPath(_targetAnimationDirectory);
			aseInfo.UpdateAsepriteData(JsonUtility.FromJson<SpriteSheetData>(File.ReadAllText(dataPath)));

			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(atlasAssetPath);
			importer.userData = JsonUtility.ToJson(aseInfo);
			AssetDatabase.WriteImportSettingsIfDirty(atlasAssetPath);

			// Now that we have all sprite sheet data saved in the meta file, delete its temp JSON file generated by Aseprite. 
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
    }
}