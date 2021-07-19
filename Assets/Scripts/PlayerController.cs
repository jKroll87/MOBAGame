using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // var. Input System
    private PlayerInput playerInput;    //Player(GameObject)의 Player Input을 사용하기 위한 변수 

    // Scripts폴더 -> PlayerControls의 Move/Look/Jump를 사용하기 위한 변수 
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

    private CharacterController controller; //Plaeyr(GameObject)의 Character Controller를 사옹하기 위한 변수

    // Camera
    private Transform cameraTransform;      //Camera의 위치정보에 접근하기 위한 변수

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;       //게임을 시작할 때 마우스 프로인터를 없엔다.

        controller = gameObject.GetComponent<CharacterController>();    //Player(GameObject)의 Character Controller 사용
        playerInput = gameObject.GetComponent<PlayerInput>();   //Player(GameObject)의 Player Input 사용 

        //PlayerControls의 Move/Look/Jump를 사용(변수에 연결)
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];

        cameraTransform = Camera.main.transform;    //위치정보에 접근 할 Camera를 Main Camera로 설정 
    }

    void Update()
    {
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
        Vector2 input = moveAction.ReadValue<Vector2>();        //키보드 WASD는 앞/뒤/좌/우 2차원 이므로 Vector3가 아닌 Vector2 사용. "Vector3 move"는 Vector2의 값을 이용하여 Vector3 3차원 상에서의 Player를 앞/뒤/좌/우로 움직이려는 것이다.
        Vector3 move = new Vector3(input.x, 0, input.y);        //Player가 이동할 벡터 (이때, x와 z값을 각각 input.x, input.y로 설정하여 Player Controls의 Move 즉, 키보드 WASD로 벡테 방향을 바꿀 수 있도록 한다.)
                                                                //z값에 input.y를 설정한 이유는 3D에서 y값은 "위/아래", z값은 "앞/뒤"를 나타내기 때문이다.
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;     //Player가 Camera를 기준으로 "좌/우,앞/뒤"를 이동할 수 있도록 한다. 즉, Camera로 우측을 바라보면 기존 Player의 우측방향 또한 Camera를 따라 바뀐다.
                                                                                                            //원래 Player의 방향은 World를 기준으로 한다. 이를, Camera 기준으로 바꾸는 것이다.
        move.y = 0f;    //x와 z값이 변하면 y값도 변할 수 있으므로 (즉, Player가 위/아래로 이동할 수 있으므로) 미리 0으로 고정시킨다.
        controller.Move(move * Time.deltaTime * playerSpeed);   //move 벡터를 사용하여 Player를 이동시킨다.
    }
    private void Jump()
    {
        //Jump
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
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