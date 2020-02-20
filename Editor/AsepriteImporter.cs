using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Miscreant.Aseprite.Editor
{
	using Debug = UnityEngine.Debug;

	[ScriptedImporter(2, new string[] { "aseprite", "ase" } )]
    public sealed class AsepriteImporter : ScriptedImporter
    {
		public const string ATLAS_SUFFIX = "_aseprite";

		[SerializeField]
		private DefaultAsset _targetAtlasDirectory = null;

		private struct AseFileInfo
		{
			public readonly string fileAbsolutePath;
			public readonly string fileName;
			public readonly string title;
			public readonly string extension;

			public AseFileInfo(string asepriteFilePath)
			{
				this.fileAbsolutePath = asepriteFilePath;
				this.fileName = asepriteFilePath.Substring(asepriteFilePath.LastIndexOf('/') + 1);
				this.title = fileName.Substring(0, fileName.LastIndexOf('.'));
				this.extension = fileName.Substring(title.Length + 1);
			}

			public override string ToString()
			{
				return $"{nameof(AseFileInfo)}:\n\t{nameof(title)}: {title}\n\t{nameof(extension)}: {extension}\n\t{nameof(fileName)}: {fileName}\n" + 
					$"\t{nameof(fileAbsolutePath)}: {fileAbsolutePath}";
			}
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			if (_targetAtlasDirectory == null || !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_targetAtlasDirectory)))
			{
				Debug.LogError("Target atlas directory must be a valid folder under 'Assets'.");
				return;
			}

			string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
			string asepriteFilePath = $"{projectPath}/{ctx.assetPath}";
			var fileInfo = new AseFileInfo(asepriteFilePath);

			GenerateSpriteSheet(fileInfo);
		}

		private void GenerateSpriteSheet(AseFileInfo aseInfo)
		{
			string atlasDirectoryAssetPath = AssetDatabase.GetAssetPath(_targetAtlasDirectory);
			string atlasDirectoryAbsolutePath = GetAbsolutePath(atlasDirectoryAssetPath);

			string atlasPath = $"{atlasDirectoryAbsolutePath}/{aseInfo.title}{ATLAS_SUFFIX}.png";
			string dataPath = $"{atlasDirectoryAbsolutePath}/{aseInfo.title}{ATLAS_SUFFIX}.json";

			if (!File.Exists(dataPath))
			{
				// Create a new file containing some valid JSON.
				File.WriteAllText(dataPath, "{}");
			}

			RunAsepriteProcess(
				"--batch",
				"--debug",
				aseInfo.fileAbsolutePath,
				"--filename-format {title}_{tag}-{tagframe}",
				"--sheet-type packed",
				"--inner-padding 1", // Add space for the sprites to be extruded by 1px later (no native Aseprite CLI support)
				"--trim",
				$"--sheet {atlasPath}",
				"--list-tags",
				"--format json-array",
				$"--data {dataPath}"
			);

			// Import the modified assets and refresh the AssetDatabase so created/modified files show up the project window. 
			AssetDatabase.ImportAsset(GetAssetPath(dataPath), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			AssetDatabase.ImportAsset(GetAssetPath(atlasPath), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private static void RunAsepriteProcess(params string[] args)
		{
			var settings = (Settings)AssetDatabase.LoadAssetAtPath(Settings.PATH, typeof(Settings));
			var processStartInfo = new ProcessStartInfo(settings.asepritePath, string.Join(" ", args));

			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.UseShellExecute = false; // Required to be set to 'false' to read output.

			using(Process process = Process.Start(processStartInfo))
			{
				TryLogStandardOutput(process);
				TryLogStandardError(process);

				process.WaitForExit();
				process.Close();
			}
		}

		[MenuItem("Miscreant/Aseprite Importer/Validate Aseprite Path")]
		public static void CheckAsepriteInstallation()
		{
			RunAsepriteProcess(
				"--batch",
				"--version"
			);
		}

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

		private static string GetAbsolutePath(string assetPath)
		{
			assetPath = assetPath.Substring("Assets".Length);
			return Application.dataPath + "/" + assetPath;
		}

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