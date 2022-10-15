using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipToeLogic : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    private int width = 10;
    private int depth = 13;

    // Start is called before the first frame update 13/40  10/30
    void Start()
    {
        int[,] path = new int[10,13]{{0,0,0,0,1,0,0,0,0,0,0,0,0},
                                     {0,0,0,0,1,0,0,0,0,0,0,0,0},
                                     {0,0,0,0,1,1,1,0,0,0,0,0,0},
                                     {0,0,0,0,0,0,1,0,0,1,1,1,0},
                                     {0,0,0,0,0,0,1,0,0,1,0,1,1},
                                     {0,0,0,0,0,0,1,1,1,1,0,0,1},
                                     {0,0,0,0,0,0,0,0,0,0,0,0,1},
                                     {0,0,0,0,0,0,0,0,1,1,1,1,1},
                                     {0,0,0,0,0,0,1,1,1,0,0,0,0},
                                     {0,0,0,0,0,0,1,0,0,0,0,0,0}};

        float adj_x = gameObject.transform.position.x;
        float adj_z = gameObject.transform.position.z;
        for(int i=0;i<width;i++)
            for(int j=0;j<depth;j++)
                Instantiate(platformPrefab, new Vector3(i*3f + adj_x, -0.25f, j*3f + adj_z), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
