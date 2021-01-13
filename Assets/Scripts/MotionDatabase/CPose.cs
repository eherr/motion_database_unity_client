using UnityEngine;

namespace MotionDatabase
{
	[System.Serializable]
	public class CPose : CKeyframe
	{
		public Vector3[] translations;

		public void ScaleTranslations(float scaleFactor)
		{
			for (int i = 0; i < translations.Length; i++)
			{
				translations[i] *= scaleFactor;
			}
		}
	}
}
