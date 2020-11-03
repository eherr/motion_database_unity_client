//author: Erik Herrmann
//orbiting camera based on http://www.glprogramming.com/red/chapter03.html and https://forum.unity.com/threads/how-to-change-main-camera-pivot.700442/
//Horizontal movement based on
//http://www.youtube.com/watch?v=RInkwoCgIps
//http://www.youtube.com/watch?v=H20stuPG-To

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Vector3 lastPos;
    Vector3 delta;
    Camera cam;
    public float rotationScale = 1f;
    public float translationScale = 0.1f;
    public float zoomSpeed = 1f;
    bool isRotating;
    bool isTranslating;

    public float pitch = 0;
    public float yaw = 0;
    public float zoom = 0;
    public bool Active;
    public Transform cameraTransform;
    public Transform cameraTarget;
    const int ROTATE_BUTTON = 0;//1;
    const int TRANSLATE_BUTTON = 1;//2;
    void Start()
    {
        Active = true;
        isRotating = false;
        isTranslating = false;
        //set initial values from transform
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        rotate(Vector2.zero);
        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }

    }

    void Update()
    {
        if (!Active) return;
        if(cameraTarget != null)
        {
            transform.position = cameraTarget.position;
        }
        else{ 
            handleTranslationInput();
        }
        handleZoomInput();
        handleRotationInput();
    }

    void handleZoomInput()
    {

        if (!isTranslating)
        {
            var z = Input.mouseScrollDelta.y;
            cameraTransform.Translate(0, 0, z * zoomSpeed, Space.Self);
        }
    }

    void handleTranslationInput()
    {
        if (Input.GetMouseButtonDown(TRANSLATE_BUTTON))
        {
            if (!isRotating)
            {
                lastPos = Input.mousePosition;
                delta = Vector3.zero;

                isTranslating = true;
            }
        }
        if (Input.GetMouseButtonUp(TRANSLATE_BUTTON))
        {
            isTranslating = false;
        }
        if (isTranslating)
        {
            delta = Input.mousePosition - lastPos;
            translate(delta);
            lastPos = Input.mousePosition;
        }
    }

    void handleRotationInput()
    {
        if (Input.GetMouseButtonDown(ROTATE_BUTTON))
        {
            if (!isRotating)
            {
                lastPos = Input.mousePosition;
                delta = Vector3.zero;
                isRotating = true;
            }
        }
        if (Input.GetMouseButtonUp(ROTATE_BUTTON))
        {
            isRotating = false;
        }
        if (isRotating)
        {
            delta = Input.mousePosition - lastPos;
            rotate(delta);
            lastPos = Input.mousePosition;
        }
    }

    void rotate(Vector2 delta)
    {

        yaw += rotationScale * delta.x;
        pitch += rotationScale * -delta.y;

        var qx = Quaternion.Euler(pitch, 0, 0);
        var qy = Quaternion.Euler(0, yaw, 0);
        transform.rotation = qy * qx;

        yaw %= 360;
        pitch %= 360;
    }

    void translate(Vector2 delta)
    {
        var pos = transform.position;
        var rad = Mathf.Deg2Rad * -yaw;
        float distance = -delta.x * translationScale * 2;
        pos.x += distance * Mathf.Cos(rad);
        pos.z += distance * Mathf.Sin(rad);
        pos.y -= translationScale * delta.y;
        transform.position = pos;
        
    }

}
