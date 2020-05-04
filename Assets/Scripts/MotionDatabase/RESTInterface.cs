using System.Collections;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using UnityEngine.Networking;

namespace MotionDatabaseInterface
{
	public delegate void PostRequestCallback(string response);

	public delegate void BytePostRequestCallback(byte[] response);

    public class RESTInterface : MonoBehaviour
    {

        public string protocol = "https";
        public int port;
        public string url;
        public bool usePort;
        public bool usePortWorkAround;

        /// <summary>
        /// Synchronous HTTP Post request.
        /// </summary>
        /// <param name="method">REST method name</param>
        /// <param name="messageBody">POST message body string</param>
        /// <returns>Response string</returns>
        public string sendRequest(string method, string messageBody)
        {
            Debug.Log("Try to connect to server");
            //http://stackoverflow.com/questions/9145667/how-to-post-json-to-the-server
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + url + ":" + port.ToString() + "/" + method);
            request.ContentType = "application/json";
            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(messageBody);
                streamWriter.Flush();
                streamWriter.Close();
                Debug.Log("Send message to server");
            }

            WebResponse response = request.GetResponse();
            string responseFromServer = null;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseFromServer = reader.ReadToEnd();
            }
            response.Close();
            return responseFromServer;
        }

        /// <summary>
        /// Asynchronous HTTP Post request.
        /// src: http://stackoverflow.com/questions/27310201/async-requests-in-unity
        /// https://docs.unity3d.com/Manual/UnityWebRequest-CreatingDownloadHandlers.html
        /// </summary>
        /// <param name="method">REST method name.</param>
        /// <param name="messageBody">POST message body string.</param>
        /// <param name="callback">Callback handler that process the response string. </param>
        /// <returns></returns>
        protected IEnumerator sendRequestCoroutine(string method, string messageBody, PostRequestCallback callback)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(messageBody);
            string urlString = getMethodURL(method);

            UnityWebRequest webRequest = UnityWebRequest.Post(urlString, UnityWebRequest.kHttpVerbPOST);
            webRequest.uploadHandler = new UploadHandlerRaw(data);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                print("Error: " + webRequest.error);
            }
            else
            {
                //print(webRequest.downloadHandler.text);
                byte[] results = webRequest.downloadHandler.data;
                //webRequest.downloadHandler.text
                var text = System.Text.Encoding.UTF8.GetString(results);
                callback(text);
            }
        }

        

        /// <summary>
        /// Asynchronous HTTP Post request.
        /// src: http://stackoverflow.com/questions/27310201/async-requests-in-unity
        /// https://docs.unity3d.com/Manual/UnityWebRequest-CreatingDownloadHandlers.html
        /// </summary>
        /// <param name="method">REST method name.</param>
        /// <param name="messageBody">POST message body string.</param>
        /// <param name="callback">Callback handler that process the response string. </param>
        /// <returns></returns>
        protected IEnumerator sendRequestCoroutine(string method, string messageBody, BytePostRequestCallback callback)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(messageBody);
            string urlString = getMethodURL(method);
            UnityWebRequest webRequest = UnityWebRequest.Post(urlString, UnityWebRequest.kHttpVerbPOST);
            webRequest.uploadHandler = new UploadHandlerRaw(data);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                print("Error: " + webRequest.error);
            }
            else if(webRequest.downloadHandler.text == "Error")
            {
                print("Error to get file. ");
            } 
            else
            {
                callback(webRequest.downloadHandler.data);
            }

        }
        
        /// <summary>
        /// Asynchronous HTTP Post request.
        /// src: http://stackoverflow.com/questions/27310201/async-requests-in-unity
        /// https://docs.unity3d.com/Manual/UnityWebRequest-CreatingDownloadHandlers.html
        /// </summary>
        /// <param name="method">REST method name.</param>
        /// <param name="messageBody">POST message body string.</param>
        /// <param name="callback">Callback handler that process the response string. </param>
        /// <returns></returns>
        protected IEnumerator sendRequestCoroutineString(string method, string messageBody, System.Action<string> callback)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(messageBody);
            string urlString = getMethodURL(method);
            UnityWebRequest webRequest = UnityWebRequest.Post(urlString, UnityWebRequest.kHttpVerbPOST);
            webRequest.uploadHandler = new UploadHandlerRaw(data);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                print("Error: " + webRequest.error);
            }
            else
            {
                string result = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                callback(result);
            }

        }


        /// <summary>
        /// Asynchronous HTTP GET request.
        /// src: http://stackoverflow.com/questions/27310201/async-requests-in-unity
        /// https://docs.unity3d.com/Manual/UnityWebRequest-CreatingDownloadHandlers.html
        /// </summary>
        /// <param name="method">REST method name.</param>
        /// <param name="callback">Callback handler that process the response string. </param>
        /// <returns></returns>
        protected IEnumerator sendGETRequestCoroutine(string method, System.Action<string> callback)
        {

            UnityWebRequest webRequest = UnityWebRequest.Get(getMethodURL(method));
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                print("Error: " + webRequest.error);
            }
            else
            {
                callback(webRequest.downloadHandler.text);
            }

        }

        string getMethodURL(string method)
        {
            string urlString = "";
            if (usePortWorkAround && usePort)
            {
                urlString = protocol + "://" + url + "/" + port.ToString() + method;
            }
            else if (usePort)
            {
                urlString = protocol + "://" + url + ":" + port.ToString() + "/" + method;
            }
            else
            {
                urlString = protocol + "://" + url + "/" + method;
            }
            return urlString;
        }
    }

    }