using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MovePlayer();
        LookAround();
    }

    void MovePlayer()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(PlayerKeySettings.ForwardKey))
        {
            vertical += 1f;
        }

        if (Input.GetKey(PlayerKeySettings.BackKey))
        {
            vertical -= 1f;
        }

        if (Input.GetKey(PlayerKeySettings.RightKey))
        {
            horizontal += 1f;
        }

        if (Input.GetKey(PlayerKeySettings.LeftKey))
        {
            horizontal -= 1f;
        }

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}