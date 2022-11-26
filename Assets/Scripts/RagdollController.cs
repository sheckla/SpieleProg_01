using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    // Must Contain Ragdoll Elements like Rigidbodies, Colliders & Joints
    public GameObject Armature;
    public GameObject Torso;
    public GameObject FeetL;
    public GameObject FeetR;

    // Ragdoll
    private Rigidbody[] Rigidbodies;
    private Collider[] Colliders;
    private CharacterJoint[] Joints;

    private Vector3 initialRootPos;
    private Vector3 initialRootRot;

    private bool Active = false;

    public bool StaticActive;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbodies = Armature.GetComponentsInChildren<Rigidbody>();
        Colliders = Armature.GetComponentsInChildren<Collider>();
        Joints = Armature.GetComponentsInChildren<CharacterJoint>();

        initialRootPos = Armature.transform.localPosition;
        initialRootRot = Armature.transform.localEulerAngles;

        print(initialRootPos);
        print(initialRootRot);
    }

    void Update() {
        if (StaticActive) enable();    
    }

    public void applyArmatureRoot(Transform trans)
    {
        Armature.transform.localPosition = initialRootPos;
        Armature.transform.localRotation = Quaternion.Euler(initialRootRot);
    }


    public BoxCollider torsoBoxCollider()
    {
        return Torso.GetComponent<BoxCollider>();
    }

    public CapsuleCollider[] feetCollider()
    {
        CapsuleCollider[] colliders = new CapsuleCollider[2];
        colliders[0] = FeetL.GetComponent<CapsuleCollider>();
        colliders[1] = FeetR.GetComponent<CapsuleCollider>();
        return colliders;
    }

    public float torsoVelocity() 
    {
        return Torso.GetComponent<Rigidbody>().velocity.magnitude;
    }


    public bool active() 
    {
        return Active;
    }

    public void disable()
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
        Active = false;
    }

    public void enable()
    {
        foreach (Collider col in Colliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rbody in Rigidbodies)
        {
            rbody.isKinematic = false;
            rbody.detectCollisions = true;
            rbody.useGravity = true;
        }

        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = true;
        }
        Active = true;
    }

    public void applyForce(Vector3 forceDir, float impulse) 
    {
        foreach (Rigidbody rbody in Rigidbodies)
        {
            rbody.AddExplosionForce(impulse, Armature.transform.position + forceDir, 10f, 0f, ForceMode.Impulse);
        }
    }
}
