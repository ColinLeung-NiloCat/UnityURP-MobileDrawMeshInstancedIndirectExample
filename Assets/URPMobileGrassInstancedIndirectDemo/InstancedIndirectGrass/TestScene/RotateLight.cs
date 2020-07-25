using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLight : MonoBehaviour
{
    public Transform light;

    float YRotateSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        light.transform.Rotate(Vector3.up, YRotateSpeed * Time.deltaTime,Space.World);
    }

    private void OnGUI()
    {
        YRotateSpeed = (int)(GUI.HorizontalSlider(new Rect(500, 300, 200, 30), YRotateSpeed, 0, 90));
    }
}
