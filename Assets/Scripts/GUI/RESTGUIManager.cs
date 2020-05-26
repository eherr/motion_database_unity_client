using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using MotionDatabaseInterface;
using UnityEngine.SceneManagement;
using Siccity.GLTFUtility;
using B83.Win32;
using System.Linq;

[System.Serializable]
public class AvatarDefinition
{
    public string name;
    public string skeletonType;
}

public class FileUpload
{
    public  string upload_file_path { get; set; }
}

public class DropInfo
{
    public string file;
    public Vector2 pos;
}

public class RESTGUIManager : MonoBehaviour
{
    public string protocol;
    public int port;
    public string url;
    public bool usePortWorkAround;
    public CustomAnimationPlayerInterface animationPlayer;
    public List<AvatarDefinition> avatars;
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
    public InputField file_path;
    FileUpload newfile = new FileUpload();
    
    
    static string f_path, u_path;
    List<string> log = new List<string>();
    DropInfo dropInfo = null;

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
        useMesh = false;

        animationPlayer.ProgressBar.gameObject.SetActive(false);
        //https://www.tangledrealitystudios.com/development-tips/prevent-unity-webgl-from-stopping-all-keyboard-input/
#if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
#endif

        GetSkeleton();
        if (sourceSkeletonModel == "")
            sourceSkeletonModel = "mh_cmu";
        animationPlayer.GetAvatarList(sourceSkeletonModel, handleAvatarList);
#if UNITY_EDITOR
        //  animationPlayer.UploadAvatarToServer("C:\\Users\\Anindita\\DFKI_work\\models\\model8_cmu.glb");
        //file_path.text = "C:\\Users\\Anindita\\DFKI_work\\models\\model8_cmu.glb";
#endif
    }
    
    


    void OnEnable()
    {
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        // do something with the dropped file names. aPos will contain the 
        // mouse position within the window where the files has been dropped.
        f_path = aFiles.Aggregate((a, b) => a + "\n\t" + b);
        Debug.Log(f_path);
        log.Add(f_path);

        string file = "";
        // scan through dropped files and filter out supported image types
        foreach (var f in aFiles)
        {
            var fi = new System.IO.FileInfo(f);
            var ext = fi.Extension.ToLower();
            if (ext == ".glb")
            {
                file = f;
                break;
            }
            else
            {
                log.Add("Not a GLB file");
            }
        }

        // If the user dropped a supported file, create a DropInfo
        if (file != "")
        {
            var info = new DropInfo
            {
                file = file,
                pos = new Vector2(aPos.x, aPos.y)
            };
            dropInfo = info;
        }
    }

    void LoadModel(DropInfo aInfo)
    {
        if (aInfo == null)
            return;
        GetSkeleton();
        animationPlayer.ClearGeneratedObjects();
        animationPlayer.UploadAvatarToServer(f_path);
        //ClearingScene();
        //result = Importer.LoadFromFile(aInfo.file);
        //generatedObjects.Add(result);
    }

    private void OnGUI()
    {
        foreach (var s in log)
            GUILayout.Label(s);
        DropInfo tmp = null;
        if (Event.current.type == EventType.Repaint && dropInfo != null)
        {
            tmp = dropInfo;
            dropInfo = null;
        }

        LoadModel(tmp);
    }

   void handleAvatarList(string stringArray)
    {
        string[] words = stringArray.Split('"');

        for (int i = 1; i < words.Length; i = i + 2)
        {
            string word = words[i];
            avatars.Add(new AvatarDefinition {name = word, skeletonType = sourceSkeletonModel});
        }

        fillAvatarList();
    }

    // Update is called once per frame
    void Update()
    {
        if (animationPlayer.avatar == null) return;
        var slider = GetComponentInChildren<Slider>();
        if (userInteraction)
        {
            animationPlayer.avatar.SetCurrentFrame((int) slider.value);
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

    public void OnBeginSliderDrag()
    {
        userInteraction = true;
    }

    public void OnEndSliderDrag()
    {
        userInteraction = false;
    }

    public bool IsPlaying()
    {
        if (animationPlayer.avatar != null)
        {
            return false;
        }
        else
        {
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

    public void OnTextChanged()
    {
        u_path = file_path.text;
        print(u_path);
    }
    public void ButtonIsClicked()
    {
        u_path = file_path.text;
        if (u_path == "" || u_path == null)
        {
            print("Field is empty ....");
        }
        else
        {
            print(u_path);
           
            GetSkeleton();
            animationPlayer.ClearGeneratedObjects();
            animationPlayer.UploadAvatarToServer(u_path);
            
        }
    }
    public void OnChangeModel()
    {
        loadAvatar();
    }

    public void loadAvatar()
    {
        int newModelIdx = modelDropdown.value;
        if (newModelIdx >= 0 && newModelIdx < avatars.Count)
        {
            modelIndex = newModelIdx;
            GetSkeleton();
            animationPlayer.ClearGeneratedObjects();
            animationPlayer.LoadAvatar(sourceSkeletonModel, avatars[modelIndex].name);
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

    public void ToggleMesh()
    {
        if (animationPlayer.waitingForSkeleton)
        {
            Debug.Log("waiting");
            animationPlayer.meshToggle.SetIsOnWithoutNotify(useMesh);
            return;
        }

        if (avatars.Count > 0)
        {
            useMesh = !useMesh;
        }
        else
        {
            useMesh = false;
        }

        animationPlayer.meshToggle.SetIsOnWithoutNotify(useMesh);
        Debug.Log("use mesh" + useMesh.ToString());
        if (!useMesh)
        {
            animationPlayer.ToggleAnimation();
            animationPlayer.avatar.SetAvatarMesh(null, null);
            animationPlayer.ClearGeneratedObjects();
            GetSkeleton();
        }
        else
        {
            loadAvatar();
        }
    }


    public void GetSkeleton()
    {
        animationPlayer.GetSkeleton(sourceSkeletonModel);
    }

    public void SetSourceSkeleton(string name)
    {
        Debug.Log("Set source skeleton " + name);
        if (name != sourceSkeletonModel || !initialized)
        {
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

    public void EnableCamera()
    {
        if (cameraController != null) cameraController.gameObject.SetActive(true);
        Debug.Log("Enable Camera");
    }


    public void DisableCamera()
    {
        if (cameraController != null) cameraController.gameObject.SetActive(false);
        Debug.Log("Disable Camera");
    }

    public void ToggleCenterCamera()
    {
        var root = animationPlayer.avatar.root;
        centerCamera = !centerCamera && root != null;
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

    void SetURL(string newURL)
    {
        url = newURL;
        animationPlayer.SetURL(newURL);
    }

    public void SetProtocol(string newProtocol)
    {
        protocol = newProtocol;
        animationPlayer.SetProtocol(protocol);
    }

    void LoadScene(string clipID)
    {
        SceneManager.LoadScene("websocket_client");
    }
}