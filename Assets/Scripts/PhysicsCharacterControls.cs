using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacterControls : MonoBehaviour
{
    // Ragdoll
    private RagdollController RagdController;

    // Character
    private Animator Anim;
    private Rigidbody Rbody;
    private CapsuleCollider CapCol;
    private GameObject Cam;

    // Public
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
    private bool CurrentlyStandingUp = false;

    public float RootMotionTimer = 0;

    void Start()
    {
        RagdController = GetComponent<RagdollController>();

        // Get Character Components
        Anim = GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody>();
        CapCol = GetComponent<CapsuleCollider>();
        Cam = GameObject.Find("Main Camera");

        // Init
        Physics.IgnoreLayerCollision(7, 6, true); // Disable Rigidbody and Ragdoll Collisions
        RagdController.disable();
    }

    // each Frame
    private void Update() {

        // Ragdoll / Animator interaction
        Anim.enabled = (Standing || CurrentlyStandingUp) ? true : false;

        // Player Input
        inputAxisDir(ref AxisDir);
    }

    // Once per physics step
    private void FixedUpdate() {
        handleFalling();
        handleStandingUp();
        handleRunning();

        if (Input.GetKey("e"))
        {
            Standing = false;
            Anim.enabled = false;
            RagdController.enable();
            RagdController.applyForce(new Vector3(0f,1f,0), 10);
        }

        Anim.SetBool("Standing", Standing);
        Anim.SetBool("Grounded", Grounded);
        Anim.SetFloat("Speed", AnimSpeedVal);
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

        // Character Rigidbody follow Ragdoll Torso
        if (!Standing && !CurrentlyStandingUp)
        {
            BoxCollider col = RagdController.torsoBoxCollider();
            CapsuleCollider[] cols = RagdController.feetCollider();
            Vector3 pos1 = cols[0].transform.position;
            Vector3 pos2 = cols[1].transform.position;
            Rbody.position = Vector3.Lerp(Rbody.position, pos1 + (pos2 - pos1) * 0.5f, 0.21f);
            Rbody.rotation = Quaternion.Euler(col.transform.eulerAngles);
            Rbody.isKinematic = true;
            Rbody.detectCollisions = false;
        }

        // Character currently in falling motion
        StandingUpTimer = (!Standing && RagdController.torsoVelocity() <= 0.5f) ? StandingUpTimer += Time.deltaTime : StandingUpTimer = 0;

        // Stand up if timer reached
        if (StandingUpTimer >= 3)
        {
            Anim.applyRootMotion = true;
            CurrentlyStandingUp = true;
            Rbody.isKinematic = false;
            Rbody.detectCollisions = true;

            // keep y rotation while standing up
            Quaternion endRotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w); 
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime * 5.5f);

            // Play Standing Up Animation
            Anim.Play("Standing_Up_Exit");
            if (RagdController.active()) RagdController.disable();

            // Standing Up End-state
            if (transform.rotation == endRotation) {
                StandingUpTimer = 0;
                Standing = true;
                CurrentlyStandingUp = false;
                Anim.applyRootMotion = false;
            }
        } 
    }

    // *** Input, Rotation & Running ***
    private void handleRunning()
    {

        if (!Standing)
        {
            incrementFloatWithinBounds(ref AnimSpeedVal, 0, 1, -0.02f);
            return;
        }

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
        } else {
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
            RagdController.enable();
            Standing = false;
        }
    }

    // Checks Collisions for ParcourObjects
    // If Impulse great enough -> Ragdoll
    private void OnCollisionEnter(Collision other) {
        Rigidbody r = other.rigidbody;
        if (r == null) return;
        print("Collision with" + other.gameObject.tag + ", impulse=" + other.impulse.magnitude * other.rigidbody.mass);
        if (other.gameObject.CompareTag("ParcourObject") && other.impulse.magnitude * other.rigidbody.mass >= 100)
        {
            //freeRigidbodyConstraints();
            if (!RagdController.active()) RagdController.enable();
            RagdController.applyForce(-Rbody.velocity.normalized, other.impulse.magnitude);
            Standing = false;
        }
    }

    private float incrementFloatWithinBounds(ref float val, float min, float max, float fac)
    {
        val += fac;
        if (val < min) val = min;
        if (val > max) val = max;
        return val;
    }
}


/* Standing up rotation
// keep y rotation while standing up
            Quaternion endRotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w); 
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime * 3.5f);

            // Standing Up End-state
            if (transform.rotation == endRotation) {
                StandingUpTimer = 0;
                Standing = true;
                RagdController.disable();
            }
            */