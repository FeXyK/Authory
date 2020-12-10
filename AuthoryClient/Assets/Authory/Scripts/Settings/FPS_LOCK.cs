using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FPS_LOCK : MonoBehaviour
{
    [SerializeField] private int MaxFps = 60;
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = MaxFps;
        //Cursor.lockState = CursorLockMode.Confined;

        Debug.unityLogger.logEnabled = true;
    }
}
