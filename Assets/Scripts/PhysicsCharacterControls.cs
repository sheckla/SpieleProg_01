using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacterControls : MonoBehaviour
{
    // Ragdoll
    private Rigidbody[] Rigidbodies;
    private Collider[] Colliders;
    private CharacterJoint[] Joints;
    public GameObject StandUpTarget;


    private Animator Anim;
    private Rigidbody Rbody;
    private CapsuleCollider CapCol;
    private GameObject Cam;

    public float TurnSmoothTime;
    public float Speed;

    private Vector3 AxisDir;
    private bool Grounded = true;
    private bool Standing = true;
    private float CurrentMovementSpeed = 0;
    private float AnimSpeedVal = 0;
    public float StandingUpTimer = 0;
    private float JumpingCooldown = 0;
    private float GravityFactor = 1;
    private float TurnSmoothVelocity;
    private bool RootMotionApplied = false;
    private int RootMotionAppliedIterations = 0;
    private bool StandUpTargetSet = false;

    void Start()
    {
        // Get Ragdoll Components
        GameObject Armature = GameObject.Find("Armature");
        Rigidbodies = Armature.GetComponentsInChildren<Rigidbody>();
        Colliders = Armature.GetComponentsInChildren<Collider>();
        Joints = Armature.GetComponentsInChildren<CharacterJoint>();

        // Get Character Components
        Anim = GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody>();
        CapCol = GetComponent<CapsuleCollider>();
        Cam = GameObject.Find("Main Camera");

        // Init
        Physics.IgnoreLayerCollision(7, 6, true);
        disableRagdoll();
    }

    void disableRagdoll()
    {
        foreach (Collider col in Colliders)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rbody in Rigidbodies)
        {
            rbody.isKinematic = true;
            rbody.detectCollisions = false;
        }

        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = false;
        }
        Standing = true;
    }

    void enableRagdoll()
    {
        foreach (Collider col in Colliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rbody in Rigidbodies)
        {
            rbody.isKinematic = false;
            rbody.detectCollisions = true;
            Rigidbody targetRbody = StandUpTarget.GetComponent<Rigidbody>();
            targetRbody.AddForce(Rbody.velocity);
            rbody.useGravity = true;
        }

        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = true;
        }
        Standing = false;
    }

    // each Frame
    private void Update() {

        // Ragdoll / Animator interaction
        Anim.enabled = (Standing) ? true : false;

        // Player Input
        inputAxisDir(ref AxisDir);
    }

    // Once per physics step
    private void FixedUpdate() {
        handleStandingUp();
        handleFalling();
        if (Standing) handleRunning();
        applyRootMotionIterations();
        Anim.SetBool("Standing", Standing);
        Anim.SetBool("Grounded", Grounded);
        Anim.SetFloat("Speed", AnimSpeedVal);
    }

    private void applyRootMotionIterations()
    {
        // Apply Root Motion several times after standing up
        if (Standing && !RootMotionApplied && RootMotionAppliedIterations < 20)
        {
            Anim.applyRootMotion = true;
            RootMotionApplied = true;
            RootMotionAppliedIterations++;
        } else 
        {
            Anim.applyRootMotion = false;
            RootMotionAppliedIterations = 0;
        }

    }

    private Vector3 inputAxisDir(ref Vector3 vec)
    {
        // Inputs as int [0 | 1]
        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical"); 
        vec = new Vector3(horizontal, 0f, vertical).normalized;
        return vec;
    }

    private void handleStandingUp()
    {
        // Stand up
        if (!Standing && StandingUpTimer >= 3)
        {
                foreach (Rigidbody rbody in Rigidbodies)
            {
                rbody.isKinematic = true;
            }
            lockRigidbodyConstraints();

            if (!StandUpTargetSet) 
            {
            BoxCollider col = StandUpTarget.GetComponent<BoxCollider>();
            Rbody.position = StandUpTarget.transform.position;
            Rbody.rotation = StandUpTarget.transform.rotation;
            StandUpTargetSet = true;
            }

            // keep y rotation while standing up
            Quaternion endRotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w); 
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime * 3.5f);

            // Standing Up End-state
            if (transform.rotation == endRotation) {
                StandingUpTimer = 0;
                Standing = true;
                disableRagdoll();
                RootMotionApplied = false;
                StandUpTargetSet = false;
            }
        } else
        {
            // Character currently in falling motion
            StandingUpTimer = (!Standing && Rbody.velocity.magnitude <= 0.9f) ? StandingUpTimer += Time.deltaTime : StandingUpTimer = 0;
        }



    }

    // *** Input, Rotation & Running ***
    private void handleRunning()
    {
        // Sprinting
        CurrentMovementSpeed = (Input.GetKey("left shift")) ? Speed * 2.3f : Speed;

        if (AxisDir.magnitude >= 0.1f)
        {
            // atan2(x, z) gives angle between vector(x,z), Cam Rotation is on y
            float targetAngle = (Mathf.Atan2(AxisDir.x, AxisDir.z) * Mathf.Rad2Deg) + Cam.transform.eulerAngles.y;

            // Smooth Rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref TurnSmoothVelocity, TurnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // Apply Rotation

            Rbody.AddForce(moveDir * CurrentMovementSpeed);

            incrementFloatWithinBounds(ref AnimSpeedVal, 0, 1, 0.02f);
        } else
        {
            incrementFloatWithinBounds(ref AnimSpeedVal, 0, 1, -0.02f);
        }
    }

    // *** Jumping & Gravity *** 
    private void handleFalling()
    {
        // Grounded, reset falling
        if (Grounded && Standing) 
        {
            GravityFactor = 1; // Reset Gravity Factor
            if (Input.GetKey("space") && JumpingCooldown >= 1.3f)
            {
                Rbody.AddForce(transform.up * 1750);
                JumpingCooldown = 0;
            } else
            {
                JumpingCooldown += Time.deltaTime;
            }

        // Init Falling
        } else {
            Vector3 gravityVelocity =  -Vector3.up * 9.81f * incrementFloatWithinBounds(ref GravityFactor, 1, 10, Time.deltaTime * 5);
            Rbody.AddForce(gravityVelocity);
        } 

        // Spherecasting
        RaycastHit hit;
        float dist = Mathf.Infinity;
        if (Physics.SphereCast(transform.position + transform.up, CapCol.radius, -transform.transform.up, out hit, 100)) dist = hit.distance;
        Grounded = (dist <= CapCol.radius + 1E-6f) ? true : false;

        // Check Ground slip angle 
        float groundAngle = Mathf.Acos(Vector3.Dot(hit.normal, transform.up)) * Mathf.Rad2Deg;
        if (Grounded && groundAngle > 45)
        {
            freeRigidbodyConstraints();
            enableRagdoll();
            Standing = false;
        }
    }

    // Checks Collisions for ParcourObjects
    // If Impulse great enough -> Ragdoll
    private void OnCollisionEnter(Collision other) {
        print("Collision with" + other.gameObject.tag + ", impulse=" + other.impulse.magnitude);
        if (other.gameObject.CompareTag("ParcourObject") && other.impulse.magnitude >= 20)
        {
            freeRigidbodyConstraints();
            enableRagdoll();
            Standing = false;
        }
    }

    private void freeRigidbodyConstraints()
    {
        Rbody.constraints = RigidbodyConstraints.None;
    }

    private void lockRigidbodyConstraints()
    {
        Rbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private float incrementFloatWithinBounds(ref float val, float min, float max, float fac)
    {
        val += fac;
        if (val < min) val = min;
        if (val > max) val = max;
        return val;
    }
}
