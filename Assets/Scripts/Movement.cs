using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    PlayerControls controls;

    [Header("Variables")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float mouseLookSensitivity = 0.1f;
    [SerializeField] private float gamepadLookSensitivity = 150f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float turnSpeed = 720f;          // degrees per second when moving or aiming
    [SerializeField] private float turnThreshold = 70f;       // degrees before a turn-in-place plays
    [SerializeField] private float turnInPlaceSpeed = 180f;   // degrees per second during a turn-in-place
    [SerializeField] private float turnFinishAngle = 5f;      // turn ends when the body is this close to the camera

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform cameraTarget;

    private float yaw, pitch;
    private bool isTurning;

    public bool IsAiming { get; private set; }
    public bool IsCrouching { get; private set; }

    void Awake()     => controls = new PlayerControls();
    void OnEnable()  => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Start() => Cursor.lockState = CursorLockMode.Locked;

    void Update()
    {
        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        Vector2 look = controls.Player.Look.ReadValue<Vector2>();

        bool usingStick = controls.Player.Look.activeControl?.device is Gamepad;
        float sensitivity = usingStick ? gamepadLookSensitivity * Time.deltaTime : mouseLookSensitivity;

        yaw += look.x * sensitivity;
        pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);
        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0);

        IsAiming = controls.Player.Aim.IsPressed();
        IsCrouching = controls.Player.Crouch.IsPressed();
        int sprint = controls.Player.Sprint.IsPressed() ? 2 : 1;

        Quaternion yawRotation = Quaternion.Euler(0, yaw, 0);
        bool isMoving = move.sqrMagnitude > 0.01f;

        // Signed angle between where the body faces and where the camera looks (-180 to 180)
        float angle = Mathf.DeltaAngle(transform.eulerAngles.y, yaw);

        if (isMoving || IsAiming)
        {
            // Normal turning takes over, so cancel any turn-in-place
            if (isTurning)
            {
                isTurning = false;
                playerAnimator.ResetTrigger("TurnLeft");    // clear any trigger that never got used
                playerAnimator.ResetTrigger("TurnRight");
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, yawRotation, turnSpeed * Time.deltaTime);
        }
        else if (isTurning)
        {
            // Rotate the body to catch up while the turn animation plays
            transform.rotation = Quaternion.RotateTowards(transform.rotation, yawRotation, turnInPlaceSpeed * Time.deltaTime);

            if (Mathf.Abs(angle) < turnFinishAngle)
                isTurning = false;
        }
        else if (Mathf.Abs(angle) > turnThreshold)
        {
            // Standing still and the camera has swung past the threshold
            isTurning = true;
            playerAnimator.SetTrigger(angle > 0 ? "TurnRight" : "TurnLeft");
        }

        // Move relative to the camera's direction, not the body's, so it responds immediately
        Vector3 dir = yawRotation * new Vector3(move.x, 0f, move.y);
        characterController.Move(dir * moveSpeed * sprint * Time.deltaTime);

        playerAnimator.SetBool("Crouching", IsCrouching);
        playerAnimator.SetBool("Aiming", IsAiming);
        playerAnimator.SetFloat("X", move.x * sprint, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("Y", move.y * sprint, 0.1f, Time.deltaTime);
    }
}