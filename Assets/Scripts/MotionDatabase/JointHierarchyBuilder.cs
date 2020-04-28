using System;
using UnityEngine;

namespace MotionDatabase {
	public class JointHierarchyBuilder
	{
		public SkeletonDesc skeletonDescription;

		public JointHierarchyBuilder(string serializedDescription)
		{
			skeletonDescription = JsonUtility.FromJson<SkeletonDesc>(serializedDescription);
		}
			
		public MGJoint Build(float scaleFactor=100)
		{
			skeletonDescription.referencePose.ScaleTranslations(1.0f / scaleFactor);
			return buildMGJointStructure();
		}

		private MGJoint buildMGJointStructure()
		{
			JointDesc rootJointDescription = skeletonDescription.jointDescs[0];
			MGJoint root = createJoint(null, rootJointDescription.name);
            root.relativePath = "character_grp/characterScale_grp/animation_controls_grp/body_grp/"+ root.relativePath;
            //root.relativePath = "transform1/" + root.relativePath;
            buildMGJoints(root, rootJointDescription);
			return root;
		}

		private void buildMGJoints(MGJoint parentJoint, JointDesc parentJointDescription)
		{
			foreach (string childName in parentJointDescription.children)
			{
				MGJoint childJoint = createJoint(parentJoint, childName);
				JointDesc childDesc = skeletonDescription.jointDescs[childJoint.indexInJointSequence];
				buildMGJoints(childJoint, childDesc);
			}
		}

		private MGJoint createJoint(MGJoint parent, string name)
		{
			MGJoint joint = new MGJoint();
			joint.name = name;
			joint.indexInJointSequence = Array.IndexOf(skeletonDescription.jointSequence, joint.name);
			setTransform(joint, skeletonDescription.referencePose);
			joint.parent = parent;
			if (parent != null)
			{
				joint.relativePath = parent.relativePath + "/" + joint.name;
				parent.children.Add(joint);
			}
			else
			{
				joint.relativePath = joint.name;
			}
			return joint;
		}

		private void setTransform(MGJoint joint, CPose pose)
		{
			var r = pose.rotations[joint.indexInJointSequence];
			var t = pose.translations[joint.indexInJointSequence];
			joint.rotation = new Quaternion(r.x, r.y, r.z, r.w);
			joint.position = new Vector3(t.x, t.y, t.z);
		}
	}
}
