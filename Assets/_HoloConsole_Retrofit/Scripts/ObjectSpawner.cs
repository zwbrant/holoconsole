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
        ObjPool1 = new GameObject[10];
        ObjPool2 = new GameObject[10];
        ObjPool3 = new GameObject[200];
        ObjPool4 = new GameObject[10];

        PopulateArray(ObjPool1, Object1);
        PopulateArray(ObjPool2, Object2);
        PopulateArray(ObjPool3, Object3);
        PopulateArray(ObjPool4, Object4);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SpawnObject1();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            NotImplemented();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SpawnObject2();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SpawnObject4();

        if (Input.GetKeyDown(KeyCode.Alpha5))
            SpawnObject3();

        if (Input.GetKeyDown(KeyCode.Alpha6))
            ClearAllObjects();
    }

    public void SpawnObject1()
    {
        SpawnObjFromPool(ObjPool1);

    }

    public void SpawnObject2()
    {
        SpawnObjFromPool(ObjPool2);
    }

    public void SpawnObject3()
    {
        SpawnObjFromPool(ObjPool3, 50);


    }

    public void SpawnObject4()
    {
        SpawnObjFromPool(ObjPool4);

    }

    public void NotImplemented()
    {
        throw new System.NotImplementedException("No platypus prefab found");
    }

    public void PopulateArray(GameObject[] array, GameObject prefab)
    {
        for (int i = 0; i < array.Length; i++)
        {
            var go = Instantiate(prefab);
            go.transform.position = SpawnPoint.position;
            go.SetActive(false);

            array[i] = go;
        }
    }

    public void SpawnObjFromPool(GameObject[] pool, int count = 1)
    {
        for (int i = 0; i < pool.Length; i += count)
        {


            if (pool[i].activeSelf)
                continue;
            else
            {
                if (count == 1)
                {
                    pool[i].SetActive(true);

                    return;
                }
                else
                {
                    for (int j = 0; j < count; j++)
                        pool[i + j].SetActive(true);

                    return;
                }

            }
        }
    }

    public void ClearAllObjects()
    {
        ClearPool(ObjPool1);
        ClearPool(ObjPool2);
        ClearPool(ObjPool3);
        ClearPool(ObjPool4);

    }

    public void ClearPool(GameObject[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {

            if (!pool[i].activeSelf)
                continue;
            else
                pool[i].SetActive(false);
        }
    }

    private GameObject[] ObjPool1, ObjPool2, ObjPool3, ObjPool4;


}
