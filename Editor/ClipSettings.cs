using UnityEngine;
using System;

namespace Miscreant.Aseprite.Editor
{
	[Serializable]
	public struct ClipSettings
	{
		public const string DEFAULT_RENDERER_PATH = "";

		[Tooltip("Path from the root Animator to the SpriteRenderer's GameObject.")]
		public string spriteRendererPath;

		public static ClipSettings Default
		{
			get 
			{
				return new ClipSettings {
					spriteRendererPath = DEFAULT_RENDERER_PATH,
				};
			}
		}

		public static float CalculateAutoFrameRate(float maxFPS, params int[] keyframeDurationsInMilliseconds)
		{
			int GCD = 0;

			for(int i = 0; i < keyframeDurationsInMilliseconds.Length - 1; i++)
			{
				GCD = Euclid(GCD, keyframeDurationsInMilliseconds[i]);
			}

			float FPS = 1 / (GCD / 1000.0f);
			return FPS < maxFPS ? FPS : maxFPS;
		}


		/// <summary>
		/// Iterative function to calculate the GCD of two numbers using Euclid’s Algorithm
		/// </summary>
		private static int Euclid(int a, int b)
		{
			int r;
			
			while (b > 0)
			{
				r = a % b;
				a = b;
				b = r;
			}

			return a;
		}
	}
}
