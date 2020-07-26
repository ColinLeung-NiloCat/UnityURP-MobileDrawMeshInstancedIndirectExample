using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLight : MonoBehaviour
{
    public Light mainLight;
    public Light pointLight;

    public float YRotateSpeed = 45;

    // Update is called once per frame
    void Update()
    {
        mainLight.transform.Rotate(Vector3.up, YRotateSpeed * Time.deltaTime,Space.World);
    }

    private void OnGUI()
    {
        mainLight.enabled = GUI.Toggle(new Rect(100, 150, 200, 30), mainLight.enabled, "Main Light On/OFF");
        pointLight.enabled = GUI.Toggle(new Rect(100, 250, 200, 30), pointLight.enabled, "PointLight Light On/OFF");

        GUI.Label(new Rect(100, 350, 200, 30), "Light Rotate Speed");
        YRotateSpeed = (int)(GUI.HorizontalSlider(new Rect(100, 400, 200, 30), YRotateSpeed, 0, 90));
    }
}
