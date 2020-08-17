using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public sealed class AsepriteFileInfo
	{
		public string absolutePath;
		public string absoluteDirectoryPath;
		public string fileName;
		public string title;

		public SpriteSheetData spriteSheetData;

		public void Initialize(string asepriteAssetPath)
		{
			this.absolutePath = GetAbsolutePath(asepriteAssetPath);
			this.absoluteDirectoryPath = absolutePath.Substring(0, absolutePath.LastIndexOf('/'));

			string fileName = asepriteAssetPath.Substring(asepriteAssetPath.LastIndexOf("/") + 1);
			this.title = fileName.Substring(0, fileName.LastIndexOf("."));
		}

		public override string ToString()
		{
			return $"{nameof(AsepriteFileInfo)}:\n\t{nameof(title)}: {title}\n\t{nameof(fileName)}: {fileName}\n" +
				$"\t{nameof(absolutePath)}: {absolutePath}";
		}

		private string GetAbsolutePath(string assetPath)
		{
			string assetsRelativePath = assetPath.Substring("Assets".Length);
			return Application.dataPath + "/" + assetsRelativePath;
		}
	}
}