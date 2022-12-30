using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavRunnerScript : MonoBehaviour
{
    public GameObject Target;
    public Vector3 TargetPosition;
    public RagdollController Ragdoll;
    public Animator Animator;
    public GameObject RespawnPos;
    public GameObject TargetSphere;
    public Vector3 desiredVel;


    private bool InRagdoll = false;
    private NavMeshAgent Agent;
    private CharacterMovementControls Controls;
    private CharacterController CharController;
    private float RagdollTimer = 0.0f;
    private GameObject[,] platforms;
    public bool[,] discovered;
    private int width;
    private int depth;
    public int currentI = 0;
    public int currentJ = 0;
    private int random;


    // Start is called before the first frame update
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Controls = GetComponent<CharacterMovementControls>();
        Ragdoll = GetComponent<RagdollController>();
        Animator = GetComponent<Animator>();
        random = Random.Range(0,2);

        CharController = GetComponent<CharacterController>();
        if (Ragdoll) Ragdoll.disable();
        FindObjectOfType<TipToeLogic>().getPlatforms(ref platforms, ref width, ref depth);
        discovered = new bool[width, depth];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < depth; j++) {
                discovered[i,j] = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Agent.updatePosition = false;
        Agent.updateRotation = false;


        Agent.destination = Target.transform.position;
        desiredVel = Agent.desiredVelocity;

        bool jumping = false;
        if (Agent.desiredVelocity.y > 0.01f) jumping = true;

        if (!InRagdoll) {
            Controls.Move(Agent.desiredVelocity, jumping, false);
            CharController.detectCollisions = true;
            Animator.enabled = true;
            Ragdoll.disable();
            Agent.enabled = true;
            CharController.enabled = true;
        } 

        if (InRagdoll) {
            Animator.enabled = false;
            Ragdoll.enable();
            Agent.enabled = false;
            RagdollTimer+= Time.deltaTime;
            Controls.Move(new Vector3(0,0,0), false, false);
            CharController.enabled = false;
            if (RagdollTimer >= 2.0f) {
                InRagdoll = false;
                RagdollTimer = 0.0f;
            }
            return;
        }

        Animator.SetBool("Grounded", GetComponent<CharacterController>().isGrounded);
        Animator.SetFloat("Speed", Agent.desiredVelocity.magnitude);

        // Fallen off arena
        if (transform.position.y < -20) {
            transform.position = RespawnPos.transform.position;
            Agent.transform.position = RespawnPos.transform.position;
            Agent.enabled = false;
            Agent.enabled = true;
            currentI = 0;
            currentJ = 0;
        }

        Agent.transform.position = CharController.transform.position;
        Agent.velocity = CharController.velocity;
        Agent.nextPosition = CharController.transform.position;
        setNextTargetPlatform();
    }

    // x = col, y = row
    void setNextTargetPlatform() {

        // Check for already visible path
        for (int i = 0 + currentI; i < depth; i++) {
            for (int j = 0 + currentJ; j < width; j++) {
                if (platforms[j,i].GetComponent<TipToePlatform>().isTaggedPath) {
                    TargetPosition = platforms[j,i].transform.position;
                    currentI = i;
                    currentJ = j;
                    TargetSphere.transform.position = platforms[j,i].transform.position;
                    Agent.SetDestination(TargetSphere.transform.position);
                    return;
                }
            }
        } 

        // Randomly access path
        for (int i = 0 + currentI; i < depth; i++) { // depth
            for (int j = 0 + currentJ; j < width; j++) {
                j += random;
                j = Mathf.Clamp(j, 0, width-1);
                if (!discovered[j,i] && platforms[j,i].GetComponent<TipToePlatform>().active()) {
                    TargetPosition = platforms[j,i].transform.position;
                    currentI = i;
                    currentJ = j;
                    TargetSphere.transform.position = platforms[j,i].transform.position;
                    Agent.SetDestination(TargetSphere.transform.position);
                    return;
                } 
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TipToePlatform plat = hit.gameObject.GetComponent<TipToePlatform>();
        if (plat)
        {
            if (plat.name == platforms[currentJ, currentI].name) {
                discovered[currentJ, currentI] = true;
                if (plat.isPath) {
                    currentI = plat.row + 1;
                    currentJ = plat.col;
                }
            }
            plat.CharacterTouches();
            if (plat.isPath) {
                plat.isTaggedPath = true;
            }
        }

        if (hit.gameObject.tag == "ParcourObject") {
            InRagdoll = true;
        }
    }
}
