using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendel : MonoBehaviour
{

    private Rigidbody Rbody;
    public bool goingForward = true;
    private float prevZ;

    // Start is called before the first frame update
    void Start()
    {
        Rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() 
    {  
        goingForward = (transform.position.z > prevZ) ? true : false;
        prevZ = transform.position.z;

        if (Rbody.velocity.magnitude >= 10) return;

        if (goingForward)
        {
            Rbody.AddForce(new Vector3(0,0, 10f));
        } else 
        {
            Rbody.AddForce(new Vector3(0,0, -10f));
        }
    }
}
