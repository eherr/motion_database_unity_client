﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Siccity.GLTFUtility;


namespace MotionDatabase
{

    [RequireComponent(typeof(CustomAnimationPlayer))]
    public class MotionDatabaseInterface : RESTInterface
    {
        public string clipID;
        public CanvasGroup loadIcon;
        public CustomAnimationPlayer player;
        public List<AvatarDefinition> avatars;
        private bool isLoading;
        List<GameObject> generatedObjects = new List<GameObject>();
        public bool waitingForSkeleton = false;
        public delegate void dgEventRaiser();
        public event dgEventRaiser OnNewAvatarList;
        void Start()
        {
            player = GetComponent<CustomAnimationPlayer>();
            isLoading = false;
            waitingForSkeleton = false;
        }


        void handleSkeleton(string response)
        {
            
            player.ProcessSkeletonString(response);
            waitingForSkeleton = false;
        }


        protected void handleMotion(byte[] response)
        {
          player.ProcessMotionBytes(response, Vector3.zero);
          GetAnnotation();


        }
        protected void handleAnnotation(string response)
        {
            Debug.Log("received annotation");
            player.ProcessAnnotationString(response);


        }


        public void GetSkeleton(string skeletonType="")
        {
            if (player != null);
                player.clearMotion();
            print("Get skeleton");
            var message = "{}";
            if (skeletonType!= "") { 
                message = "{ \"skeleton_type\": \"" + skeletonType + "\"}";
            }
            waitingForSkeleton = true;
            StartCoroutine(LoadAndRequest("get_skeleton", message, handleSkeleton));
            
        }


        public void GetMotion()
        {
            var message = "{ \"clip_id\":"+clipID+"}";
            StartCoroutine(LoadAndRequestBytes("get_motion", message, handleMotion));
        }

        public void GetMotionByID(string clipID)
        {
            print("Get motion by id" + clipID);
            var message = "{ \"clip_id\":" + clipID + "}";
            this.clipID = clipID;
            StartCoroutine(LoadAndRequestBytes("get_motion", message, handleMotion));
        }


        public void GetRandomSample(string modelID)
        {
            print("Get random sample of model " + modelID);
            var message = "{ \"model_id\": \"" + modelID + "\"}";
            StartCoroutine(LoadAndRequestBytes("get_sample", message, handleMotion));
        }

        public void LoadAvatar(string name)
        {
            print("Get avatar " + name);
            var message = name;// "{ \"name\": \"" + name + "\"}";
            StartCoroutine(LoadAndRequestBytes("get_binary", message, handleAvatar));
        }

        public void GetAvatarList()
        {
            print("Get avatar list");
            StartCoroutine(GetRequest("get_GLB_list", handleAvatarList));
        }

        public void GetAnnotation()
        {

            print("Get annoation by id" + clipID);
            var message = "{ \"clip_id\":" + clipID + "}";
            StartCoroutine(LoadAndRequest("download_annotation", message, handleAnnotation));
        }

        public void SetPort(int newPort)
        {
            port = newPort;
            usePort = port >= 0;
        }

        public void SetURL(string newURL){
              url = newURL;
        }
        
        public void SetProtocol(string newProtocol){
              protocol = newProtocol;
        }

        public void SetPortWorkAround(bool enable)
        {
            usePortWorkAround = enable;
        }
        
       public void TogglePortWorkaround()
        {
            usePortWorkAround = !usePortWorkAround;
        }
        public void ToggleAnimation()
        {
            player.ToggleAnimation();
        }



        public void Connect(string newURL)
        {
            print("Connect to " + newURL + " with port " + port.ToString());
            print("use port" + usePort.ToString());
            url = newURL;
            // port = newPort;
            GetSkeleton();
        }

        protected IEnumerator GetRequest(string method, System.Action<string> callback)
        {
            Debug.Log("GET request!");
            yield return StartCoroutine(sendGETRequestCoroutine(method, callback));
        }


        protected IEnumerator LoadAndRequest(string method, string messageBody, PostRequestCallback callback)
        {
            Debug.Log("POST request!");
            yield return StartCoroutine(Fade(1f));
            loadIcon.blocksRaycasts = true;

            yield return StartCoroutine(sendRequestCoroutine(method, messageBody, callback));

            loadIcon.blocksRaycasts = false;
            yield return StartCoroutine(Fade(0f));
        }

        protected IEnumerator LoadAndRequestBytes(string method, string messageBody, BytePostRequestCallback callback)
        {
            Debug.Log("Loading request!");
            yield return StartCoroutine(Fade(1f));
            loadIcon.blocksRaycasts = true;

            yield return StartCoroutine(sendRequestCoroutine(method, messageBody, callback));

            loadIcon.blocksRaycasts = false;
            yield return StartCoroutine(Fade(0f));
        }

        protected IEnumerator Fade(float finalAlpha)
        {
            isLoading = true;
            float fadeDuration = 2f;
            float fadeSpeed = Mathf.Abs(loadIcon.alpha - finalAlpha) / fadeDuration;

            while(!Mathf.Approximately(loadIcon.alpha, finalAlpha))
            {
                loadIcon.alpha = Mathf.MoveTowards(loadIcon.alpha, finalAlpha, fadeDuration * Time.deltaTime);
                yield return null; //wait 1 frame
            }

            isLoading = false;
            Debug.Log("Finish Fade");
        }
 
        protected void handleAvatar(byte[] response)
        {
            var model = Importer.LoadFromBytes(response);
            generatedObjects.Add(model);
            model.transform.localRotation = Quaternion.identity;

            player.SetAvatarMesh(model.transform, null);
            if (player != null)
            {
                player.playAnimation = false;
            }

            player.ShowMesh();
            player.SetupSkeleton();
            waitingForSkeleton = false;
        }

        public void ClearGeneratedObjects()
        {

            Debug.Log(generatedObjects.Count);
            for (int i = 0; i < generatedObjects.Count; ++i)
            {
                if (generatedObjects[i] != null)
                {
                    Destroy(generatedObjects[i]);
                }
            }
            generatedObjects.Clear();
        }

        void handleAvatarList(string stringArray)
        {

            string[] words = stringArray.Split('"');

            for (int i = 1; i < words.Length; i = i + 2)
            {
                string word = words[i];
                avatars.Add(new AvatarDefinition { name = word, skeletonType = "mh_cmu" });
            }
            OnNewAvatarList();

        }
    }

}