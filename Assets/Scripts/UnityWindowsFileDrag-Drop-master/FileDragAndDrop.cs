using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using Siccity.GLTFUtility;

public class FileDragAndDrop : MonoBehaviour
{
    List<string> log = new List<string>();
    DropInfo dropInfo = null;
    GameObject result = null;
    List<GameObject> generatedObjects = new List<GameObject>();
    class DropInfo
    {
        public string file;
        public Vector2 pos;
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
        string str = "Dropped " + aFiles.Count + " files at: " + aPos + "\n\t" +
            aFiles.Aggregate((a, b) => a + "\n\t" + b);
        Debug.Log(str);
        log.Add(str);

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
        ClearingScene();
        result = Importer.LoadFromFile(aInfo.file);
        generatedObjects.Add(result);
    }
    private void OnGUI()
    {
            
        if (GUILayout.Button("CLEAR"))
        {
            log.Clear();
            ClearingScene();
        }
            
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
    void ClearingScene()
    {
       
        Debug.Log("Inside ClearingScene");
        Debug.Log(generatedObjects.Count);
        for (int i = 0; i < generatedObjects.Count; ++i)
        {
            if (generatedObjects[i] != null)
            {
                Debug.Log("previous object exists");
                Destroy(generatedObjects[i]);
                
            }
        }
        generatedObjects.Clear();
    }
}
