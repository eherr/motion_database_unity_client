﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MotionDatabase;
using UnityEngine.SceneManagement;
using Siccity.GLTFUtility;

[System.Serializable]
public class AvatarDefinition
{
    public string name;
    public string skeletonType;
    
}

public class RESTGUIManager : MonoBehaviour {

    public MotionDatabaseInterface motionDatabase;
    public bool userInteraction;
    public Text animationTitle;
    public Text frameCountText;
    public GameObject modelPanel;
    public GameObject settingsPanel;

    public Dropdown modelDropdown;
    public Toggle meshToggle;
    public CameraController cameraController;

    public string skeletonType;
    public bool initialized;
    bool centerCamera = false;
    public bool useMesh = false;
    public int modelIndex;
    public bool showAnnotation = false;
    List<GameObject> generatedObjects = new List<GameObject>();
    GUIStyle style = new GUIStyle();
    Texture2D whiteTexture;
    // Use this for initialization
    void Start()
    {
        modelIndex = 0;
        userInteraction = false;
        initialized = false;
        centerCamera = false;
        useMesh = false;
        showAnnotation = false;
        motionDatabase.OnNewAvatarList += fillAvatarList;
        motionDatabase.GetAvatarList(skeletonType);

        style.alignment = TextAnchor.MiddleCenter;
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        bool loadSkeleton = true;
        //https://www.tangledrealitystudios.com/development-tips/prevent-unity-webgl-from-stopping-all-keyboard-input/
#if !UNITY_EDITOR && UNITY_WEBGL
         WebGLInput.captureAllKeyboardInput = false;
        loadSkeleton = false;
#endif

        if(loadSkeleton)GetSkeleton();

    }

    // Update is called once per frame
    void Update()
    {
        if (motionDatabase.player == null) return;
        var slider = GetComponentInChildren<Slider>();
        if (userInteraction)
        {
            motionDatabase.player.SetCurrentFrame((int)slider.value);
            int currentFrame = motionDatabase.player.frameIdx;
            int nFrames = motionDatabase.player.GetNumFrames();
            frameCountText.text = "Frame: " + currentFrame.ToString() + "/" + nFrames.ToString();
            animationTitle.text = motionDatabase.player.GetClipTitle();
        }
        else
        {
            int currentFrame = motionDatabase.player.frameIdx;
            int nFrames = motionDatabase.player.GetNumFrames();
            frameCountText.text = "Frame: " + currentFrame.ToString() + "/" + nFrames.ToString();
            animationTitle.text = motionDatabase.player.GetClipTitle();

            slider.maxValue = nFrames;
            slider.value = currentFrame;
        }
    }

    public void OnBeginSliderDrag()
    {
        userInteraction = true;
        if (cameraController != null) cameraController.Active = false;

    }

    public void OnEndSliderDrag()
    {
        userInteraction = false;
        if (cameraController != null) cameraController.Active = true;
    }

    public bool IsPlaying(){
        if (motionDatabase.player != null){
            return false;
        }else{
            return motionDatabase.player.playAnimation;
        }
    }

    public void ToggleAnimation()
    {
         motionDatabase.ToggleAnimation();
    }

    public void ToggleModelPanel()
    {
        if (!modelPanel.activeInHierarchy && settingsPanel.activeInHierarchy) // close settings first
        {
            settingsPanel.SetActive(false);
        }
        modelPanel.SetActive(!modelPanel.activeInHierarchy);
    }

    public void ToggleSettingsPanel()
    {
        if (!settingsPanel.activeInHierarchy && modelPanel.activeInHierarchy) // close model first
        {
            modelPanel.SetActive(false);
        }
        settingsPanel.SetActive(!settingsPanel.activeInHierarchy);
    }

    public void GetMotion()
    {

        motionDatabase.GetMotion();
    }

