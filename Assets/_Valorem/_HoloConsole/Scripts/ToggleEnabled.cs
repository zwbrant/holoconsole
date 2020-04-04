using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEnabled : MonoBehaviour {
    public GameObject ObjectToToggle;
    public KeyCode ToggleKey;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(ToggleKey))
        {
            ObjectToToggle.SetActive(!ObjectToToggle.activeSelf);
        }
	}
}
