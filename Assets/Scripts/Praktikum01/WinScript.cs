using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScript : MonoBehaviour
{
     public ParticleSystem particles;

    // Start is called before the first frame update 13/40  10/30
    void Start()
    {
        particles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
    }

     public void win()
    {
        particles.Play();
    }


}