    public void OnChangeModel()
    {
        useMesh = true;
        loadAvatar();
        meshToggle.SetIsOnWithoutNotify(useMesh);
    }
    public void loadAvatar()
    {
        int newModelIdx = modelDropdown.value;
        if (newModelIdx >= 0 && newModelIdx < motionDatabase.avatars.Count)
        {
            modelIndex = newModelIdx;
            GetSkeleton();
            motionDatabase.ClearGeneratedObjects();
            motionDatabase.LoadAvatar(motionDatabase.avatars[modelIndex].name, skeletonType);
        }
    }
   
    public void fillAvatarList()
    {
        modelDropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        foreach (var a in motionDatabase.avatars)
        {
            var o = new Dropdown.OptionData();
            o.text = a.name;
            options.Add(o);
        }
        modelDropdown.AddOptions(options);
    }


    public void ToggleMesh()
    {
        if (motionDatabase.waitingForSkeleton)
        {
            Debug.Log("waiting" );
            meshToggle.SetIsOnWithoutNotify(useMesh);
            return;
        }
        if (motionDatabase.avatars.Count > 0)
        {
            useMesh = !useMesh;
        }else { 
            useMesh = false;
        }
        meshToggle.SetIsOnWithoutNotify(useMesh);
        Debug.Log("use mesh"+ useMesh.ToString());
        if (!useMesh) { 
            motionDatabase.ToggleAnimation();
            motionDatabase.player.SetAvatarMesh(null, null);
            motionDatabase.ClearGeneratedObjects();
            GetSkeleton();
        }else
        {
            loadAvatar();
        }
    }


    public void GetSkeleton()
    { 

        motionDatabase.GetSkeleton(skeletonType);
    
    }

    public void SetSourceSkeleton(string name){
        useMesh = false;
        meshToggle.SetIsOnWithoutNotify(useMesh);
        motionDatabase.ToggleAnimation();
        motionDatabase.player.SetAvatarMesh(null, null);
        motionDatabase.ClearGeneratedObjects();

        Debug.Log("Set source skeleton "+name);
        if (name != skeletonType || !initialized){
            skeletonType = name;
            Debug.Log("update skeleton from server");
            motionDatabase.GetAvatarList(skeletonType);
            GetSkeleton();
            initialized = true;
        }
    }


   public void GetMotionByID(string clipID)
    {
       motionDatabase.GetMotionByID(clipID);
    }


    public void GetRandomSample(string modelID)
    {
        motionDatabase.GetRandomSample(modelID);
    }

    public void SetPort(int newPort)
    {
        motionDatabase.SetPort(newPort);
    }

    public void TogglePortWorkaround()
    {
         motionDatabase.TogglePortWorkaround();
    }

    public void EnableCamera(){
        if (cameraController != null) cameraController.gameObject.SetActive(true);
        Debug.Log("Enable Camera");
    }


    public void DisableCamera(){
        if (cameraController != null) cameraController.gameObject.SetActive(false);
        Debug.Log("Disable Camera");
        
    }

    public void ToggleCenterCamera()
    {
        var root = motionDatabase.player.root;
        centerCamera = !centerCamera &&  root!= null;
        if (centerCamera)
        {
            var cameraTarget = root.transform;
            
            Debug.Log("set target");
            cameraController.cameraTarget = cameraTarget;
        }
        else
        {

            cameraController.cameraTarget = null;
            Debug.Log("remove target");
        }

    }

    void SetURL(string newURL){

        motionDatabase.SetURL(newURL);
    }
    
    public void SetProtocol(string newProtocol){
        motionDatabase.SetProtocol(newProtocol);
    }
    
    void LoadScene(string clipID)
    {
      
    }

    void OnGUI()
    {
        if (showAnnotation)DrawAnnotation();

    }

