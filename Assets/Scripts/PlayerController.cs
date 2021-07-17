using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // var. Input System
    private PlayerInput playerInput; //-> "PlayerInput" prechosen name?

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    // var. Movement
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private CharacterController controller;

    // Camera
    private Transform cameraTransform;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = gameObject.GetComponent<CharacterController>(); //-> Isn't this same as "CharacterController controller"?
        playerInput = gameObject.GetComponent<PlayerInput>();
        
        moveAction = playerInput.actions["Move"];
        //lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];

        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Gravity
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Movement
        Vector2 input = moveAction.ReadValue<Vector2>();
        //-> moveAction.ReadValue<Vector2>() : Value of "Move" Input System
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Jump
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        // Gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Rotation
        Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        //-> Euler(0f, cameraTransform.eulerAngles.y, 0f) : Euler "angle" of camera's y-roll.
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        /*-> Quaternion.Lerp(rotation1, rotation2, rotation-speed) : Change current (player's) rotation
        from rotation1 to rotation2 with speed of rotation-speed.
             .Lerp : Change rotation linearly
             .Slerp: Change rotation spherically
         
         */
    }
}