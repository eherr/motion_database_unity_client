using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CallRestAPI : MonoBehaviour
{
	private static CallRestAPI _instance;
	public static CallRestAPI Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<CallRestAPI>();
				if (_instance == null)
				{
					GameObject go = new GameObject();
					go.name = typeof(CallRestAPI).Name;
					_instance = go.AddComponent<CallRestAPI>();
					DontDestroyOnLoad(go);
				}
			}
			return _instance;
		}
	}

	public IEnumerator Get(string url, System.Action<string> callback)
	{
		string jsonResult = "";
		using (UnityWebRequest www = UnityWebRequest.Get(url))
		{
			yield return www.SendWebRequest();
			if(www.isNetworkError)
			{
				Debug.Log(www.error);
			} else
			{
				if(www.isDone)
				{
					jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
					yield return jsonResult;

					callback(jsonResult);
					

				}
			}
		}
	}

	// For Method "get_binary" return a byte array instead of string
	public IEnumerator Get_model_from_binary(string url, string name , System.Action<byte[]> callback)
	{
		
		UnityWebRequest www = UnityWebRequest.Post(url, name);
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else
		{
			if (www.isDone)
			{
				byte[] bt = www.downloadHandler.data;
				//jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
				//Debug.Log(jsonResult);
				yield return bt;
				callback(bt);
			}

		}
	}

	// Returns a string
	public IEnumerator Get_string_from_method(string url, string name, System.Action<string> callback)
	{

		UnityWebRequest www = UnityWebRequest.Post(url, name);
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else
		{
			if (www.isDone)
			{
				string result = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
				//Debug.Log(jsonResult);
				yield return result;
				callback(result);
			}

		}
	}


}
