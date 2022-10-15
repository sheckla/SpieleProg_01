using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipToeLogic : MonoBehaviour
{
    // Public Offset Vals
    public float PlatformSpacing;
    public float TotalUnitWidth;
    public float TotalUnitDepth;
    public float xOffset;
    public float zOffset;

    [SerializeField] public GameObject platformPrefab;
    private const int PlatformWidth = 10;
    private const int PlatformDepth = 13;

    private GameObject[,] platforms = new GameObject[PlatformWidth, PlatformDepth];

    void Start()
    {
        for (int i = 0; i < PlatformWidth; i++)
        {
            for (int j = 0; j < PlatformDepth; j++)
            {
               GameObject platform = GameObject.Instantiate(platformPrefab);
               float randFloat = Random.Range(0.0f,1.0f);
               bool isPath = (randFloat < 0.5f) ? true : false;
               TipToePlatform ttplat = platform.GetComponent<TipToePlatform>();
               ttplat.SetPath(isPath);
               platforms[i,j] = platform;
            }
        }
    }

    void applyPlatformTransformations()
    {
        float d = TotalUnitWidth / PlatformDepth;
        float w = TotalUnitDepth / PlatformWidth;
        for (int i = 0; i < PlatformWidth; i++)
        {
            for (int j = 0; j < PlatformDepth; j++)
            {
                float widthOffset = zOffset + i*w + i*PlatformSpacing;
                float depthOffset = xOffset + j*d + j*PlatformSpacing;
                Vector3 offset = new Vector3(widthOffset, -0.2f, depthOffset);
                Vector3 scale = new Vector3(w, 0.2f, d);
                platforms[i,j].SetActive(true);
                platforms[i,j].transform.position = offset;
                platforms[i,j].transform.localScale = scale;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Uncomment if adjusting Platform Matrix positions in Unity-Editor
        //applyPlatformTransformations();
    }
}
