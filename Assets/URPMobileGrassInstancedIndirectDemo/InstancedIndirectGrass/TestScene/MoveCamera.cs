using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Camera camera;
    public List<Transform> allCameraTransforms = new List<Transform>();

    int index = 0;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 750, 200, 100), "Move camera"))
        {
            index = (index+1) % allCameraTransforms.Count;
            Transform current = allCameraTransforms[index];
            camera.transform.position = current.position;
            camera.transform.rotation = current.rotation;
        }
    }
}
