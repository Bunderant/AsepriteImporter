using System.Diagnostics;
using System.IO;
using UnityEditor;

using Debug = UnityEngine.Debug;

namespace Miscreant.Aseprite.Editor
{
	internal static class AsepriteCLI
	{
		/// <summary>
		/// Runs Aseprite with the given command line interface arguments. <see href="https://www.aseprite.org/docs/cli/">Aseprite Docs</see>.
		/// </summary>
		/// <param name="args">Arguments to the CLI.</param>
		internal static void Run(params string[] args)
		{
			var settings = (Settings)AssetDatabase.LoadAssetAtPath(Settings.PATH, typeof(Settings));
			var processStartInfo = new ProcessStartInfo(settings.asepritePath, string.Join(" ", args));

			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.UseShellExecute = false; // Required to be set to 'false' to read output.

			using (Process process = Process.Start(processStartInfo))
			{
				process.WaitForExit();

				TryLogStandardOutput(process);
				TryLogStandardError(process);

				process.Close();
			}
		}

		/// <summary>
		/// Prints the currently installed version of Aseprite. 
		/// </summary>
		[MenuItem("Miscreant/Aseprite Importer/Validate Aseprite Path")]
		internal static void CheckAsepriteInstallation()
		{
			Run(
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
	}
}
