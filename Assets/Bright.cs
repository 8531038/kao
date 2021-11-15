using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Bright : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnGUI()
    {
        var rect = new Rect(20, 150, 700, 100);
        GUI.skin.label.fontSize = 45;
        rect.y = 450;
        GUI.Label(rect, string.Format("Bright = {0:f2}", Screen.brightness.ToString()));
    }
}
