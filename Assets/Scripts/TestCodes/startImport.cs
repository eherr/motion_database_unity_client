using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
public class startImport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		ImportGLTF("Assets/Prefabs/Models/mh_cmu_male.glb");
    }
	// Single thread
	void ImportGLTF(string filepath)
	{
		GameObject result = Importer.LoadFromFile(filepath);
	}
	
}



