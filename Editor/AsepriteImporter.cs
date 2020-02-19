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
		public override void OnImportAsset(AssetImportContext ctx)
		{
			Debug.Log("Imported an .ase file.");
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