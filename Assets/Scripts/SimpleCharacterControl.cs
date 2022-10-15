using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterControl : MonoBehaviour
{
    private Animator Anim;
    private Rigidbody Rbody;
    private CapsuleCollider CapColl;

    [SerializeField]
    public float RotationSpeed;
    public float MovementSpeed;     

    // Smoothed movement momentum vals
    public float FallMomentum = 1;
    enum MomentumTypes {WalkMomentum, JumpMomentum} // specifies decay/growth for smooth transitions
    public float RunMomentum = 0; 
    public float JumpMomentum = 0;

    private Vector3 InitialStartPosition;
    private Vector3 Dir;
    public bool Grounded;
    public float Dist;

    private bool JumpLockout;

    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody>();
        CapColl = GetComponent<CapsuleCollider>();
        InitialStartPosition = Rbody.position;
    }

    // Called each Frame
    void Update() 
    {
        // get direction via. keyboard inputs [W,A,S,D]
        Dir = WASD_Dir();
        handleJumping();
    }

    // Called each physics step
    void FixedUpdate()
    {
        handleJumping();
        Vector3 gravity;
        handleFalling(out gravity); 
        handleWalkRotation();
        handleJumping();
        Vector3 forward = Rbody.transform.forward * RunMomentum * MovementSpeed * Time.deltaTime;
        Vector3 jump = Rbody.transform.up * JumpMomentum * Time.deltaTime;


        // move pos with forward, jump and gravity
        Rbody.MovePosition(Rbody.position + forward + gravity + jump); 
    }

    void handleFalling(out Vector3 gravity)
    {
        gravity = new Vector3();
        JumpLockout = false;
        const float EPSILON = 1E-6f;
        float fallSpeed = -9.81f * Time.deltaTime * (FallMomentum++ / 55);

        // Spherecasting
        RaycastHit hit;
        float dist = Mathf.Infinity;
        if (Physics.SphereCast(Rbody.position + CapColl.center, CapColl.height / 2, -Rbody.transform.up, out hit, 100)) dist = hit.distance;
        Grounded = (dist <= EPSILON) ? true : false;

        Dist = dist;

        if (Grounded)
        {
            Anim.SetBool("Grounded", true);
            Anim.speed = 1.0f;
            FallMomentum = 1;

            // Check Platform
            TipToePlatform ttplat = hit.collider.GetComponent<TipToePlatform>();
            if (ttplat) 
            {
                ttplat.CharacterTouches();
                if (ttplat.Dead()) JumpLockout = true;
            }

            GameObject finish = GameObject.FindGameObjectWithTag("Finish");
            if (finish == hit.collider.gameObject) 
            {
                finish.GetComponent<WinScript>().win();
            }
        } else {
            Anim.SetBool("Grounded",false);
            // Clipping check
            if (dist < Mathf.Abs(fallSpeed)) 
            {
                gravity = new Vector3(0,-dist/1.2f,0); // divide by factor to help against clipping
            } else 
            {
                gravity = new Vector3(0, fallSpeed, 0); // normal gravity
            }

        }


        // Reset if jumped off arena
        if (Rbody.position.y <= -10) Rbody.position = InitialStartPosition;
    }

    void handleJumping()
    {
        // only jump if grounded
        if (Input.GetKey("space") && Grounded && !JumpLockout)
        {
            JumpMomentum = 14.5f;
            transform.position = Rbody.position;
        } else
        {
            // decay jump momentum
            updateMomentumWithinBounds(ref JumpMomentum, false, MomentumTypes.JumpMomentum);
        }
    }

    void handleWalkRotation()
    {
        // No WASD Input detected
        if (Dir.x == 0 && Dir.y == 0 && Dir.z == 0) {
            Anim.SetFloat("Speed", RunMomentum); // IDLE Animation
            updateMomentumWithinBounds(ref RunMomentum, false, MomentumTypes.WalkMomentum); // lose momentum
        } else 
        {
            // WASD Input detected - start running
            updateMomentumWithinBounds(ref RunMomentum, true, MomentumTypes.WalkMomentum); // gain momentum

            // apply rotation/forward transformation
            Quaternion look = Quaternion.LookRotation(Dir);
            Quaternion lookY = Quaternion.Euler(0, look.eulerAngles.y, 0); // only use y-axis for rotation
            Rbody.MoveRotation(Quaternion.Lerp(Rbody.rotation, lookY, RotationSpeed));
            Anim.SetFloat("Speed", RunMomentum); // Running animation
        }
    }

    // *** Helper Functions ***
    // update float with grow/decay factor withing respective bounds
    void updateMomentumWithinBounds(ref float val, bool growing, MomentumTypes mType)
    {
        float growthFactor = 1.0f;
        float decayFactor = 1.0f;

        switch (mType)
        {
            case MomentumTypes.WalkMomentum:
                growthFactor = 2.5f;
                decayFactor = 3.8f;
                break;
            case MomentumTypes.JumpMomentum:
                growthFactor = 15.8f;
                decayFactor = 15.5f;
                break;
        }

        if (growing)
        {
            val += growthFactor * Time.deltaTime;
        } else 
        {
            val -= decayFactor * Time.deltaTime;
        }

        switch (mType)
        {
            case MomentumTypes.WalkMomentum:
                val = Mathf.Clamp(val, 0.0f, 1.0f);
                break;
            case MomentumTypes.JumpMomentum:
                val = Mathf.Clamp(val, 0.0f, 45.0f);
                break;
        }
    }

    Vector3 WASD_Dir()
    {
        Vector3 direction = new Vector3(0, 0, 0);
        GameObject cam = GameObject.Find("Main Camera");
        if (Input.GetKey("w"))
        {
            direction = cam.transform.forward;
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
        return direction;
    }
}
