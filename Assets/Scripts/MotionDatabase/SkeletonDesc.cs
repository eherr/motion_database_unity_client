namespace MotionDatabaseInterface
{
	[System.Serializable]
	public class SkeletonDesc
	{
		public string root;
		public JointDesc[] jointDescs;
		public string[] jointSequence;
		public CPose referencePose;
        public float frameTime;
		public string name;
    }
}
