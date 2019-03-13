﻿// ================================================================================================================================
// File:        FPSDisplay.cs
// Description: Displays the current frame rate in the top right corner of the screen
// Author:      Harley Laurie          
// Notes:       
// ================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    private int AverageFPS;
    private Text Component;

    private void Awake()
    {
        Component = GetComponent<Text>();
    }

    private void Update()
    {
        AverageFPS = (int)(Time.frameCount / Time.time);
        Component.text = AverageFPS + " FPS";
    }
}