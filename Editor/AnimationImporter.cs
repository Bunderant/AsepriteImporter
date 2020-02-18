using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace Miscreant.AsepriteImporter.Editor
{
	using Debug = UnityEngine.Debug;

    public sealed class AnimationImporter : AssetImporter
    {
		[MenuItem("Miscreant/Aseprite Importer/Validate Aseprite Path")]
		public static void CheckAsepriteInstallation()
		{
			var settings = (Settings)AssetDatabase.LoadAssetAtPath(Settings.PATH, typeof(Settings));

			var processStartInfo = new ProcessStartInfo(settings.asepritePath, "--version");

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