namespace MotionDatabase {
	[System.Serializable]
	public class JointDesc
	{
		public string name; // name in MorphableGraphs
		public float[] offset; // name in Unity
		public string targetName; // name in Unity
		public string[] children;
	}
}
