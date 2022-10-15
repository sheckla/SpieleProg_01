using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectCamera : MonoBehaviour
{
    public Transform Target;

    [SerializeField]
    public float CameraSpeed;
    public float MaxCameraAngleY;
    public float MaxCameraDistance;

    private float RotX;
    private float RotY;
    private bool MouseDown;

    [SerializeField] // prevent from being set to 0 after play-press
    private Vector3 offset;

    // called after Update
    void LateUpdate()
    {
        CameraOffset(); // Cam Distance
        if (MouseDown) CameraRotation();

        // Apply transform
        transform.position = Target.position + (transform.forward * offset.z + transform.up * offset.y + transform.right * offset.x);
    }

    void OnGUI()
    {
        Event e = Event.current; // get MouseEvent, potential MouseButtonClick

        // Initial MouseButtonDown
        if (e.type == EventType.MouseDown)
        {
            MouseDown = true;
            return;
        }

        // After MouseButtonDown - Mouse is now dragged
        if (e.type == EventType.MouseDrag)
        {
            MouseDown = true;
            return;
        }

        // If neither MouseButtonDown nor MouseDrag - cameraMovement is null
        MouseDown = false;
    }

    void CameraRotation()
    {
        float fixedFactor = 100;
        RotX += Input.GetAxis("Mouse X") * Time.deltaTime * CameraSpeed * fixedFactor;
        RotY += -Input.GetAxis("Mouse Y") * Time.deltaTime * CameraSpeed * fixedFactor;
        RotY = Mathf.Clamp(RotY, 10, MaxCameraAngleY);
        transform.localEulerAngles = new Vector3(RotY, RotX, 0);
    }

    void CameraOffset()
    {
        offset.z += Input.mouseScrollDelta.y;
        offset.z = Mathf.Clamp(offset.z, -MaxCameraDistance, -5);
    }
}
