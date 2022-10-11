using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectCamera : MonoBehaviour
{
    public Transform Target;
    public float CameraSpeed = 1;


    private float maxCameraAngleY = 30;

    private float prevMouseX;
    private float prevMouseY;
    private float cameraMovementX;
    private float cameraMovementY;


    private float rotX;
    private float rotY;

// comment from sheckla-laptop
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

        // Save Rotation
        rotX += cameraMovementX * Time.deltaTime * CameraSpeed;
        rotY += cameraMovementY * Time.deltaTime * CameraSpeed;

        rotY = Mathf.Clamp(rotY, 10, 40);

        transform.localEulerAngles = new Vector3(rotY, rotX, 0);
        offset.z += Input.mouseScrollDelta.y;
        /* offset.z = -Mathf.Clamp(offset.z, 2, 20); */
        offset.z = Mathf.Clamp(offset.z, -30, -5);
        // Move camera to target entity
        transform.position = Target.position + (transform.forward * offset.z + transform.up * offset.y);

        //transform.Rotate(cameraMovementY * Time.deltaTime * CameraSpeed, cameraMovementX * Time.deltaTime * CameraSpeed, 0);
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
            prevMouseY = e.mousePosition.y;
            return;
        }

        // After MouseButtonDown - Mouse is now dragged
        if (e.type == EventType.MouseDrag)
        {
            cameraMovementX = e.mousePosition.x - prevMouseX;
            prevMouseX = e.mousePosition.x;

            cameraMovementY = e.mousePosition.y - prevMouseY;
            prevMouseY = e.mousePosition.y;
            return;
        }

        // If neither MouseButtonDown nor MouseDrag - cameraMovement is null
        cameraMovementX = 0;
        cameraMovementY = 0;

    }
}
