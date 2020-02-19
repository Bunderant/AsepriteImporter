using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text;

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
			Debug.Log("Imported an .ase file.");

			string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
			string asepriteFilePath = $"{projectPath}/{ctx.assetPath}";

			var fileInfo = new AseFileInfo(asepriteFilePath);
			Debug.Log(fileInfo.ToString());
		}

		private static void GenerateSpriteSheet(string asepriteFilePath, string atlasDirectoryPath)
		{

		}

		private static void RunAsepriteProcess(params string[] args)
		{
			var settings = (Settings)AssetDatabase.LoadAssetAtPath(Settings.PATH, typeof(Settings));

			// Convert the arguments to a single concatenated string
			var concatenatedArgs = new StringBuilder(string.Empty);
			for (int i = 0; i < args.Length; i++)
			{
				concatenatedArgs.Append(args[i]);
				if (i != args.Length - 1)
				{
					concatenatedArgs.Append(" ");
				}
			}

			var processStartInfo = new ProcessStartInfo(
				settings.asepritePath,
				concatenatedArgs.ToString()
			);

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