using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlControls : MonoBehaviour
{
    // *** PUBLIC
    private Animator Anim;
    private CharacterController CharController;
    public float TurnSmoothTime = 0.1f;
    [SerializeField]
    public float Speed;

    // *** PRIVATE
    private float TurnSmoothVelocity;
    private float JumpMomentum;
    private float FallingMomentumMultiplier;
    private GameObject Cam;

    private float CurrentMovementSpeed = 0;
    private float AnimRunningVal = 0;

    private void Start() {
        CharController = GetComponent<CharacterController>();
        Anim = GetComponent<Animator>();
        Cam = GameObject.Find("Main Camera");
    }

    // https://www.youtube.com/watch?v=dJPnqv2IOTE&t=618s
    // https://www.youtube.com/watch?v=4HpC--2iowE
    private void Update() {
        handleRunning();
        handleFalling();
    }

    // *** Rotation & Running ***
    private void handleRunning()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // As int
        float vertical = Input.GetAxisRaw("Vertical"); // as int
        Vector3 axisDir = new Vector3(horizontal, 0f, vertical).normalized;

        CurrentMovementSpeed = (Input.GetKey("left shift")) ? Speed * 2.3f : Speed;

        if (axisDir.magnitude >= 0.1f)
        {
            // Get InputAngle adjusted for CameraAngle
            // atan2(x, z) gives angle between vector(x,z) and the +z axis
            float targetAngle = Mathf.Atan2(axisDir.x, axisDir.z) * Mathf.Rad2Deg + Cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,ref TurnSmoothVelocity, TurnSmoothTime);

            Vector3 camMoveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // Apply Rotation
            CharController.Move(camMoveDirection * CurrentMovementSpeed * Time.deltaTime); // Apply Movement
            Anim.SetFloat("Speed", smoothVal(ref AnimRunningVal, 0, 1, 0.02f));
        } else
        {
            Anim.SetFloat("Speed", smoothVal(ref AnimRunningVal, 0, 1, -0.02f));
        }
    }

    // *** Jumping & Gravity *** 
    private void handleFalling()
    {
        Vector3 gravityVelocity = Vector3.zero;

        // Grounded, reset falling
        if (CharController.isGrounded) 
        {
            FallingMomentumMultiplier = .0f;
            if (Input.GetKey("space"))
            {
                JumpMomentum = 25;
                Anim.SetTrigger("Jumping");
            } else
            {
                JumpMomentum = 0;
            }

        } else {
        // Not Grounded, init Falling
            gravityVelocity =  -transform.up * 9.81f * Time.deltaTime * FallingMomentumMultiplier;
            incrementVal(ref FallingMomentumMultiplier);
            decrementVal(ref JumpMomentum);
        }
        Vector3 jumpVelocity = transform.up * JumpMomentum * Time.deltaTime;
        CharController.Move(gravityVelocity + jumpVelocity); // Apply Gravity/Jumping


        // Spherecasting
        RaycastHit hit;
        float dist = Mathf.Infinity;
        if (Physics.SphereCast(transform.position + transform.up, CharController.radius, -transform.transform.up, out hit, 100)) dist = hit.distance;
        bool grounded = (dist <= CharController.radius + 1E-6f) ? true : false;
        Anim.SetBool("Grounded", grounded);        
    }

    private void incrementVal(ref float val)
    {
        if (val >= 5) return;
        val += 0.02f;
        if (val >= 5) val = 5;
    }

    private void decrementVal(ref float val)
    {
        if (val == 0) return;
        val -= 0.02f;
        if (val < 0) val = 0;
    }

    private float smoothVal(ref float val, float min, float max, float fac)
    {
        val += fac;
        if (val < min) val = min;
        if (val > max) val = max;
        return val;
    }
}
