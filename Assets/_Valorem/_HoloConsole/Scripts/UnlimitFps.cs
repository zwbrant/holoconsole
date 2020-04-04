using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

public class UnlimitFps : MonoBehaviour {

	// Use this for initialization
	void Start () {
        HolographicSettings.ActivateLatentFramePresentation(true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
