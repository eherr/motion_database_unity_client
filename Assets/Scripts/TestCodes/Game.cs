using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

public class Game : MonoBehaviour
{

	public string WEB_URL =  "http://localhost:8888/"; // "https://motion.dfki.de/8888"; 
	public string method1 = "get_GLB_list";
	public string method2 = "get_binary";
   
	public Dropdown drop1;
	public InputField inp;

	GameObject abc = null;
	List<GameObject> generatedObjects = new List<GameObject>();
	void Start()
    {
		string methodURL1 = WEB_URL + method1;
		string methodURL2 = WEB_URL + method2;
		string temp = "";
		
		List<string> glb_names = new List<string>();
		List<string> skeleton_names = new List<string>();
		StartCoroutine(CallRestAPI.Instance.Get(methodURL1, (stringArray) =>
		{
			temp = stringArray;
			Debug.Log(temp);
			
			string[] words = temp.Split('"');

			for (int i = 1; i < words.Length; i = i + 2)
			{
				string word = words[i];
				//Debug.Log(word);
				glb_names.Add(word);
			}
			drop1.AddOptions(glb_names);
			
		}));
		
	}

	public void OnButtonClicked()
	{
		print(inp.text);
	}
	public void Dropdown_IndexChanges(int index)
	{
		ClearingScene();
		string methodURL2 = WEB_URL + method2;
		byte[] temp;
		//get the selected index
		int menuIndex = drop1.GetComponent<Dropdown>().value;

		//get all options available within this dropdown menu
		List<Dropdown.OptionData> menuOptions = drop1.GetComponent<Dropdown>().options;

		//get the string value of the selected index
		string value = menuOptions[menuIndex].text;

		//Call method to load model from binaries
		StartCoroutine(CallRestAPI.Instance.Get_model_from_binary(methodURL2, value, (stringArray) => {
			temp = stringArray;
			abc = Importer.LoadFromBytes(temp); 
			generatedObjects.Add(abc);

			/* // To parse json do the following:
			JObject json = JObject.Parse(temp);
			string textures = json.GetValue("textures").ToString();
			*/


		}));
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
