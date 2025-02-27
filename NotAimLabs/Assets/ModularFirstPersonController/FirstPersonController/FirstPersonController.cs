// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public float _mouseSensitivity = 20;

    public float mouseSensitivity
    {
        get => _mouseSensitivity;
        set
        {
            Debug.Log($"FPC: Changing sensitivity from {_mouseSensitivity} to {value}");
            _mouseSensitivity = value;
            OnSensitivityChanged?.Invoke(value);
            Debug.Log($"FPC: Sensitivity set to: {_mouseSensitivity}");
        }
    }
    public event System.Action<float> OnSensitivityChanged;

    private Rigidbody rb;
    private Vector3 groundNormal = Vector3.up;
    private bool componentsInitialized = false;

    #region Camera Movement Variables
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;

    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;
    #endregion

    #region Camera Zoom Variables
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    private bool isZoomed = false;
    #endregion

    #region Movement Variables
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    private bool isWalking = false;

    #region Sprint Variables
    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;
    #endregion

    #region Jump Variables
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    private bool isGrounded = false;
    #endregion

    #region Crouch Variables
    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    private bool isCrouched = false;
    private Vector3 originalScale;
    #endregion
    #endregion

    #region Head Bob Variables
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    private Vector3 jointOriginalPos;
    private float bobTimer = 0;
    #endregion

    private void InitializeComponents()
    {
        if (!componentsInitialized)
        {
            // Get or add required components
            rb = GetComponent<Rigidbody>();
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();
            crosshairObject = GetComponentInChildren<Image>();

            if (HasRequiredComponents())
            {
                // Initialize camera
                playerCamera.fieldOfView = fov;
                Vector3 rotation = transform.rotation.eulerAngles;
                yaw = rotation.y;
                pitch = playerCamera.transform.localRotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;

                // Initialize transforms
                originalScale = transform.localScale;
                if (joint != null)
                    jointOriginalPos = joint.localPosition;

                // Configure rigidbody
                rb.freezeRotation = true;
                rb.useGravity = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Initialize sprint
                if (!unlimitedSprint)
                {
                    sprintRemaining = sprintDuration;
                    sprintCooldownReset = sprintCooldown;
                }

                componentsInitialized = true;
            }
        }
    }

    private bool HasRequiredComponents()
    {
        if (rb == null || playerCamera == null)
        {
            Debug.LogError("Missing required components. Please ensure Rigidbody and Camera are assigned.");
            return false;
        }
        return true;
    }

    private void Awake()
    {
        Debug.Log("Awake called");
        _mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3.0f);
        Debug.Log($"Loaded sensitivity from PlayerPrefs: {_mouseSensitivity}");
        mouseSensitivity = _mouseSensitivity;
        InitializeComponents();
    }

    private void Start()
    {
        Debug.Log("Start called");
        if (!componentsInitialized)
        {
            Debug.LogWarning("Components not initialized in start");
            return;
        }

        SetupCursor();
        SetupCrosshair();
        SetupSprintBar();

        Debug.Log($"First Person Controller: Initial Sens Value {mouseSensitivity}");
    }

    private void SetupCursor()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetupCrosshair()
    {
        if (crosshairObject != null)
        {
            if (crosshair)
            {
                crosshairObject.sprite = crosshairImage;
                crosshairObject.color = crosshairColor;
            }
            else
            {
                crosshairObject.gameObject.SetActive(false);
            }
        }
    }

    private void SetupSprintBar()
    {
        if (!useSprintBar) return;

        sprintBarCG = GetComponentInChildren<CanvasGroup>();
        if (sprintBarBG != null && sprintBar != null)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector2(sprintBarWidth, sprintBarHeight);
            sprintBar.rectTransform.sizeDelta = new Vector2(sprintBarWidth - 2, sprintBarHeight - 2);

            if (hideBarWhenFull)
                sprintBarCG.alpha = 0;
        }
        else
        {
            Debug.LogWarning("Sprint bar UI elements not assigned!");
        }
    }

    private void Update()
    {
        if (!componentsInitialized || !HasRequiredComponents()) return;

        HandleCamera();
        HandleZoom();
        HandleSprint();
        HandleJump();
        HandleCrouch();

        CheckGround();

        if (enableHeadBob && joint != null)
            HeadBob();
    }

    private void HandleCamera()
    {
        if (cameraCanMove)
        {
            if (_mouseSensitivity != mouseSensitivity)
            {
                Debug.Log($"Mismatch detected! Field:{_mouseSensitivity}, Property:{mouseSensitivity}");
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Debug logging to see values
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                Debug.Log($"FPC: Using sensitivity value: {mouseSensitivity} for camera movement");
            }
            // Account for camera inversion
            mouseY = invertCamera ? mouseY : -mouseY;

            // Update yaw and pitch
            yaw += mouseX;
            pitch += mouseY;

            // Clamp pitch to prevent over-rotation
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            // Apply rotations using Quaternions
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    public void OnValidate()
    {
        Debug.Log($"Sensitivity changed to: {mouseSensitivity}");
    }

    private void HandleZoom()
    {
        if (!enableZoom || isSprinting) return;

        if (!holdToZoom)
        {
            if (Input.GetKeyDown(zoomKey))
                isZoomed = !isZoomed;
        }
        else
        {
            isZoomed = Input.GetKey(zoomKey);
        }

        float targetFOV = isZoomed ? zoomFOV : fov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, zoomStepTime * Time.deltaTime);
    }

    private void HandleSprint()
    {
        if (!enableSprint) return;

        if (isSprinting)
        {
            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                }
            }

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
        }
        else
        {
            sprintRemaining = Mathf.Min(sprintRemaining + Time.deltaTime, sprintDuration);
        }

        if (isSprintCooldown)
        {
            sprintCooldown -= Time.deltaTime;
            if (sprintCooldown <= 0)
            {
                isSprintCooldown = false;
                sprintCooldown = sprintCooldownReset;
            }
        }

        if (useSprintBar && !unlimitedSprint)
        {
            float sprintRemainingPercent = sprintRemaining / sprintDuration;
            sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = isSprinting ? 1 : Mathf.MoveTowards(sprintBarCG.alpha, 0, Time.deltaTime * 2f);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!componentsInitialized) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!playerCanMove) return;

        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Update walking state
        isWalking = (targetVelocity.x != 0 || targetVelocity.z != 0) && isGrounded;

        // Calculate target speed
        float currentSpeed = isSprinting && CanSprint() ? sprintSpeed : walkSpeed;
        if (isCrouched) currentSpeed *= speedReduction;

        // Transform direction relative to camera
        targetVelocity = transform.TransformDirection(targetVelocity).normalized * currentSpeed;

        // Calculate velocity change
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - new Vector3(velocity.x, 0, velocity.z));

        // Apply velocity change limit
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);
        velocityChange.y = 0;

        // Apply force
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private bool CanSprint()
    {
        return enableSprint &&
               Input.GetKey(sprintKey) &&
               !isSprintCooldown &&
               (!useSprintBar || unlimitedSprint || sprintRemaining > 0f);
    }

    private void HandleJump()
    {
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;

            if (isCrouched && !holdToCrouch)
                Crouch();
        }
    }

    private void HandleCrouch()
    {
        if (!enableCrouch) return;

        if (holdToCrouch)
        {
            if (Input.GetKeyDown(crouchKey) && !isCrouched)
                Crouch();
            else if (Input.GetKeyUp(crouchKey) && isCrouched)
                Crouch();
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
                Crouch();
        }
    }

    private void Crouch()
    {
        isCrouched = !isCrouched;
        transform.localScale = new Vector3(
            originalScale.x,
            isCrouched ? crouchHeight : originalScale.y,
            originalScale.z
        );

        walkSpeed = isCrouched ?
            walkSpeed * speedReduction :
            walkSpeed / speedReduction;
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            0.85f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            float bobSpeedMultiplier = isSprinting ? 1 + sprintSpeed / walkSpeed :
                                     isCrouched ? speedReduction :
                                     1;

            bobTimer += Time.deltaTime * bobSpeed * bobSpeedMultiplier;

            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(bobTimer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(bobTimer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(bobTimer) * bobAmount.z
            );
        }
        else
        {
            bobTimer = 0;
            joint.localPosition = Vector3.Lerp(
                joint.localPosition,
                jointOriginalPos,
                Time.deltaTime * bobSpeed
            );
        }
    }
}

