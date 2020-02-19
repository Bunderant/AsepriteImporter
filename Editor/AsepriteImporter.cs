using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Miscreant.Aseprite.Editor
{
	using Debug = UnityEngine.Debug;

	[ScriptedImporter(1, new string[] { "aseprite", "ase" } )]
    public sealed class AsepriteImporter : ScriptedImporter
    {
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
			string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
			string asepriteFilePath = $"{projectPath}/{ctx.assetPath}";

			var fileInfo = new AseFileInfo(asepriteFilePath);
			GenerateSpriteSheet(fileInfo, Application.dataPath + "/Generated");
		}

		private static void GenerateSpriteSheet(AseFileInfo aseInfo, string atlasDirectoryPath)
		{
			if (!Directory.Exists(atlasDirectoryPath))
			{
				Directory.CreateDirectory(atlasDirectoryPath);
			}

			string atlasPath = $"{atlasDirectoryPath}/{aseInfo.title}.png";
			string dataPath = $"{atlasDirectoryPath}/{aseInfo.title}.json";
			if (!File.Exists(dataPath))
			{
				// Create a new file containing some valid JSON.
				File.WriteAllText(dataPath, "{}");
			}

			RunAsepriteProcess(
				"--batch",
				"--debug",
				aseInfo.fileAbsolutePath,
				"--filename-format {title}_{tag}-{frame}",
				"--sheet-type packed",
				$"--sheet {atlasPath}",
				"--format json-array",
				$"--data {dataPath}"
			);
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
    }
}