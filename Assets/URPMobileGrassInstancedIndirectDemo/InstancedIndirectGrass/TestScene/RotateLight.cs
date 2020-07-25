using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLight : MonoBehaviour
{
    public Transform light;

    public float YRotateSpeed = 45;

    // Update is called once per frame
    void Update()
    {
        light.transform.Rotate(Vector3.up, YRotateSpeed * Time.deltaTime,Space.World);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(500, 250, 200, 30), "Light Rotate Speed");
        YRotateSpeed = (int)(GUI.HorizontalSlider(new Rect(500, 300, 200, 30), YRotateSpeed, 0, 90));
    }
}
