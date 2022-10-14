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

    private Rigidbody rigidBody;

    private bool isgrounded;
    private bool isJumping=false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject cam = GameObject.Find("Main Camera");
        //Debug.Log("" + isWalking);
        moveCharacter();
        //Debug.DrawRay(transform.position, new Vector3(0,-1.0f,0),Color.green);
        Debug.Log(isGrounded());
    }

    bool isGrounded(){
        return isgrounded;
    }

    void changeSpeed(float x){
        currentSpeed+=x * Time.deltaTime;
        if(currentSpeed>movementSpeed)currentSpeed=movementSpeed;
        if(currentSpeed<0.0f)currentSpeed=0.0f;
    }

    void OnCollisionEnter(Collision collision)
    {
         isgrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
         isgrounded = false;
    }

    void moveCharacter()
    {

        Vector3 direction = new Vector3(0, 0, 0);
        GameObject cam = GameObject.Find("Main Camera");

        if(isGrounded()){

            isJumping=false;

            if (Input.GetKey("w"))
            {
                direction += cam.transform.forward;
            }
            if (Input.GetKey("a"))
            {
                direction += -cam.transform.right;
            }
            if (Input.GetKey("s"))
            {
                direction += -cam.transform.forward;
            }
            if (Input.GetKey("d"))
            {
                direction += cam.transform.right;
            }
            direction.Normalize();
            if (Input.GetKey("space"))
            {
                rigidBody.AddForce(Vector3.up * 7.0f);
                animator.Play("Jumping");
                isJumping=true;
            }
            if (Input.GetKey("w") || Input.GetKey("s") || Input.GetKey("a") || Input.GetKey("d")){ 
                changeSpeed(7.0f);
            }

            animator.SetFloat("Speed",currentSpeed);
        }
        else
        {
            if(isJumping==false)animator.Play("Airborne");
            changeSpeed(-5.0f);
        }

        if (direction.x == 0 && isGrounded()) {changeSpeed(-12.0f);}

        if (direction.x == 0) {
            Debug.Log("Speed: "+currentSpeed);
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
            return;
        };
        
    
        Quaternion look = Quaternion.LookRotation(direction);
        Quaternion lookY = Quaternion.Euler(0, look.eulerAngles.y, 0); // only use y-axis for rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, lookY, rotationSpeed);

        Debug.Log("Speed: "+currentSpeed);
        transform.Translate(0, 0, currentSpeed * Time.deltaTime); 
        
    }
}
