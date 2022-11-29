using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacterControls : MonoBehaviour
{
    // ***** Public: ******
    public float TurnSmoothTime;
    public float Speed;
    
    // ***** Private: *****
    // Ragdoll
    private RagdollController RagdController;

    // Character
    private Animator Anim;
    private Rigidbody Rbody;
    private CapsuleCollider CapCol;
    private GameObject Cam;

    // User Input Walking Direction
    private Vector3 AxisDir;

    // Motion States
    private bool Grounded = true;
    private bool Standing = true;
    private bool CurrentlyStandingUp = false;

    // Physics / Movement Vals
    private float MovementSpeedFactor = 0;
    private float TurnSmoothVelocity;

    // Animator Movementspeed value
    private float AnimationSpeedValue = 0; 
    private const float MIN_ANIMATION_SPEED_VALUE = 0.1f;

    // Gravity Factor - increased while falling
    private float GravityFactor = 1f; 
    private const float MAX_GRAVITY_FACTOR = 5f;

    // Standing Up Timer
    private float Timer_StandingUp = 0;
    private const float MAX_TIME_STANDING_UP = 3.0f;

    // Jumping Cooldown Timer
    private float Timer_JumpingCooldown = 0;
    private const float MAX_TIME_JUMPING_COOLDOWN = 1.2f;

    // Slip Angle Timer
    private float Timer_OnSlipAngle = 0;
    private const float MAX_TIME_ON_SLIP_ANGLE = 1f;
    private const float MAX_SLIP_ANGLE = 45f;


    void Start()
    {
        // Ragdoll Script Controller
        RagdController = GetComponent<RagdollController>();
        RagdController.disable();

        // Get Character Components
        Anim = GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody>();
        CapCol = GetComponent<CapsuleCollider>();

        // Ignore Character & RagdollCollisions
        Physics.IgnoreLayerCollision(7, 6, true); 

        // Needed for Rotating Character according to Camera-Position
        Cam = GameObject.Find("Main Camera");
    }

    // each Frame
    private void Update() {

        // Ragdoll / Animator interaction
        Anim.enabled = (Standing || CurrentlyStandingUp) ? true : false;

        bool SmoothedCam = (RagdController.active()) ? true : false;
        Cam.GetComponent<SimpleObjectCamera>().smooth(SmoothedCam);

        // Player Input walking Dir
        inputAxisDir(ref AxisDir);
    }

    // Once per physics step
    private void FixedUpdate() {

        // Physics Steps
        handleFalling();
        handleStandingUp();
        handleRunning();

        // Animator Passing
        Anim.SetBool("Standing", Standing);
        Anim.SetBool("Grounded", Grounded);
        Anim.SetFloat("Speed", AnimationSpeedValue);

        // Throw Ragdoll
        if (!RagdController.active() && Input.GetKey("e"))
        {
            Standing = false;
            Anim.enabled = false;
            RagdController.enable();
            RagdController.applyForce(-Rbody.velocity.normalized, Rbody.velocity.magnitude);
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
        Timer_StandingUp = (!Standing && RagdController.torsoVelocity() <= 0.5f) ? Timer_StandingUp += Time.deltaTime : Timer_StandingUp = 0;

        // Stand up if timer reached
        if (Timer_StandingUp >= MAX_TIME_STANDING_UP)
        {
            Anim.applyRootMotion = true;
            CurrentlyStandingUp = true;
            Rbody.isKinematic = false;
            Rbody.detectCollisions = true;
            RagdController.applyArmatureRoot();

            // keep y rotation while standing up
            Quaternion endRotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w); 
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime * 5.5f);

            // Play Standing Up Animation
            Anim.Play("Standing_Up_Exit");
            if (RagdController.active()) RagdController.disable();

            // Standing Up End-state
            if (transform.rotation == endRotation) {
                Timer_StandingUp = 0;
                Standing = true;
                CurrentlyStandingUp = false;
                Anim.applyRootMotion = false;
                RagdController.applyArmatureRoot();
            }
        } 

    }

    // *** Input, Rotation & Running ***
    private void handleRunning()
    {

        if (!Standing)
        {
            incrementFloatWithinBounds(ref AnimationSpeedValue, 0, 1, -0.02f);
            return;
        }

        // Sprinting
        MovementSpeedFactor = (Input.GetKey("left shift")) ? Speed * 2.3f : Speed;
        if (AxisDir.magnitude >= MIN_ANIMATION_SPEED_VALUE)
        {
            // atan2(x, z) gives angle between vector(x,z) [-pi, pi], Cam Rotation is on y
            float targetAngle = (Mathf.Atan2(AxisDir.x, AxisDir.z) * Mathf.Rad2Deg) + Cam.transform.eulerAngles.y;

            // Smooth Rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref TurnSmoothVelocity, TurnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // Apply Rotation

            Rbody.AddForce(moveDir * MovementSpeedFactor);

            incrementFloatWithinBounds(ref AnimationSpeedValue, 0, 1, 0.02f);
        } else {
            incrementFloatWithinBounds(ref AnimationSpeedValue, 0, 1, -0.02f);
        }
    }

    // *** Jumping & Gravity *** 
    private void handleFalling()
    {
        // Grounded, reset falling
        if (Grounded && Standing) 
        {
            GravityFactor = 1; // Reset Gravity Factor
            if (Input.GetKey("space") && Timer_JumpingCooldown >= MAX_TIME_JUMPING_COOLDOWN)
            {
                Rbody.AddForce(transform.up * 1750);
                Timer_JumpingCooldown = 0;
            } else
            {
                Timer_JumpingCooldown += Time.deltaTime;
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
        Timer_OnSlipAngle = (Grounded && groundAngle > MAX_SLIP_ANGLE) ? Timer_OnSlipAngle + Time.deltaTime : 0;
        if (Timer_OnSlipAngle >= MAX_TIME_ON_SLIP_ANGLE)
        {

            RagdController.enable();
            Standing = false;
            Timer_OnSlipAngle = 0;
        }
    }

    // Checks Collisions for ParcourObjects
    // If Impulse great enough -> Ragdoll
    // test to branchdasdas d
    // asdas d
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