    void DrawAnnotation()
    {

        //backround
        float annotationDisplayHeight = 50f;
        float annotationDisplayWidth = Screen.width;
        float startX = 0;
        float startY = 0.8f* Screen.height;
        DrawRectangle(new Rect(startX, startY, Screen.width, annotationDisplayHeight), new Color(0,0,0));
        //annotation labels
        float labelWidth = 0.05f*Screen.width;


        float boxWidth = 10;//SET BY ZOOM Mathf.Max(timeLineWidth / nFrames, 1f);
        float timeLineWidth = annotationDisplayWidth - labelWidth;
        int nFrames = motionDatabase.player.GetNumFrames();
        if (nFrames <= 0) return;
        if (motionDatabase.player.frameLabels == null) return;
        //Debug.Log("n frames" + nFrames.ToString() + " "+ timeLineWidth.ToString()+" "+ boxWidth.ToString());
        int nCategories = Mathf.Max(motionDatabase.player.labels.Count, 1);
        float boxHeight = annotationDisplayHeight / nCategories;

        Color frameMarkerColor = new Color(0, 0, 1);
        Color emptyColor = new Color(1, 0, 0);
        Color labelBackColor = new Color(0, 1, 0);
        float yPixelOffset = 0;
        GUIContent content;
        for (int j = 0; j < nCategories; j++)
        {
            var pos = new Rect(startX, startY + yPixelOffset, labelWidth, boxHeight);
            DrawRectangle(pos, labelBackColor);
            content = new GUIContent(motionDatabase.player.labels[j], whiteTexture, motionDatabase.player.labels[j]);

            // Position the Text and Texture in the center of the box
            style.alignment = TextAnchor.MiddleLeft;

            GUI.Box(pos, content, style);
     
        yPixelOffset += boxHeight;
        }


        //annotation box
        startX = labelWidth;
        Debug.Log("n frames" + nFrames.ToString() + " "+ nCategories.ToString());
        float wMargin = Mathf.Max(0.1f * boxWidth, 1f);
        float hMargin = Mathf.Max(0.1f * annotationDisplayHeight, 1f);
        //always display the same amount of frames based on the box size
        //move the window of frames that are displayed based on the current frame
        int nDisplayedFrames = (int)(timeLineWidth/boxWidth);
        int frameWindowSize = nDisplayedFrames / 2;
        int start = 0;
        if (motionDatabase.player.frameIdx < nFrames - frameWindowSize) { 
            start = Mathf.Max(motionDatabase.player.frameIdx - frameWindowSize, 0);
        }
        else //set start in case end is reached
        {
            start = Mathf.Max(nFrames - nDisplayedFrames,0); 
        }


        float xPixelOffset = 0;
        yPixelOffset = 0;
        int end = Mathf.Min(start + nDisplayedFrames, nFrames);
        for (int i = start; i < end; i++)
        {
            yPixelOffset = 0;
            for (int j = 0; j < nCategories; j++) {
                if (i < motionDatabase.player.frameLabels.Count && motionDatabase.player.frameLabels[i].Contains(j)) { 
                    DrawRectangle(new Rect(startX + xPixelOffset + wMargin, startY + yPixelOffset + hMargin, boxWidth - wMargin, boxHeight - hMargin), emptyColor);
                }

                yPixelOffset += boxHeight;
            }
            if (i == motionDatabase.player.frameIdx) {
                DrawScreenRectBorder(new Rect(startX + xPixelOffset + wMargin, startY + hMargin, boxWidth - wMargin, annotationDisplayHeight - hMargin), wMargin, frameMarkerColor);
            }
            xPixelOffset += boxWidth;

        }

    }

    /// <summary>
    /// https://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/
    /// https://docs.unity3d.com/ScriptReference/GL.QUADS.html
    /// https://docs.unity3d.com/ScriptReference/GUI.Box.html
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    void DrawRectangle(Rect position, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(position, whiteTexture);
        GUI.color = Color.white;
    }
    /// <summary>
    ///  https://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="thickness"></param>
    /// <param name="color"></param>
    public void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawRectangle(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawRectangle(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawRectangle(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawRectangle(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public void ToggleAnnotation()
    {
        showAnnotation = !showAnnotation;
    }
}