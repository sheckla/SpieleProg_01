using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshRegenerator : MonoBehaviour
{

    public GameObject[] surfaces;

    // Start is called before the first frame update
    void Start()
    {
        
        for (int i = 0; i < surfaces.Length; i++) {
            surfaces[i].AddComponent<NavMeshSurface>();
            surfaces[i].GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
