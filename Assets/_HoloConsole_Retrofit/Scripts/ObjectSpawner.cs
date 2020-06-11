using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject Object1;
    public GameObject Object2;
    public int Obj3Count = 200;
    public GameObject Object3;
    public GameObject Object4;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnObject1()
    {
        var obj = GameObject.Instantiate(Object1);
        obj.transform.position = SpawnPoint.position;
    }

    public void SpawnObject2()
    {
        var obj = GameObject.Instantiate(Object2);
        obj.transform.position = SpawnPoint.position;
    }

    public void SpawnObject3()
    {
        for (int i = 0; i < Obj3Count; i++)
        {
            var obj = GameObject.Instantiate(Object3);
            obj.transform.position = SpawnPoint.position;
        }

    }

    public void SpawnObject4()
    {
        var obj = GameObject.Instantiate(Object4);
        obj.transform.position = SpawnPoint.position;
    }

    public void NotImplemented()
    {
        throw new System.NotImplementedException("No platypi found");
    }

}
