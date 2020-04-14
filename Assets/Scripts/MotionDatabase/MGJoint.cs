using System.Collections.Generic;
using UnityEngine;
using System;

namespace MotionDatabaseInterface {

    public class MGJoint
    {
        public string name;
        public List<MGJoint> children = new List<MGJoint>();
		public MGJoint parent = null;
		public string relativePath;
		public int indexInJointSequence;
		public Quaternion rotation;
		public Vector3 position;
        public Transform transform;

        public JointDesc findJointDesc(JointDesc[] descs, string name){
            JointDesc r = null;
            foreach(JointDesc j in descs){
                if (j.name == name){
                    r = j;
                    break;
                }
            }
            return r;
        }

        public void setPose(CPose pose, Dictionary<string, int> indexMap)
        {
            if (indexMap.ContainsKey(name) && transform != null)
            {
                var r = pose.rotations[indexMap[name]];
                var t = pose.translations[indexMap[name]];
                transform.localRotation = new Quaternion(r.x, r.y, r.z, r.w);
                //transform.localPosition = new Vector3(t.x, t.y, t.z);
            }
            foreach (MGJoint c in children)
            {
                c.setPose(pose, indexMap);
            }

        }

        public void setPose(CKeyframe frame, Dictionary<string, int> indexMap)
        {
            if (indexMap.ContainsKey(name) && transform != null)
            {
                var r = frame.rotations[indexMap[name]];
                transform.localRotation = new Quaternion(r.x, r.y, r.z, r.w);
            }
            foreach (MGJoint c in children)
            {
                c.setPose(frame, indexMap);
            }
        }

        public void setPose(float[] pose, Dictionary<string, int> indexMap)
        {
            if (indexMap.ContainsKey(name) && transform != null)
            {
                int o = indexMap[name]*4+3;
                transform.localRotation = new Quaternion(pose[o+1], pose[o+2], pose[o+3], pose[o]);
            }
            foreach (MGJoint c in children)
            {
                c.setPose(pose, indexMap);
            }
        }

