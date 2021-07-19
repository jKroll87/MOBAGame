using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class SwitchVCam : MonoBehaviour
{
    //*** To change transition-speed between Virtual Cameras,
    //head over to - Main Camera -> CinemachineBrain -> Default Blend -> change number

    // var. Input System
    [SerializeField] private PlayerInput playerInput;

    private InputAction aimAction;

    // var. Camera
    private CinemachineVirtualCamera virtualCamera;     //Cinemachine의 Virtual Camera를 사용하기 위한 변수

    [SerializeField] private int priorityIncrementAmount = 10;      //Cinemachine의 각 Camera는 고유의 priority number를 가지고 있다.
                                                                    //Cinemachine의 Camera 개수가 2이상일 경우, priority number가 높은 Camera일 수록 그 Camera를 사용한다.

    [SerializeField] private Canvas thirdPersonCanvas;  //과녁 image를 표시하기 위한 canvas
    [SerializeField] private Canvas aimCanvas;

    //실행 순서: Awake() -> OnEnable() -> Start() -> Update() (OnDisable()은 OnEnable()이 종료되었을 경우)

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        aimAction = playerInput.actions["Aim"];
    }

    private void OnEnable()
    {
        //aimAction에 StartAim(), CancelAim() 연결
        aimAction.performed += _ => StartAim();     //aimAction을 했을 경우 (즉, 마우스의 우측버튼을 눌렀을 경우 (<- Scripts폴더의 Player Contrls에서 확인할 수 있다)) StartAim()을 ()실행하라.
        aimAction.canceled += _ => CancelAim();     //마우스 우측버튼을 놓았을 경우 CancelAim()을 실행하라. 
                                                    //"_"는 context로 WASD와 같이 여러키를 사용할 때, 각 키를 구분하기 위해 사용하는 하는 부분이다. 지금은 한 키를 사용하므로 null을 의미하는 _를 사용한다.
    }

    private void OnDisable()
    {
        //aimAction에 StartAim(), CancelAim() 연결 취소 
        aimAction.performed -= _ => StartAim();
        aimAction.canceled -= _ => CancelAim();
    }

    private void StartAim()
    {
        //현재, 각 Cinemachine Virtual Camera의 priorithy number는 다음과 같다.
        //"과녁X 카메라": 10 (마우스 우측키를 누르지 않았을 경우)
        //"과녀O 카메라":  9 (마우스 우칙키를 눌렀을 경우)

        virtualCamera.Priority += priorityIncrementAmount; //-> 과녁X 카메라: 10 과녁O 카메라: 19
        aimCanvas.enabled = true;           //ZoomIn Reticle 이미지 표시
        thirdPersonCanvas.enabled = false;  //ZoomOut Reticle 이미지 표시 X

        //cf. Hierarchy에서   3rdPersonCinemachine -> 3rdPersonCanvas -> ZoomOut Reticle
        //                   AimCinemachine -> AimCanvas -> ZoomIn Reticle
    }

    private void CancelAim()
    {
        virtualCamera.Priority -= priorityIncrementAmount;
        aimCanvas.enabled = false;
        thirdPersonCanvas.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
