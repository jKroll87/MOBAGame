using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // var. Input System
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;

    // var. Movement
    private CharacterController controller;

    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // var. Bullet
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private Transform barrelTransform;
    [SerializeField] private float bulletHitMissDistance = 25f;
    
    // var. Camera
    private Transform cameraTransform;

    // var. Animation
    private Animator animator;
    int moveXAnimationParameterId;
    int moveZAnimationParameterId;
    int jumpAnimation;
    int recoilAnimation;

    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;

    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private float animationPlayTime = 0.15f;

    // var. Rig
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float aimDistance = 1f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = gameObject.GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];

        cameraTransform = Camera.main.transform;

        // Animation
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
        jumpAnimation = Animator.StringToHash("Pistol Jump");
        recoilAnimation = Animator.StringToHash("PistolShoot Recoil");
    }

    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();
    }

    private void ShootGun()
    {
        RaycastHit hit;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, barrelTransform.position, Quaternion.identity, bulletParent);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            bulletController.target = hit.point;
            bulletController.hit = true;
        }
        else {
            bulletController.target = cameraTransform.position + cameraTransform.forward * bulletHitMissDistance;
            bulletController.hit = false;
        }
        animator.CrossFade(recoilAnimation, animationPlayTime);
    }

    void Update()
    {
        // Rig(Aim)
        aimTarget.position = cameraTransform.position + cameraTransform.forward * aimDistance + cameraTransform.right * 10f;

        //Gravity
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Movement();
        Jump();
        Rotation();

        //Gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void Movement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        /* Vector2.SmoothDamp();
        param1: current position (cf. Position value would change everytime Player moves. Also, from ex.(12, 7), input.x = (12, 0) and input.y = (0, 7)) 
        param2: target position
        param3: current velocity-vector (Keeps changing based on x and y velocity-vector (Maximum x and y velocity-vector would be x:(1, 0) and y:(0, 1)))
        */
        Vector3 move = new Vector3(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y); //-> Changed "input.x/.y" to "currentAnimationBlendVector.x/.y"
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Animation
        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x); //-> Changed "input.x/.y" to "currentAnimationBlendVector.x/.y"
        animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);
    }
    private void Jump()
    {
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);

            // Animation
            animator.CrossFade(jumpAnimation, animationPlayTime); //-> Smoothly fades to next animation
            /* Alternatives
            animator.SetTrigger(jumpAnimation); (With transitions)  (-> Cons. Gets too complicated with transitions)
            animator.Play(jumpAnimation);                           (-> Cons. Cuts off to next animation without fading)
            */
        }
    }
    private void Rotation()
    {
        //Camera가 바라보는 방향에 따라 Player가 바라보는 방향이 같이 바뀌도록 하는 함수이다.

        Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);    //Camera의 Euler y값을 사용하기 위해, Quaternion y값으로 바꿔준다.
                                                                                             //Euler y값은 rotation y값으로 위/아래가 아닌 좌/우 회전을 나타낸다.
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);   //Camera의 Euler y값과 Player의 rotation값을 동일하게 한다.
    }
}