        /// <summary>
        ///  Depth first search for transform with target name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public Transform targetSearchDF(Transform parent, string targetName, int level = 0, bool debug = false)
        {

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child_t = parent.GetChild(i);

                if (child_t.name == targetName)
                {
                    transform = child_t;
                    return transform;
                }
                else
                {
                    if (debug) Debug.Log("Go down one level " + child_t.name + " " + level);
                    Transform temp = targetSearchDF(child_t, targetName, level++, debug);
                    if (temp != null)
                    {
                        return temp;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Assigns recursively a joint struct to a transform of the skeleton in the scene based on a mapping defined in a JointDesc instance.
        /// </summary>
        /// <param name="jointDesc">Contains name of the joint in the source and target skeleton</param>
        /// <param name="skeletonDesc">Has joint sequence property that is needed to look up the JointDesc structures of the child joints.</param>
        /// <param name="parent">"Root transform of the skeleton in the scene"</param>
        public void assignJointTransformFromDesc(JointDesc jointDesc, SkeletonDesc skeletonDesc, Transform parent)
        {

            name = jointDesc.name;
            if (jointDesc.targetName != "none")
            {
                transform = targetSearchDF(parent, jointDesc.targetName, 0, false);
                if (transform != null)
                {
                    Debug.Log("Assigned " + name + " to " + transform.name);

                }
                else
                {
                    Debug.Log("Could not assign " + name);
                }
            }
            else // skip joints without target
            {
                Debug.Log("Ignore " + name);
            }


            foreach (string name in jointDesc.children)
            {
               
                JointDesc childDesc = findJointDesc(skeletonDesc.jointDescs, name);
                if(childDesc == null)continue;

                MGJoint childJoint = new MGJoint();
                childJoint.assignJointTransformFromDesc(childDesc, skeletonDesc, parent);
                children.Add(childJoint);
            }
        }

        public GameObject createGameObjectfromDesc(JointDesc jointDesc, SkeletonDesc skeletonDesc, GameObject parent, string skeletonJointTag="SKELETON_JOINT", GameObject prefab=null, float scale=1.0f)
        {
            name = jointDesc.name;
            GameObject jointObj = new GameObject();
        
            jointObj.tag = skeletonJointTag;
            var q = new Quaternion();
            float magnitude = 0;
            float prefabScale = 1f;
            if (prefab!=null && jointDesc.children.Length > 0){
                var REF_VECTOR = new Vector3(0,0,1);
                foreach(var name in jointDesc.children){
                    JointDesc childDesc = findJointDesc(skeletonDesc.jointDescs, name);
                    if(childDesc == null)continue;
                    //int idx = Array.IndexOf(skeletonDesc.jointSequence, name);
                    //JointDesc childDesc = skeletonDesc.jointDescs[idx];
                    var offset = new Vector3(childDesc.offset[0], childDesc.offset[1],childDesc.offset[2]);
                    if (offset.magnitude > 0 ){
                        q = Quaternion.FromToRotation(REF_VECTOR, offset.normalized);
                        magnitude = offset.magnitude;
                        float heightScale = magnitude/2;
                        GameObject visObject = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), q);
                        visObject.transform.localScale = new Vector3(prefabScale, prefabScale, heightScale*prefabScale);
                        visObject.transform.parent = jointObj.transform;
                    }
                }
            }
            
            jointObj.transform.parent = parent.transform;
            jointObj.name = name;
            jointObj.transform.localPosition = new Vector3(jointDesc.offset[0]*scale, jointDesc.offset[1]*scale,jointDesc.offset[2]*scale);
            if (jointObj != null)
            {
                transform = jointObj.transform;

                foreach (string name in jointDesc.children)
                {
                    
                    JointDesc childDesc = findJointDesc(skeletonDesc.jointDescs, name);
                    if(childDesc == null)continue;

                    MGJoint childJoint = new MGJoint();
                    childJoint.createGameObjectfromDesc(childDesc, skeletonDesc, jointObj, skeletonJointTag, prefab, scale);
                    children.Add(childJoint);
                }
            }
            return jointObj;
        }
        public GameObject createGameObjectfromDescOld(JointDesc jointDesc, SkeletonDesc skeletonDesc, GameObject parent, string skeletonJointTag="SKELETON_JOINT", GameObject prefab=null)
        {
            name = jointDesc.name;
            GameObject jointObj;
            if (prefab==null){
                jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }else{
                jointObj = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            }
            jointObj.tag = skeletonJointTag;
            var q = new Quaternion();
            float magnitude = 0;
            float scale = 1f;//0.06f;// 

            Mesh mesh = jointObj.GetComponent<MeshFilter>().mesh;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                var v = mesh.vertices[i];
                Vector3 n = new Vector3(v.x, v.y, v.z);
                vertices.Add(n);
                v = mesh.normals[i];
                n = new Vector3(v.x, v.y, v.z);
                normals.Add(n);
            }
            if (jointDesc.children.Length > 0){
                var REF_VECTOR = new Vector3(0,0,1);
               
                foreach(var name in jointDesc.children){
                    JointDesc childDesc = findJointDesc(skeletonDesc.jointDescs, name);
                    if(childDesc == null)continue;
                    var offset = new Vector3(childDesc.offset[0], childDesc.offset[1],childDesc.offset[2]);
                    if (offset.magnitude > 0 ){
                        q = Quaternion.FromToRotation(REF_VECTOR, offset.normalized);
                        magnitude = offset.magnitude;
                    }else{
                        scale = 0.00001f;
                    }

                    float heightScale = magnitude/2;
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        var v = vertices[i];
                        Vector3 n = new Vector3(v.x * scale, v.y * scale, v.z * scale*heightScale );
                        n = q * n;
                        newVertices.Add(n);
                        v = mesh.normals[i];
                        n = new Vector3(v.x, v.y, v.z);
                        n = q * n;
                        newNormals.Add(n.normalized);
                    }
                }
            }
         
            jointObj.GetComponent<MeshFilter>().mesh.vertices = newVertices.ToArray();
            jointObj.GetComponent<MeshFilter>().mesh.normals = newNormals.ToArray();
            
            jointObj.transform.parent = parent.transform;
            jointObj.name = name;
            if (jointObj != null)
            {
                transform = jointObj.transform;

                foreach (string name in jointDesc.children)
                {
                    
                    JointDesc childDesc = findJointDesc(skeletonDesc.jointDescs, name);
                    if(childDesc == null)continue;

                    MGJoint childJoint = new MGJoint();
                    childJoint.createGameObjectfromDescOld(childDesc, skeletonDesc, jointObj, skeletonJointTag, prefab);
                    children.Add(childJoint);
                }
            }
            return jointObj;
        }


        public void getJointAngles(List<Vector3> angles)
        {
            Vector3 localEulerAngles = transform.localEulerAngles;
            angles.Add(localEulerAngles);
            foreach (MGJoint childJoint in children)
            {
                if (childJoint.transform != null)
                {
                    childJoint.getJointAngles(angles);
                }
            }
        }

        public void getJointPositions(List<Vector3> positions)
        {
            Vector3 jointPosition = transform.position;
            foreach (MGJoint childJoint in children)
            {
                if (childJoint.transform != null)
                {
                    Vector3 childPosition = childJoint.transform.position;
                    positions.Add(jointPosition);
                    positions.Add(childPosition);
                    childJoint.getJointPositions(positions);
                    positions.Add(childPosition);
                    positions.Add(jointPosition);
                }
            }
        }
    }
}