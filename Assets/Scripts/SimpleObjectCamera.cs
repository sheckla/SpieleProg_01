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

    private bool SmoothCamera = false;
    private float RotY;
    private float RotX;
    private bool MouseDown;
    public float ActiveScrollDistance = 0;

    [SerializeField] // prevent from being set to 0 after play-press
    private Vector3 offset;

    public void smooth(bool b)
    {
        SmoothCamera = b;
    }

    // called after Update
    void LateUpdate()
    {
        CameraOffset(); // Cam Distance
        if (MouseDown) CameraRotation();

            Vector3 newCamPos = Target.position + (transform.forward * offset.z + transform.up * offset.y + transform.right * offset.x);
        // Apply transform
        if (!SmoothCamera)
        {
            transform.position = newCamPos;
        } else 
        {
            transform.position = Vector3.Lerp(transform.position, newCamPos, 0.11f);
        }
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
        float mouseMovementX = Input.GetAxis("Mouse X") * Time.deltaTime * CameraSpeed * fixedFactor;
        float mouseMovementY = -Input.GetAxis("Mouse Y") * Time.deltaTime * CameraSpeed * fixedFactor;

        // Raycast to ground
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity);
        // Check for ground clipping when moving camera
        if ((hit.distance <= 2.0f) && mouseMovementY <= 0f) 
        {
            mouseMovementY = 0;
        }
        RotY += mouseMovementX;
        RotX += mouseMovementY;
        RotX = Mathf.Clamp(RotX, -90, 90);

        // Rotate smoothly
        if (!SmoothCamera)
        {
            transform.rotation = Quaternion.Euler(RotX, RotY, 0);
        }
        // Rotate statically
         else 
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(RotX, RotY, 0), 0.11f);
        }
    }

    void CameraOffset()
    {
        ActiveScrollDistance -= Input.mouseScrollDelta.y;
        ActiveScrollDistance = Mathf.Clamp(ActiveScrollDistance, 5, MaxCameraDistance);

        RaycastHit hit;
        float dist = 0;
        Vector3 targetToCam = transform.position - Target.transform.position;
        if (Physics.Raycast(Target.transform.position, targetToCam.normalized, out hit, ActiveScrollDistance, ~6)) 
        {
            dist = hit.distance - 3;
            //offset.z = -dist;
        } else {
        }
            offset.z = -ActiveScrollDistance;
    }

    // float i~[fromMin,fromMax] -> i~[toMin, toMax]
	static float remapBounds(float i, float fromMin, float fromMax, float toMin, float toMax)
	{
		return (i - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
	}
}
