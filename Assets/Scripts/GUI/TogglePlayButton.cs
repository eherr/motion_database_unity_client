using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePlayButton : MonoBehaviour {

    private Sprite original;
    public Sprite toggle;
    public RESTGUIManager guiManager;

    private void Start()
    {
        original = gameObject.GetComponent<Image>().sprite;
    }

    // Update is called once per frame
    void Update () {
        if (guiManager.IsPlaying())
        {
            gameObject.GetComponent<Image>().sprite = toggle;
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = original;
        }
	}
}
