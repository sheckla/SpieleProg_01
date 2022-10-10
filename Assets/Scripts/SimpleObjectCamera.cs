using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectCamera : MonoBehaviour
{
    public Transform Target;
    public float CameraSpeed = 10;

    private float prevMouseX;
    private float cameraMovementX;

    [SerializeField] // prevent from being set to 0 after play-press
    private Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    // called after Update
    void LateUpdate()
    {
        // Move camera to target entity
        transform.position = Target.position + (transform.forward * offset.z + transform.up * offset.y);
        transform.Rotate(0, cameraMovementX * Time.deltaTime * CameraSpeed, 0);

        //transform.position = (Target.position) - transform.forward;
        //print(scaledForward);
    }

    void OnGUI()
    {
        Event e = Event.current; // get MouseEvent, potential MouseButtonClick

        // Initial MouseButtonDown
        if (e.type == EventType.MouseDown)
        {
            prevMouseX = e.mousePosition.x;
            return;
        }

        // After MouseButtonDown - Mouse is now dragged
        if (e.type == EventType.MouseDrag)
        {
            cameraMovementX = e.mousePosition.x - prevMouseX;
            prevMouseX = e.mousePosition.x;
            return;
        }

        // If neither MouseButtonDown nor MouseDrag - cameraMovement is null
        cameraMovementX = 0;
    }
}
