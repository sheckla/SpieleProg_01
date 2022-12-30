using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CooldownTimer;
using static FloatRange;

/******************************
Movement Script
- Set MoveDir each Frame with parameters -> 'Move()'
*******************************/
[RequireComponent(typeof(CharacterController))]
public class CharacterMovementControls : MonoBehaviour
{
    /******************************
    * Public: 
    *******************************/
    [Range(0.0f, 1.0f)] public float Speed = 0.5f;
    [Range(1.0f, 2.0f)] public float SprintSpeed = 1.2f;
    [Range(0.0f, 1.0f)] public float ModelRotationSpeed = 0.8f;
    [Range(.0f, 1.0f)] public float JumpHeight = .5f;
    [Range(.0f, 1.0f)] public float AirMovement = .5f;
    [Range(.0f, 10.0f)] public float Gravitation = 3.0f;
    public bool RotateTowardsCamera = false;
    public SimpleObjectCamera Cam;
    public bool forceJump = false;

    /******************************
    * Private: 
    *******************************/
    private CharacterController CharacterController;

    // Physics / Movement Vals
    private Vector3 MoveDirection; // ** Has to be set externally! ** 
    private float SprintSpeedMultiplier;
    private bool Sprinting;

    // Jumping / Gravity
    private bool Jumping;
    private FloatRange JumpingMomentum = new FloatRange(0.0f, 30.0f);
    private CooldownTimer CooldownTimer_Jumping = new CooldownTimer(0.1f);

    private Vector3 _GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);
    private FloatRange GravityFactor = new FloatRange(1.0f, 28.0f);
    
    private bool InAir = false;
    private Vector3 Initial_InAirVelocity;
    private Vector3 Current_InAirVelocity;

    // Animator Speed Value
    private FloatRange AnimationSpeedValue = new FloatRange(0.0f, 1.0f);

    /******************************
    * Placeholder Variables (can be ignored): 
    *******************************/
    private float TurnSmoothVelocity;
    private int StaticSpeedMultiplier = 100;
    public float currentAngle;

    void Start()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    public void Move(Vector3 axisDir, bool jumping, bool sprinting) 
    {
        this.MoveDirection = axisDir;
        Jumping = jumping;
        Sprinting = sprinting;
    }

    private void Update() {
        // Initial (neutral) velocity
        Vector3 runningVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 jumpingVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 gravityVelocity = _GRAVITY;
        float rotationTargetAngle = transform.eulerAngles.y;

        // Calculate Velocities
        JumpingVelocity(ref jumpingVelocity); 
        GravityVelocity(ref gravityVelocity); 
        RunningVelocity(ref runningVelocity, ref rotationTargetAngle); // also gets smoothed target angle

        // Apply Movement Transformations
        Vector3 totalMovementVelocity = new Vector3(0,0,0);
        if (CharacterController.isGrounded) {
            // 1. Normal Walking
            totalMovementVelocity = runningVelocity + jumpingVelocity + gravityVelocity;
        } else 
        {
            // 2. In Air Movement
            Current_InAirVelocity += runningVelocity * AirMovement;
            Current_InAirVelocity = Vector3.ClampMagnitude(Current_InAirVelocity, 100 * Speed);
            totalMovementVelocity = Current_InAirVelocity + jumpingVelocity + gravityVelocity;
        }

        // Apply Movement & Rotation
        CharacterController.Move(totalMovementVelocity * Time.deltaTime);
        applyRotation(rotationTargetAngle);
    }

    // *** Rotation & Running ***
    private void RunningVelocity(ref Vector3 runningDirVec, ref float targetAngle)
    {
        // Sprinting
        SprintSpeedMultiplier = (Sprinting) ? Speed * SprintSpeed * StaticSpeedMultiplier: Speed * StaticSpeedMultiplier;

        // Running Velocity Calculation
        if (MoveDirection.magnitude > 0.01f)
        {
            // If Camera attached - get cam rotation
            float cameraY = 0;
            if (Cam) cameraY = Cam.transform.eulerAngles.y;

            // atan2(x, z) gives angle between vector(x,z) [-pi, pi], Cam Rotation is on y
            targetAngle = (Mathf.Atan2(MoveDirection.x, MoveDirection.z) * Mathf.Rad2Deg) + cameraY;

            // Final Running Velocity
            runningDirVec =Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * Mathf.Clamp(MoveDirection.magnitude, 0, 1) * SprintSpeedMultiplier;
        } 
    }

    private void applyRotation(float targetAngle) {
        
        // Rotate Model to Camera if Cam attached
        if (RotateTowardsCamera && Cam) {
            targetAngle = Cam.transform.eulerAngles.y;
        }
        currentAngle = targetAngle;
        // Smooth Rotation
        float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref TurnSmoothVelocity, 1.0f-ModelRotationSpeed);

        // Apply Rotation
        transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }

    // *** Jumping *** 
    private void JumpingVelocity(ref Vector3 jumpingDir)
    {
        if (CharacterController.isGrounded) 
        {
            // Reset Jumpingmomentum & GravityMomentum
            JumpingMomentum.setMin();
            GravityFactor.setMin();

            // Space Input Toggle
            if (Jumping && !CooldownTimer_Jumping.onCooldown()) 
            {
                JumpingMomentum.set(JumpingMomentum.Max);
                CooldownTimer_Jumping.start();
            }

            if (forceJump) JumpingMomentum.set(JumpingMomentum.Max);

            CooldownTimer_Jumping.update(Time.deltaTime);
        } else {
            // Decrement JumpingMomentum
            JumpingMomentum.set(JumpingMomentum.value() + -(JumpingMomentum.Max) * Time.deltaTime);
        }

        jumpingDir = JumpingMomentum * new Vector3(0.0f, 5.0f, 0.0f) * JumpHeight;
    }

    private void GravityVelocity(ref Vector3 gravity)
    {
        // Is Falling
        if (!CharacterController.isGrounded) 
        {
            // Generate AirMovement Snapshot
            if (!InAir) {
                Initial_InAirVelocity = CharacterController.velocity;
                Initial_InAirVelocity.y = 0;
                Current_InAirVelocity = Initial_InAirVelocity;
                InAir = true;
            }

            // Increase Gravity over time
            GravityFactor.set(GravityFactor.value() +  Gravitation * Time.deltaTime);
        } 
        // Is Grounded
        else {
            // Reset Gravity
            GravityFactor.setMin();
            InAir = false;
        }
        gravity = _GRAVITY * GravityFactor;
    }

    public void movementInfo(out bool grounded, out float rotation)
    {
        grounded = CharacterController.isGrounded;

        // Current Model-Rotation Factor from [0,1]
        Quaternion current = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        Quaternion target = Quaternion.Euler(0, currentAngle, 0);
        rotation = MathUtil.remapBounds(Quaternion.Angle(current, target), 0.0f, 180.0f, 0.0f, 1.0f);
        if (Mathf.Abs(rotation) < 0.0001) rotation = 0;
    }
}