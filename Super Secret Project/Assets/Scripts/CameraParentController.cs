using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class CameraParentController : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 3.5f;
    [SerializeField] private bool lockCursor = true;
    [SerializeField] [Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f;

    private float cameraPitch = 0.0f;

    private Vector2 targetMouseDelta;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private void Start()
    {
        targetMouseDelta = new Vector2();
        currentMouseDelta = Vector2.zero;
        currentMouseDeltaVelocity = Vector2.zero;
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            cameraTransform.gameObject.SetActive(true);

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }


        UpdateMouseLook(); // The camera looking up and down according to the mouse
    }

    private void UpdateMouseLook()
    {
        targetMouseDelta.x = Input.GetAxis("Mouse X");
        targetMouseDelta.y = Input.GetAxis("Mouse Y");

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
        cameraTransform.localEulerAngles = Vector3.right * cameraPitch;

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }
}
