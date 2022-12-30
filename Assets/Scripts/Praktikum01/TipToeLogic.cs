using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor.AI;

public class TipToeLogic : MonoBehaviour
{
    // Public Offset Vals
    public float Spacing;
    public float Width;
    public float Depth;
    public float xOffset;
    public float zOffset;
    public float yOffset;
    public float Timer = 0;

    [SerializeField] public GameObject platformPrefab;
    private const int cols = 10;
    private const int rows = 13;

    private GameObject[,] platforms = new GameObject[cols, rows];

    void Start()
    {
        xOffset = transform.position.x;
        zOffset = transform.position.z;
        yOffset = transform.position.y;
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
               GameObject platform = GameObject.Instantiate(platformPrefab);
               TipToePlatform ttplat = platform.GetComponent<TipToePlatform>();
               ttplat.name = "TTPLAT_" + (i) + "_" + j;
               ttplat.row = j;
               ttplat.col = i;
               platforms[i,j] = platform;
            }
        }

        generatePath();
        applyPlatformTransformations();
        generateNavMesh();
    }

    void generateNavMesh()
    {
        //UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        for (int i = 0; i < cols; i++) {
            for (int j = 0; j < rows; j++) {
                platforms[i,j].SetActive(false);
                platforms[i,j].SetActive(true);
            }
        }
    }

    public void getPlatforms(ref GameObject[,] platforms, ref int width, ref int depth) {
        platforms = this.platforms;
        width = cols;
        depth = rows;
    }

    void generatePath() 
    {
        // Start at the last row
        int i = Random.Range(0, cols-1);
        int j = rows-1;
        platforms[i, j].GetComponent<TipToePlatform>().isPath = true;

        while (j > 0)
        {
            int direction = Random.Range(0, 4);


            if (direction == 0) 
            {
                // left
                if (!platforms[Mathf.Clamp(i-1, 0, cols-1), Mathf.Clamp(j+2, 0, rows-1)].GetComponent<TipToePlatform>().isPath &&
                !platforms[Mathf.Clamp(i-1, 0, cols-1), Mathf.Clamp(j-2, 0, rows-1)].GetComponent<TipToePlatform>().isPath)
                {
                i--;

                }
            } else if (direction == 1)
            {
                // forward
                    j--;
            } else if (direction == 2)
            {
                // backward
                j--;
                if (!platforms[Mathf.Clamp(i, 0, cols-1), Mathf.Clamp(j+2, 0, rows-1)].GetComponent<TipToePlatform>().isPath)
                {
                   // j++;
                }
            } else {
                // right
                if (!platforms[Mathf.Clamp(i+1, 0, cols-1), Mathf.Clamp(j+2, 0, rows-1)].GetComponent<TipToePlatform>().isPath &&
                !platforms[Mathf.Clamp(i+1, 0, cols-1), Mathf.Clamp(j-2, 0, rows-1)].GetComponent<TipToePlatform>().isPath)
                {
                i++;
                }
            }

            i = Mathf.Clamp(i, 0, cols - 1);
            j = Mathf.Clamp(j, 0, rows - 1);
            platforms[i, j].GetComponent<TipToePlatform>().isPath = true;
            //platforms[i,j].GetComponent<TipToePlatform>().isTaggedPath = true;
            //if (j == 0) WinningTarget.transform.position = platforms[i, j].transform.position;
        }
    }

    void applyPlatformTransformations()
    {
        float d = Width / rows;
        float w = Depth / cols;
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                float widthOffset = i*w + i*Spacing;
                float depthOffset = j*d + j*Spacing;
                Vector3 offset = new Vector3(widthOffset, -0.2f, depthOffset);
                Vector3 scale = new Vector3(w, 0.2f, d);
                platforms[i,j].SetActive(true);
                platforms[i,j].transform.position = offset;
                platforms[i,j].transform.localScale = scale;
                platforms[i,j].transform.position += transform.position;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Uncomment if adjusting Platform Matrix positions in Unity-Editor
        applyPlatformTransformations();
    }
}
