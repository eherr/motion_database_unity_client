using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionDatabase{
	public class SkeletonManager : MonoBehaviour {

		public float scale = 0.01f;

		public Dictionary<string, MGJoint> rootJoints;
		public Dictionary<string, GameObject> skeletons;

		public string skeletonJointTag = "SKELETON_JOINT";

		public GameObject jointPrefab;
		// Use this for initialization
		void Start () {
			rootJoints = new Dictionary<string, MGJoint>();
			skeletons = new Dictionary<string, GameObject>();
		}

		public void HideSkeletons(){
			GameObject[] skeletonJoints = GameObject.FindGameObjectsWithTag(skeletonJointTag);
			foreach(GameObject j in skeletonJoints){
            	j.gameObject.SetActive(false);
			}
		}
		public void HideSkeleton(string name){
			if (rootJoints.ContainsKey(name)){
				var root = rootJoints[name].transform;
				hideHierarchy(root);
			}
		}
		 public void hideHierarchy(Transform node)
        {
            for(int idx = 0; idx < node.childCount; idx++){
                var child = node.GetChild(idx);
                hideHierarchy(child);
            }
            node.gameObject.SetActive(false);
        }

		public void ShowSkeleton(string name){
			if (rootJoints.ContainsKey(name)){
				var root = rootJoints[name].transform;
				showHierarchy(root);
			}
		}
		 public void showHierarchy(Transform node)
        {
            for(int idx = 0; idx < node.childCount; idx++){
                var child = node.GetChild(idx);
                showHierarchy(child);
            }
            node.gameObject.SetActive(true);
        }
		public GameObject GetSkeleton(SkeletonDesc skeletonDesc){
			if (!rootJoints.ContainsKey(skeletonDesc.name)){
				skeletons[skeletonDesc.name] = createDebugSkeleton(skeletonDesc);
			}
			return skeletons[skeletonDesc.name];
		}
		public MGJoint GetRootJoint(SkeletonDesc skeletonDesc){
			if (!rootJoints.ContainsKey(skeletonDesc.name)){
				skeletons[skeletonDesc.name] = createDebugSkeleton(skeletonDesc);
			}
			return rootJoints[skeletonDesc.name];
		}
		public GameObject createDebugSkeleton(SkeletonDesc skeletonDesc)
		{

			JointDesc jointDesc = skeletonDesc.jointDescs[0];
			var root = new MGJoint();
			
			GameObject skeleton = root.createGameObjectfromDesc(jointDesc, skeletonDesc, gameObject, skeletonJointTag, jointPrefab, scale);
			string name = skeletonDesc.name;
			rootJoints[name] = root;
			return skeleton;
		}
	}

}