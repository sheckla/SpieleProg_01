using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterControl : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    public float rotationSpeed;
    public float movementSpeed;

    private float currentSpeed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject cam = GameObject.Find("Main Camera");
        //Debug.Log("" + isWalking);
        moveCharacter();
    }

    void moveCharacter()
    {
        Vector3 direction = new Vector3(0, 0, 0);
        GameObject cam = GameObject.Find("Main Camera");
        if (Input.GetKey("w"))
        {
            direction = cam.transform.forward;
        }
        else

        if (Input.GetKey("a"))
        {
            direction = -cam.transform.right;
        }

        if (Input.GetKey("s"))
        {
            direction = -cam.transform.forward;
        }

        if (Input.GetKey("d"))
        {
            direction = cam.transform.right;
        }

        if (Input.GetKey("w") || Input.GetKey("s") || Input.GetKey("a") || Input.GetKey("d")){ currentSpeed+=0.2f;}
        else{ currentSpeed-=0.2f;}

        animator.SetFloat("Speed",currentSpeed);

        if (direction.x == 0) {
            //animator.Play("Idle");
            return;
        };

        

        Quaternion look = Quaternion.LookRotation(direction);
        Quaternion lookY = Quaternion.Euler(0, look.eulerAngles.y, 0); // only use y-axis for rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, lookY, rotationSpeed);

        if(currentSpeed>movementSpeed)currentSpeed=movementSpeed;
        if(currentSpeed<0.0f)currentSpeed=0.0f;
        
        transform.Translate(0, 0, currentSpeed * Time.deltaTime);

        
        
    }
}
