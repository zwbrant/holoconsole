using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valorem.HoloConsole;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoloConsoleVRInput : MonoBehaviour
{
    public LogManager Console;
    public SteamVR_Input_Sources InputSource = SteamVR_Input_Sources.Any;
    public SteamVR_Action_Boolean ConsoleFollow = SteamVR_Input.GetBooleanAction("ToggleConsoleFollow");
    public SteamVR_Action_Boolean ConsoleToggle = SteamVR_Input.GetBooleanAction("ToggleConsole");
    public SteamVR_Action_Vector2 ConsoleScroll = SteamVR_Input.GetVector2Action("ScrollConsole");

    private void Start()
    {

    }

    

    // Update is called once per frame
    void Update()
    {
        if (ConsoleFollow.GetStateUp(InputSource)) 
            Console.FollowCamera = !Console.FollowCamera;       

        if (ConsoleToggle.GetStateUp(InputSource))
            Console.gameObject.SetActive(!Console.gameObject.activeSelf);

        Console.Scroll(ConsoleScroll.GetAxis(InputSource).y);

    }
}
