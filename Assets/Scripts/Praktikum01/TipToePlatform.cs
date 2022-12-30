using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEditor.AI;

public class TipToePlatform : MonoBehaviour
{
    enum State
    {
        Default,
        Touched,
        Dead,
        Desolving,
    }

    State state = State.Default;
    MeshRenderer meshRend;
    BoxCollider bCollider;
    public bool isPath;
    public Material defaultMaterial;
    GameObject cube;
    private NavMeshSurface Surface;
    private bool NavUpdated = false;
    public bool isTaggedPath = false;

    //Variables Touched State
    public Material touchedMaterial;

    public Material desolveMaterial;

    float touchedTimer = 0.0f;
    public float maxTouchedTime = 5.0f;

    public float desolveTime = 1.0f;
    public int row = 0;
    public int col = 0;

    //Variables Dead State
    float deadTimer = 0.0f;
    public float maxDeadTime = 3.0f;

    void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        cube = GetComponent<GameObject>();
        meshRend.material = defaultMaterial;
        bCollider = GetComponent<BoxCollider>();
        Surface = GetComponent<NavMeshSurface>();
    }

    void Update()
    {
        /* Debug.Log(desolveTime);
        Debug.Log(state); */

        if(state==State.Desolving){
            if(desolveTime <= 0.0){
                deadTimer = maxDeadTime;
                meshRend.enabled = false;
                bCollider.enabled = false;
                state = State.Dead;
                desolveTime = 1.0f;
            }else{
                meshRend.material = desolveMaterial;
                desolveMaterial.SetFloat("_Threshold",desolveTime);

                desolveTime-=Time.deltaTime*2;
            }
        }



        if (state == State.Dead)
        {
            //return; // Ignore Dead-restore mechanics
            //Count down timer until respawn of platform
            deadTimer -= Time.deltaTime;
            Surface.enabled = false;
            if (!NavUpdated) {
                //UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                NavUpdated = true;
            }
            if (deadTimer <= 0.0f)
            {
                return;
                ChangeState(State.Default);
                deadTimer = 0.0f;
                meshRend.enabled = true;
                bCollider.enabled = true;
                //Surface.enabled = true;
                //UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                NavUpdated = false;
                meshRend.material = defaultMaterial;
            }
        }
        if (state == State.Touched)
        {
            //Count down timer until reversion to unlit platform
            touchedTimer -= Time.deltaTime;
            if (touchedTimer <= 0.0f)
            {
                ChangeState(State.Default);
                touchedTimer = 0.0f;
                meshRend.material = defaultMaterial;
            }
        }

        if (isTaggedPath) meshRend.material = touchedMaterial;
    }

    public bool active() {
        if (!Surface.enabled) return false;
        return true;
    }

   void OnCollisionEnter(Collision col){
        CharacterTouches();
   }

    private void ChangeState(State s)
    {
        state = s;
    }

    public void showPath()
    {
        meshRend.material = touchedMaterial;
    }

    public void CharacterTouches()
    {
        if (!isPath)
        {
            ChangeState(State.Desolving);
            //HERE
            /*
            if(desolveTime <= 0.0){
                deadTimer = maxDeadTime;
                meshRend.enabled = false;
                bCollider.enabled = false;
                desolveTime = 5.0f;
            }else{
                desolveTime-=Time.deltaTime;
            }
            */
            
        }
        else
        {
            if (state == State.Touched)
            {
                touchedTimer = maxTouchedTime;
            }
            else
            {
                ChangeState(State.Touched);
                touchedTimer = maxTouchedTime;
                meshRend.material = touchedMaterial;
            }
        }
    }

    public bool Dead()
    {
        bool touched = (this.state == State.Dead) ? true : false;
        return touched;
    }

    public void SetPath(bool b)
    {
        isPath = b;
    }
}
