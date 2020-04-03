using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveInputTest : MonoBehaviour
{
    public SteamVR_Action_Vector2 TouchPad;
    public SteamVR_Input_Sources InputSource;

    public float ForceFactor = 1f;
    public Rigidbody ForceTarget;

    // Start is called before the first frame update
    void Start()
    {
        TouchPad.AddOnAxisListener(OnAxis, InputSource);
    }

    private void OnAxis(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        ForceTarget.AddForce(new Vector3(axis.x, 0f, axis.y) * Time.deltaTime * ForceFactor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
