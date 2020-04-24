using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MotionDatabaseInterface;
using UnityEngine.SceneManagement;
using Siccity.GLTFUtility;

[System.Serializable]
public class AvatarDefinition
{
    public string name;
    public string skeletonType;
  //  public Transform rootTransform;
   // public GameObject geometry;
    
}

public class RESTGUIManager : MonoBehaviour {
    public string protocol;
    public int port;
    public string url;
    public bool usePortWorkAround;
    public CustomAnimationPlayerInterface animationPlayer;
    public List<AvatarDefinition> avatars;
  //  public List<string> avatars;
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
    public int modelIndex;
    
    public string method1 = "get_GLB_list";
    public string method2 = "get_binary";
    GameObject abc = null;
    List<GameObject> generatedObjects = new List<GameObject>();
    // Use this for initialization
    void Start()
    {
        modelIndex = 0;
        userInteraction = false;
        animationPlayer.SetPort(port);
        animationPlayer.SetURL(url);
        animationPlayer.SetPortWorkAround(usePortWorkAround);
        initialized = false;
        centerCamera = false;
        string WEB_URL = animationPlayer.protocol + "://" + animationPlayer.url + ":" + animationPlayer.port + "/";
        Debug.Log(WEB_URL);
        //https://www.tangledrealitystudios.com/development-tips/prevent-unity-webgl-from-stopping-all-keyboard-input/
        string methodURL1 = WEB_URL + method1;
		string methodURL2 = WEB_URL + method2;
		string temp = "";
        
        #if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
        #endif
       
        #if UNITY_EDITOR
       // GetSkeleton();
       StartCoroutine(CallRestAPI.Instance.Get(methodURL1, (stringArray) =>
       {
           temp = stringArray;
           Debug.Log(temp);

           string[] words = temp.Split('"');

           for (int i = 1; i < words.Length; i = i + 2)
           {
               string word = words[i];
             //  Debug.Log(word);
               //avatars.Add(word);
               avatars.Add(new AvatarDefinition{name =  word, skeletonType =  "mh_cmu"});
           }
           fillAvatarList();
       }));
       
        #endif
       

    }
   
    // Update is called once per frame
    void Update()
    {
        if (animationPlayer.avatar == null) return;
        var slider = GetComponentInChildren<Slider>();
        if (userInteraction)
        {
            animationPlayer.avatar.SetCurrentFrame((int)slider.value);
            int currentFrame = animationPlayer.avatar.frameIdx;
            int nFrames = animationPlayer.avatar.GetNumFrames();
            frameCountText.text = "Frame: " + currentFrame.ToString() + "/" + nFrames.ToString();
            animationTitle.text = animationPlayer.avatar.GetClipTitle();
        }
        else
        {
            int currentFrame = animationPlayer.avatar.frameIdx;
            int nFrames = animationPlayer.avatar.GetNumFrames();
            frameCountText.text = "Frame: " + currentFrame.ToString() + "/" + nFrames.ToString();
            animationTitle.text = animationPlayer.avatar.GetClipTitle();

            slider.maxValue = nFrames;
            slider.value = currentFrame;
        }
    }

    void ClearingScene()
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

    public void OnBeginSliderDrag()
    {
        userInteraction = true;

    }

    public void OnEndSliderDrag()
    {
        userInteraction = false;
    }

    public bool IsPlaying(){
        if (animationPlayer.avatar != null){
            return false;
        }else{
            return animationPlayer.avatar.playAnimation;
        }
    }

    public void ToggleAnimation()
    {
         animationPlayer.ToggleAnimation();
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
        animationPlayer.GetMotion();
    }

    public void OnChangeModel(){
        string WEB_URL = animationPlayer.protocol + "://" + animationPlayer.url + ":" + animationPlayer.port + "/";
        string methodURL2 = WEB_URL + method2; 
        byte[] temp;
        int newModelIdx = modelDropdown.value;
        
        if (newModelIdx >= 0 && newModelIdx < avatars.Count)
        {
            modelIndex = newModelIdx;
            GetSkeleton();
            ClearingScene();
            
            
            
            string value = avatars[modelIndex].name;
            StartCoroutine(CallRestAPI.Instance.Get_model_from_binary(methodURL2, value, (stringArray) =>
            {
                temp = stringArray;
                animationPlayer.avatar.skeleton = Importer.LoadFromBytes(temp);
                generatedObjects.Add(animationPlayer.avatar.skeleton);
                
                animationPlayer.avatar.rootTransform = animationPlayer.avatar.skeleton.transform;
                
            }));
            /*
            animationPlayer.SetAvatarMesh(avatars[modelIndex].rootTransform, avatars[modelIndex].geometry);
            avatars[modelIndex].geometry.SetActive(true);
            if (animationPlayer.avatar != null)
            {
                animationPlayer.avatar.playAnimation = false;
            }
            */
        }
    }
   
    public void fillAvatarList()
    {
        modelDropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        foreach (var a in avatars)
        {
            var o = new Dropdown.OptionData();
            o.text = a.name;
            options.Add(o);
        }
        modelDropdown.AddOptions(options);
    }

    public bool HasAvatar(string name)
    {
        bool success = false;
        foreach (var a in avatars)
        {
            if (a.name == name)
            {
                success = true;
                break;
            }
        }
        return success;
    }


    public void GetSkeleton()
    { 

        if (HasAvatar(sourceSkeletonModel))
        {
            animationPlayer.meshToggle.enabled = false;
            animationPlayer.meshToggle.isOn = false;
            animationPlayer.avatar.HideMesh();
        }else
        {
            animationPlayer.meshToggle.enabled = true;
        }
        if (animationPlayer.avatar != null){
            animationPlayer.avatar.DestroySkeleton();
        }
        animationPlayer.GetSkeleton(sourceSkeletonModel);
    
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
       animationPlayer.GetMotionByID(clipID);
    }


    public void GetRandomSample(string modelID)
    {
        animationPlayer.GetRandomSample(modelID);
    }

    public void SetPort(int newPort)
    {
        port = newPort;
        animationPlayer.SetPort(newPort);
    }

    public void TogglePortWorkaround()
    {
         usePortWorkAround = !usePortWorkAround;
         animationPlayer.SetPortWorkAround(usePortWorkAround);
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
        var root = animationPlayer.avatar.root;
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

        url = newURL;
        animationPlayer.SetURL(newURL);
    }
    
    public void SetProtocol(string newProtocol){
        protocol = newProtocol;
        animationPlayer.SetProtocol(protocol);
    }
    
    void LoadScene(string clipID)
    {
       SceneManager.LoadScene("websocket_client");
    }
    
}
