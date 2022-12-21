using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float PlayerHeight = 2f;
    [SerializeField] public float MoveSpeed = 6f;
    [SerializeField] public float AirMultiplier = 0.4f;
    [SerializeField] public float MovementMultiplier = 10f;
    [SerializeField] public Transform Orientation;
    [SerializeField] public AudioSource JumpAudio;

    [Header("Sprinting")]
    [SerializeField] public float WalkSpeed = 4f;
    [SerializeField] public float SprintSpeed = 6f;
    [SerializeField] public float Acceleration = 50f;

    [Header("Jumping")]
    [SerializeField] public float JumpForce = 14f;

    [Header("Keybinds")]
    [SerializeField] public KeyCode JumpKey = KeyCode.Space;
    [SerializeField] public KeyCode SprintKey = KeyCode.LeftShift;

    [Header("Drag")]
    [SerializeField] public float GroundDrag = 6f;
    [SerializeField] public float AirDrag = 2f;

    [Header("Ground Detection")]
    [SerializeField] public Transform GroundCheck;
    [SerializeField] public float GroundDistance = 0.2f;
    [SerializeField] public LayerMask GroundMask;

    [Header("Camera")]
    [SerializeField] public float SensitivityX = 150f;
    [SerializeField] public float SensitivityY = 150f;
    [SerializeField] public float SensitivityMultiplier = 0.01f;
    [SerializeField] public Transform Camera;

    [Header("Collectables")]
    [SerializeField] public TextMeshProUGUI PointsText;
    [SerializeField] public int PointsObjective = 15;
    [SerializeField] public AudioSource WinAudio;

    private float _horizontalMovement;
    private float _verticalMovement;
    private bool _isGrounded;
    private Vector3 _moveDirection;
    private Vector3 _slopeMoveDirection;
    private Rigidbody _rb;
    private RaycastHit _slopeHit;

    private Camera _actualCam;
    private float _mouseX;
    private float _mouseY;
    private float _xRotation;
    private float _yRotation;

    private int _pointsCounter;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _actualCam = Camera.GetComponentInChildren<Camera>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PointsText.text = $"Points: <color=red>{_pointsCounter} / {PointsObjective}</color>";
    }

    private void Update()
    {
        // Check if player is grounded
        _isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        _horizontalMovement = Input.GetAxisRaw("Horizontal");
        _verticalMovement = Input.GetAxisRaw("Vertical");

        _moveDirection = Orientation.forward * _verticalMovement + Orientation.right * _horizontalMovement;

        // Rotate the player towards movement direction
        if (!_moveDirection.Equals(Vector3.zero))
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDirection), 10f * Time.deltaTime);
        //else if (Input.GetButtonDown("Fire1") && _moveDirection.Equals(Vector3.zero))
        //    transform.rotation = Quaternion.LookRotation(Camera.transform.forward);

        _mouseX = Input.GetAxisRaw("Mouse X");
        _mouseY = Input.GetAxisRaw("Mouse Y");

        _yRotation += _mouseX * SensitivityX * SensitivityMultiplier;
        _xRotation -= _mouseY * SensitivityY * SensitivityMultiplier;

        // Limit our camera rotation so we don't go upside down
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        Camera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        Orientation.transform.rotation = Quaternion.Euler(0, _yRotation, 0);

        _rb.drag = _isGrounded ? GroundDrag : AirDrag;

        // Check if sprinting
        if (Input.GetKey(SprintKey) && _isGrounded)
        {
            // Check if player is actually moving
            if (!(_horizontalMovement.Equals(0) && _verticalMovement.Equals(0)))
            {
                MoveSpeed = Mathf.Lerp(MoveSpeed, SprintSpeed, Acceleration * Time.deltaTime);
                // Adds that feeling of going fast
                _actualCam.fieldOfView = Mathf.Lerp(_actualCam.fieldOfView, 90f, 10f * Time.deltaTime);
            }
        }
        else
        {
            MoveSpeed = Mathf.Lerp(MoveSpeed, WalkSpeed, Acceleration * Time.deltaTime);
            _actualCam.fieldOfView = Mathf.Lerp(_actualCam.fieldOfView, 80f, 10f * Time.deltaTime);
        }

        // Jumping
        if (Input.GetKeyDown(JumpKey) && _isGrounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
            JumpAudio.Play();
        }

        // Set slope move direction so we can use if we are on a slope
        _slopeMoveDirection = Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal);
        
        // Restart the game if the player fall
        if (_rb.velocity.y < -14f)
            StartCoroutine(FallGroundRestart());
    }

    private void FixedUpdate()
    {
        // If grounded and not on slope
        if (_isGrounded && !OnSlope())
        {
            _rb.AddForce(_moveDirection.normalized * MoveSpeed * MovementMultiplier, ForceMode.Acceleration);
            _rb.useGravity = true;
        }
        // If grounded and on slope
        else if (_isGrounded && OnSlope())
        {
            _rb.AddForce(_slopeMoveDirection.normalized * MoveSpeed * MovementMultiplier, ForceMode.Acceleration);
            _rb.useGravity = false;
        }
        // If not grounded
        else if (!_isGrounded)
        {
            _rb.AddForce(_moveDirection.normalized * MoveSpeed * MovementMultiplier * AirMultiplier, ForceMode.Acceleration);
            _rb.useGravity = true;
        }
    }

    public IEnumerator FallGroundRestart()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return new WaitForSeconds(1);
    }

    public IEnumerator Restart(int seconds)
    {
        WinAudio.Play();
        for (int i = 0; i < 5; i++)
        {
            PointsText.text = $"<color=green>You win!</color> Restarting in {seconds - i} seconds...";
            yield return new WaitForSeconds(1);
        }
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int value)
    {
        _pointsCounter += value;
        PointsText.text = $"Points: <color=red>{_pointsCounter} / {PointsObjective}</color>";
        if (_pointsCounter >= PointsObjective)
            StartCoroutine(Restart(5));
    }

    private bool OnSlope()
    {
        // Check if we are on a slope by sending a raycast down
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, PlayerHeight / 2 + 0.5f))
            if (_slopeHit.normal != Vector3.up)
                return true;

        return false;
    }
}
