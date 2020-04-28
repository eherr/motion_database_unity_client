using System.Collections;
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
    public CameraController cameraController;

    public string sourceSkeletonModel;
    public bool initialized;
    bool centerCamera = false;
    public bool useMesh = false;
    public int modelIndex;
    
    List<GameObject> generatedObjects = new List<GameObject>();


    // Use this for initialization
    void Start()
    {
        modelIndex = 0;
        userInteraction = false;
        initialized = false;
        centerCamera = false;
        useMesh = false;
        motionDatabase.OnNewAvatarList += fillAvatarList;
        motionDatabase.GetAvatarList();

        //https://www.tangledrealitystudios.com/development-tips/prevent-unity-webgl-from-stopping-all-keyboard-input/
#if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
#endif
#if UNITY_EDITOR
        GetSkeleton();
#endif

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

    }

    public void OnEndSliderDrag()
    {
        userInteraction = false;
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

    public void OnChangeModel(){
        loadAvatar();
    }
    public void loadAvatar()
    {
        int newModelIdx = modelDropdown.value;
        if (newModelIdx >= 0 && newModelIdx < motionDatabase.avatars.Count)
        {
            modelIndex = newModelIdx;
            GetSkeleton();
            motionDatabase.ClearGeneratedObjects();
            motionDatabase.LoadAvatar(motionDatabase.avatars[modelIndex].name);
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
            motionDatabase.meshToggle.SetIsOnWithoutNotify(useMesh);
            return;
        }
        if (motionDatabase.avatars.Count > 0)
        {
            useMesh = !useMesh;
        }else { 
            useMesh = false;
        }
        motionDatabase.meshToggle.SetIsOnWithoutNotify(useMesh);
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

        motionDatabase.GetSkeleton(sourceSkeletonModel);
    
    }

    public void SetSourceSkeleton(string name){
        Debug.Log("Set source skeleton "+name);
        if (name != sourceSkeletonModel || !initialized){
            sourceSkeletonModel = name;
            Debug.Log("update skeleton from server");
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
       SceneManager.LoadScene("websocket_client");
    }
    
}
