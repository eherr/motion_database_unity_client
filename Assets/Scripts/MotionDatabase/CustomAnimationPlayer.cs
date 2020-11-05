using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MotionDatabase { 
    public class CustomAnimationPlayer : MonoBehaviour
    {
   
        public bool playAnimation = false;
        public CAnimationClip motion = null;
        public List<List<int>> frameLabels;
        public List<string> labels;
        public CAnnotation annotation;
        public CLegacyAnnotation legacyAnnotation;
        public int frameIdx = 0;
        public Transform rootTransform;
        public GameObject agentGeometry;
        public MGJoint root = null;
        public bool loop = false;
        public float frameTime = 0.013889f;
        [Range(0, 4.0f)]
        public float speedFactor = 1.0f;
        public float scaleFactor = 0.01f;
        public float animationTime = 0.0f;
        public float maxAnimatTime = 1.0f;
        public bool init = false;
        protected Dictionary<string, int> indexMap;
        private bool showMesh = false;
        public GameObject skeleton;
        List<MeshRenderer> rootMeshes;
        List<SkinnedMeshRenderer> agentMeshes;
        SkeletonDesc skeletonDesc;
        public string skeletonModel;
        int numFrames = 0;
        public SkeletonManager skeletonManager;
        string skeletonJointTag = "SKELETON_JOINT";
        public void Start () {
            rootMeshes = new List<MeshRenderer>();
            agentMeshes = new List<SkinnedMeshRenderer>();
            if (rootTransform != null) { 
                var _rootMeshes = rootTransform.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer s in _rootMeshes)
                {
                    rootMeshes.Add(s);
                }
                var _agentMeshes = agentGeometry.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer m in _agentMeshes)
                {
                    agentMeshes.Add(m);
                }
            }
        }

        public void Update()
        {
            if (init && root != null && motion != null)
            {

                frameIdx = (int)(animationTime / frameTime);
                if (frameIdx >= numFrames) { 
                    frameIdx = numFrames-1;
                }
                Vector3 pos = motion.getRootTranslation(frameIdx);
                if (rootTransform != null)
                {
                    var rootDesc = skeletonDesc.jointDescs[0];
                    Vector3 offset = new Vector3(rootDesc.offset[0],rootDesc.offset[1],rootDesc.offset[2]);
                    rootTransform.position = pos + offset*scaleFactor;
                }
                root.setPose(motion.getPose(frameIdx), indexMap);
              
                if (playAnimation)
                {
                    animationTime += Time.deltaTime * speedFactor;
                    if (loop)
                    {
                        animationTime %= maxAnimatTime;
                    }
                    else
                    {
                        if (animationTime >= maxAnimatTime)
                        {
                            playAnimation = false;
                            animationTime = maxAnimatTime;
                        }
                    }
                }
            }

        }
                
        ///https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public void ProcessMotionBytes(byte[] motionBytes, Vector3 offset, bool startAnimation=true)
        {
            annotation = null;
            legacyAnnotation = null;
            Stream stream = new MemoryStream(motionBytes);
            BsonReader reader = new BsonReader(stream);
            JsonSerializer serializer = new JsonSerializer();
            motion = serializer.Deserialize<CAnimationClip>(reader);
            motion.scaleTranslations(scaleFactor);
            motion.translateFrames(offset);
            frameTime = motion.getFrameTime();
            animationTime = 0;
            frameIdx = 0;
            speedFactor = 1.0f;
            playAnimation = startAnimation;
            numFrames = motion.GetNumFrames();
            maxAnimatTime = numFrames * frameTime;
            init = true;
        }
        public void ProcessMotionString(string motionString, Vector3 offset, bool startAnimation=true)
        {

            string oldSkeletonModel = motion.skeletonModel;
            
            motion = JsonConvert.DeserializeObject<CAnimationClip>(motionString);
            motion.scaleTranslations(scaleFactor);
            motion.translateFrames(offset);
            frameTime = motion.getFrameTime();
            animationTime = 0;
            frameIdx = 0;
            speedFactor = 1.0f;
            playAnimation = startAnimation;
            numFrames = motion.GetNumFrames();
            maxAnimatTime = numFrames * frameTime;
            init = true;
            
        }

        /// <summary>
        /// Process JSON string into CAnnotation or CLegacyAnnotation and visualize result.
        /// </summary>
        /// <param name="annotationSring"> JSON string </param>
        public void ProcessAnnotationString(string annotationSring)
        {


            if (!init) return;
            JsonSerializer serializer = new JsonSerializer();
            JsonReader reader = new JsonTextReader(new StringReader(annotationSring));
            bool success = false;
            try
            {
                Debug.Log("process legacy annotation format");
                legacyAnnotation = serializer.Deserialize<CLegacyAnnotation>(reader);


                frameLabels = new List<List<int>>();
                labels = new List<string>();
                for (int j = 0; j < legacyAnnotation.sections.Count; j++) {

                    labels.Add("c" + j.ToString());
                }
               numFrames = motion.GetNumFrames();

                for (int i =0; i < numFrames; i++) {
                    frameLabels.Add(new List<int>());
                    for (int j = 0; j < legacyAnnotation.sections.Count; j++)
                    {

                        if (legacyAnnotation.sections[j].start_idx < i && i < legacyAnnotation.sections[j].end_idx)
                        {
                            frameLabels[i].Add(j);//store index of label
                        }
                    }
                }
                success = true;
            }
            catch
            {
                Debug.Log("could not process legacy annotation format");
            }
            if (success)
            {
                return;
            }
            
            try
            {
                Debug.Log("process annotation format");
                annotation = serializer.Deserialize<CAnnotation>(reader);

                Debug.Log(annotation);
                numFrames = motion.GetNumFrames();
                frameLabels = new List<List<int>>();
                labels = new List<string>();
                foreach (var label in annotation.sections.Keys)
                {
                    labels.Add(label);
                }
                numFrames = motion.GetNumFrames();

                for (int i = 0; i < numFrames; i++)
                {
                    frameLabels.Add(new List<int>());
                    foreach (var label in labels)
                    {
                        for (int j = 0; j < annotation.sections[label].Count; j++)
                        {
                            if (annotation.sections[label][j].start_idx < i && i < annotation.sections[label][j].end_idx)
                            {
                                frameLabels[i].Add(j);//store index of label
                            }
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("could not process annotation format");
            }

        }


        public void clearMotion(){
            motion = null;
            animationTime = 0;
            frameIdx = 0;
            speedFactor = 1.0f;
            playAnimation = false;
            numFrames = 0;
            maxAnimatTime = 0;
            init = false;
        }

        public void ToggleAnimation()
        {
            playAnimation = !playAnimation;
        }

        public void ShowMesh()
        {

            foreach (MeshRenderer s in rootMeshes)
            {
                s.enabled = true;
            }

            foreach (SkinnedMeshRenderer s in agentMeshes)
            {
                s.enabled = true;
            }
            showMesh = true;
        }


        public void HideMesh()
        {
            foreach (MeshRenderer s in rootMeshes)
            {
                s.enabled = false;
            }

            foreach (SkinnedMeshRenderer s in agentMeshes)
            {
                s.enabled = false;
            }
            showMesh = false;

        }
        public void DestroySkeleton()
        {
            if (skeletonManager != null && skeletonDesc != null){
                skeletonManager.HideSkeleton(skeletonDesc.name);
            }else{
                if (skeleton != null) {
                    destroyHierarchy(skeleton.transform);
                    skeleton= null;
                }
                GameObject[] skeletonJoints = GameObject.FindGameObjectsWithTag(skeletonJointTag);
                foreach(GameObject j in skeletonJoints){
                    Destroy(j);
                }
           }
        }


        public void destroyHierarchy(Transform node)
        {
            for(int idx = 0; idx < node.childCount; idx++){
                var child = node.GetChild(idx);
                destroyHierarchy(child);
            }
            Destroy(node.gameObject);
        }

        public void ProcessSkeletonString(string skeletonString)
        {
            if (skeletonManager != null) skeletonManager.HideSkeletons();
            skeletonDesc = JsonUtility.FromJson<SkeletonDesc>(skeletonString);

            buildPoseParameterIndexMap(skeletonDesc);
            SetupSkeleton();
        }

        public void SetupSkeleton()
        {
            if (skeletonDesc == null) return;

            if (showMesh)
            {
                if (skeleton != null) DestroySkeleton();
                skeletonDesc.referencePose.ScaleTranslations(scaleFactor);
                initSkeletonFromExistingCharacter(rootTransform, skeletonDesc);

                Debug.Log("init skeleton from existing character");
            }
            else{
                skeletonDesc.referencePose.ScaleTranslations(scaleFactor);
           
                skeleton = skeletonManager.GetSkeleton(skeletonDesc);
                rootTransform = skeleton.transform;
                root = skeletonManager.GetRootJoint(skeletonDesc);
                skeletonManager.ShowSkeleton(skeletonDesc.name); 
                root.setPose(skeletonDesc.referencePose, indexMap);
                if(skeletonDesc.referencePose.translations.Length > 0 && skeletonDesc.jointDescs.Length > 0){
                    var t = skeletonDesc.referencePose.translations[0];
                    var rootDesc = skeletonDesc.jointDescs[0];
                    Vector3 offset = new Vector3(rootDesc.offset[0],rootDesc.offset[1],rootDesc.offset[2]);
                    rootTransform.position = t + offset*scaleFactor;
                }
                Debug.Log("generated skeleton");
            }
        }
        public void initSkeletonFromExistingCharacter(Transform rootTransform, SkeletonDesc skeletonDesc)
        {
            if (rootTransform != null) { 
                JointDesc jointDesc = skeletonDesc.jointDescs[0];
                root = new MGJoint();
                root.assignJointTransformFromDesc(jointDesc, skeletonDesc, rootTransform);
            }else
            {
                Debug.Log("no root transform defined");
            }
        }

        public void createDebugSkeleton(SkeletonDesc skeletonDesc)
        {

            JointDesc jointDesc = skeletonDesc.jointDescs[0];
            root = new MGJoint();
            skeleton = root.createGameObjectfromDesc(jointDesc, skeletonDesc, gameObject, skeletonJointTag);
        }


        protected void buildPoseParameterIndexMap(SkeletonDesc skeletonDesc)
        {
            int counter = 0;
            indexMap = new Dictionary<string, int>();
            foreach (string name in skeletonDesc.jointSequence)
            {
                indexMap[name] = counter;
                counter++;
            }
        }

        

        public List<Vector3> GetJointAngles()
        {
            List<Vector3> angles = new List<Vector3>();
            root.getJointAngles(angles);
            return angles;
        }
        public int GetNumFrames()
        {
            if (motion != null)
            {
                return numFrames;
            }else
            {
                return 0;
            }
        }

        public void SetCurrentFrame(int newFrameIdx)
        {
            animationTime = newFrameIdx * frameTime;
            animationTime = Math.Min(animationTime, maxAnimatTime);
            frameIdx =(int)( animationTime / frameTime);
        }

        public string GetClipTitle()
        {
            if (motion != null)
            {
                return motion.clipTitle;
            }
            else
            {
                return "n/a";
            }
        }

        public void SetAvatarMesh(Transform newRoot, GameObject newGeometry)
        {
            HideMesh();
            rootTransform = newRoot;
            agentGeometry = newGeometry;
            rootMeshes.Clear();
            agentMeshes.Clear();
            if (rootTransform != null)
            {
                var _rootMeshes = rootTransform.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer s in _rootMeshes)
                {
                    rootMeshes.Add(s);
                }
                if (agentGeometry != null) { 
                var _agentMeshes = agentGeometry.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer m in _agentMeshes)
                    {
                        agentMeshes.Add(m);
                    }
                }
                ShowMesh();
            }

        }

    }

}
 