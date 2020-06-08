using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;

public class DemoButton1 : MonoBehaviour, IInputClickHandler
{
    public List<GameObject> SpawnStuff;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {

    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        foreach (var go in SpawnStuff)
        {
            Instantiate(go);
        }
    }
}
