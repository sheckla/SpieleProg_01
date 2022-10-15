using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterControl : MonoBehaviour
{
    // *** PUBLIC
    [SerializeField]
    public float RotationSpeed;
    public float MovementSpeed;     

    // Smoothed movement momentum vals
    public float FallMomentum = 1; // normal 1-step increments/decrements
    enum MomentumTypes {WalkMomentum, JumpMomentum} // specific decay/growth for smooth transitions
    public float RunMomentum = 0; 
    public float JumpMomentum = 0;

    // *** PRIVATE
    private Animator Anim;
    private Rigidbody Rbody;
    private CapsuleCollider CapColl;

    private Vector3 InitialStartPosition; // for reseting to start
    private Vector3 Dir; // global access for walking dir
    private bool JumpLockout; // prevent jumping when standing on bait-platform
    private bool Grounded;

    // Before first frame - Initialisation
    void Start()
    {
        Anim = GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody>();
        CapColl = GetComponent<CapsuleCollider>();
        InitialStartPosition = Rbody.position;
    }

    // Each Frame
    void Update() 
    {
        // get direction via. keyboard inputs [W,A,S,D]
        Dir = WASD_Dir(); // save cpu time by only calling each frame and not each Physics-Frame
    }

    // Each Physics Step - [edit -> project_settings -> time- > fixed_timestep]
    void FixedUpdate()
    {
        Vector3 gravVec;
        Vector3 runVec;
        Vector3 jumpVec;

        handleRunning(out runVec);
        handleJumping(out jumpVec);
        handleFalling(out gravVec); 

        // Transform Rigidbidy with Running, Jumping and Gravity Translation
        Rbody.MovePosition(Rbody.position + (gravVec + runVec + jumpVec)); 
    }

    void handleFalling(out Vector3 gravity)
    {
        JumpLockout = false;
        gravity = new Vector3();
        const float EPSILON = 1E-6f;
        float fallSpeed = -9.81f * Time.deltaTime * (FallMomentum++ / 55);

        // Spherecasting
        RaycastHit hit;
        float dist = Mathf.Infinity;
        if (Physics.SphereCast(Rbody.position + CapColl.center, CapColl.height / 2, -Rbody.transform.up, out hit, 100)) dist = hit.distance;
        Grounded = (dist <= EPSILON) ? true : false;

        if (Grounded)
        {
            Anim.SetBool("Grounded", true);
            FallMomentum = 1; // reset Gravity momentum

            // Check Platform
            TipToePlatform ttplat = hit.collider.GetComponent<TipToePlatform>();
            if (ttplat) 
            {
                ttplat.CharacterTouches();
                if (ttplat.Dead()) JumpLockout = true; // prevent Jumping on already dead platform
            }

            // Shiny sparkles when winning ;)
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

    void handleJumping(out Vector3 jumpVec)
    {
        if (Input.GetKey("space") && Grounded && !JumpLockout)
        {
            JumpMomentum = 14.5f;
        } else
        {
            // decay jump momentum
            updateMomentumWithinBounds(ref JumpMomentum, false, MomentumTypes.JumpMomentum);
        }
        jumpVec = Rbody.transform.up * JumpMomentum * Time.deltaTime;
    }

    void handleRunning(out Vector3 runVec)
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

        runVec = Rbody.transform.forward * Time.deltaTime * RunMomentum * MovementSpeed;
    }

    // *** Helper Functions ***
    // update float with grow/decay factor within respective bounds
    void updateMomentumWithinBounds(ref float val, bool growing, MomentumTypes mType)
    {
        // Defaults
        float growthFactor = 1.0f;
        float decayFactor = 1.0f;

        // Assign specific Grow/Decay
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
        
        // Grow/Decay
        val = (growing) ? val += growthFactor * Time.deltaTime : val-= decayFactor * Time.deltaTime;

        // Clamp withing specific bounds
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

    // Adds up multiple Keyboard inputs into Dir
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
