using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovementControls))]
[RequireComponent(typeof(Animator))]
public class PlayerInputHandler : MonoBehaviour
{
    /******************************
    * Public: 
    *******************************/
    public SimpleObjectCamera Cam;
    public RagdollController RagdController;
    public CharacterController CharController;
    [Range(.0f, 1.0f)] public float SmoothRunning = .5f;


    /******************************
    * Private: 
    *******************************/
    private CharacterMovementControls CharacterMovementControls;
    private Animator Anim;

    // Hotkey - "1"
    private InputToggle Toggle_RotateTowardsCamera = new InputToggle(KeyCode.Alpha1);
    private bool LockModelToTarget = false;

    // Hotkey - "2"
    private InputToggle Toggle_FixedCamera = new InputToggle(KeyCode.Alpha2);
    private bool FixedCamera = false;

    // Hotkey - "q"
    private InputToggle Toggle_Aiming = new InputToggle(KeyCode.Q);
    private bool Aiming = false;

    // Hotkey - "left shift"
    private InputToggle Toggle_Crouching = new InputToggle(KeyCode.LeftShift);
    private FloatRange CrouchingAnimValue = new FloatRange(0.0f, 1.0f);

    // Hotkey - "Mouse Left" 
    private InputToggle Toggle_Shooting = new InputToggle(KeyCode.Mouse0);
    private bool Shooting = false;


    // Smoothdamp References
    private float TurnSmoothVelocity;
    private Vector3 SmoothCurrentVelocity;

    // WASD Input Walking Direction
    public Vector3 SmoothedInputAxisDir = new Vector3(.0f, .0f, .0f);

    void Start()
    {
        CharacterMovementControls = GetComponent<CharacterMovementControls>();
        CharController = GetComponent<CharacterController>();
        CharacterMovementControls.Cam = Cam;
        Anim = GetComponent<Animator>();
        RagdController.disable();
        Physics.IgnoreLayerCollision(6,7, true);
    }

    void Update()
    {
        // Hotkey toggles
        handleKeyToggles();

        // User WASD Input
        getUserInputDirection();

        updateAnimationValues();

        bool sprinting = false;
        if (Input.GetKey("left shift")) sprinting = true;

        bool jumping = false;
        if (Input.GetKey("space")) jumping = true;

        if (Input.GetKey("e")) {
            RagdController.enable();
            Anim.enabled = false;
        }
        if (Input.GetKey("r"))
        {
            RagdController.disable();
            Anim.enabled = true;
        } 

        // Pass User-Input to Movement-Script
        CharacterMovementControls.Move(SmoothedInputAxisDir, jumping, sprinting);
    }

    private void handleKeyToggles() 
    {
        Toggle_RotateTowardsCamera.update();
        Toggle_FixedCamera.update();
        Toggle_Aiming.update();
        Toggle_Crouching.update();
        Toggle_Shooting.update();
        Shooting = Toggle_Shooting.active();
    }

    private void updateAnimationValues() 
    {
        // Movement Script Physics status
        bool grounded;
        float rotation;
        CharacterMovementControls.movementInfo(out grounded, out rotation);

        // Low value check
        if (Mathf.Abs(SmoothedInputAxisDir.z) < 0.01f) SmoothedInputAxisDir.z = 0;
        if (Mathf.Abs(SmoothedInputAxisDir.x) < 0.01f) SmoothedInputAxisDir.x = 0;


    }

    private void getUserInputDirection()
    {
        // Inputs as int [0 | 1]
        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical"); 
        Vector3 rawDir = new Vector3(horizontal, 0f, vertical);

        // Smooth Damp Input
        SmoothedInputAxisDir = Vector3.SmoothDamp(SmoothedInputAxisDir, rawDir, ref SmoothCurrentVelocity, 30 * SmoothRunning * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TipToePlatform plat = hit.gameObject.GetComponent<TipToePlatform>();
        if (plat)
        {
            plat.CharacterTouches();
            print("plathit");
        }

        if (hit.gameObject.tag == "ParcourObject") {
            RagdController.enable();
            Anim.enabled = false;
        }
    }
}
