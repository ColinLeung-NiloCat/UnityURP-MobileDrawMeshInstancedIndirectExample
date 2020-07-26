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
        if(GUI.Button(new Rect(100, 450, 200, 100), "Move camera"))
        {
            index = (index+1) % allCameraTransforms.Count;
        }
    }

    private void LateUpdate()
    {
        Transform target = allCameraTransforms[index];

        camera.transform.position = Vector3.Lerp(camera.transform.position,target.position, Time.deltaTime * 2);
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation,target.rotation, Time.deltaTime * 2);
    }
}
