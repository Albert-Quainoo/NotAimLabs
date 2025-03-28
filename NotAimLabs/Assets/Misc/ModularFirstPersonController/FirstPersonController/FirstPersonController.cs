using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;

public class FirstPersonController : MonoBehaviour
{
    #region Camera Movement Variables

    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float maxLookAngle = 80f;
    public bool initialInputDelay = true;

    // Internal reference to mouse sensitivity (for editor)
    [SerializeField]
    public float _mouseSensitivity = 10f;


    // Public sensitivity property that actually gets used for movement
    public float mouseSensitivity
    {
        get => _mouseSensitivity;
        set
        {
            _mouseSensitivity = value;
            OnSensitivityChanged?.Invoke(value);

            // Save to PlayerPrefs
            PlayerPrefs.SetFloat("MouseSensitivity", value);
            PlayerPrefs.Save();

           


            Debug.Log($"FPC: Sensitivity set to: {_mouseSensitivity}");
        }
    }
    public event System.Action<float> OnSensitivityChanged;

    // Rotation helpers
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private const string xAxis = "Mouse X";
    private const string yAxis = "Mouse Y";

    private float defaultFOV;
    private float targetFOV;
    private float targetStepTime = 5f;

    #endregion

    #region UI & Crosshair

    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    private Image crosshairObject;

    #endregion

    #region Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    private bool isZoomed = false;

    #endregion

    #region Sprint Variables

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 10f;
    public float sprintDuration = 5f;
    public float sprintCooldown = 2f;
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

    // Jump checks
    private bool isGrounded = false;
    private Vector3 groundNormal = Vector3.up;

    #endregion

    #region Crouch Variables

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 0.75f;
    public float speedReduction = 0.5f;

    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion

    #region Headbob Variables

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(0.15f, 0.05f, 0f);

    private Vector3 jointOriginalPos;
    private float bobTimer = 0;

    #endregion

    #region Footstep Variables

    public bool enableFootsteps = true;
    public float baseStepSpeed = 0.5f;
    public float crouchStepMultiplier = 1.5f;
    public float sprintStepMultiplier = 0.6f;
    public AudioSource footstepAudioSource = default;
    public AudioClip[] woodClips = default;
    public AudioClip[] metalClips = default;
    public AudioClip[] grassClips = default;

    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouched ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    public float gravity = 30f;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 50f;
    private float slopeCheckDistance;
    private float slopeSlideSpeed;
    private RaycastHit slopeHit;
    private bool cancellingGrounded;

    private Rigidbody rb;
    private CharacterController charController;
    private Vector3 moveDirection;
    private bool isWalking = false;
    private bool componentsInitialized = false;

    #endregion

    #region Misc

    private CountdownTimer countdownTimer;
    private bool sensitivityLoaded = false;

    #endregion
    private void Awake()
    {
        LoadSensitivtyFromPlayerPrefs();

    }

    private void Start()
    {

        if (!componentsInitialized)
        {

            InitializeComponents();
            if (!componentsInitialized) return;
        }


        SetupCursor();
        SetupCrosshair();
        SetupSprintBar();

        cameraCanMove = true;

        countdownTimer = FindAnyObjectByType<CountdownTimer>();
    }

    private void InitializeComponents()
    {
        if (!componentsInitialized)
        {
            // Get or add required components
            rb = GetComponent<Rigidbody>();
            charController = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                    Debug.LogError("Camera component missing from FPS Controller!");
            }

            if (HasRequiredComponents())
            {
                // Initialize camera
                playerCamera.fieldOfView = fov;
                defaultFOV = fov;
                targetFOV = defaultFOV;

                Vector3 rotation = transform.rotation.eulerAngles;
                yaw = rotation.y;
                pitch = playerCamera.transform.localRotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;

                // Initialize transforms
                originalScale = transform.localScale;
                if (joint != null)
                    jointOriginalPos = joint.localPosition;
                else
                    Debug.LogWarning("Camera Joint object not found. Assign a joint object for headbob.");

                // Configure rigidbody if present
                if (rb != null)
                {
                    rb.freezeRotation = true;
                    rb.useGravity = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }

                // Initialize sprint
                sprintRemaining = sprintDuration;
                sprintCooldownReset = sprintCooldown;

                componentsInitialized = true;
            }
        }
    }

    private bool HasRequiredComponents()
    {
        if (playerCamera == null)
        {
            return false;
        }

        if (rb == null && charController == null)
        {
            return false;
        }

        return true;
    }

    
   


    private void LoadSensitivtyFromPlayerPrefs()
    {
        string prefKey = "MouseSensitivity";
        if (PlayerPrefs.HasKey(prefKey))
        {
            float savedSensitivity = PlayerPrefs.GetFloat(prefKey);
            _mouseSensitivity = savedSensitivity;
        }
        else
        {
            Debug.Log($"FPC Awake - No Saved Sensitivity found, using default {_mouseSensitivity}");
        }
    }

    private void SetupCursor()
    {
       if (lockCursor && (countdownTimer == null || !countdownTimer.isGameOver))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SetupCrosshair()
    {
        if (crosshair)
        {
            if (crosshairObject == null)
            {
                CreateCrosshair();
            }
            else
            {
                crosshairObject.sprite = crosshairImage;
                crosshairObject.color = crosshairColor;
            }
        }
        else if (crosshairObject != null)
        {
            crosshairObject.gameObject.SetActive(false);
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

        // Trigger functions
        if (playerCanMove)
        {
            HandleMouseLook();
            HandleMovementInput();

            if (enableZoom)
                HandleZoom();

            if (enableSprint)
                HandleSprint();

            if (enableJump)
                HandleJump();

            if (enableCrouch)
                HandleCrouch();

            if (enableHeadBob)
                HandleHeadbob();

            if (enableFootsteps)
                HandleFootsteps();

            if (charController != null)
                ApplyFinalMovements();
        }

        if (playerCanMove && cameraCanMove)
        {
            HandleMouseLook(true);
        }
    }

    private void FixedUpdate()
    {
        if (!componentsInitialized || !playerCanMove || rb == null) return;

        HandleRigidbodyMovement();
        CheckGround();
    }

    public void HandleMouseLook(bool force = false)
    {
       // Check if game is over
       if (countdownTimer != null && countdownTimer.isGameOver)
        {
            // Cursor should not be locked when game is over
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
   

        // Manage cursor lock/visibility
        if (Input.GetKeyDown(KeyCode.Escape) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Apply rotation if camera can move
        if (cameraCanMove)
        {
            float mouseX = Input.GetAxisRaw(xAxis) * mouseSensitivity;
            float mouseY = Input.GetAxisRaw(yAxis) * mouseSensitivity * (invertCamera ? 1 : -1);

            yaw += mouseX;
            pitch = Mathf.Clamp(pitch + mouseY, -maxLookAngle, maxLookAngle);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        if (initialInputDelay && !force) return;
    }




    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Check if walking
        isWalking = !isSprinting && (horizontalInput != 0 || verticalInput != 0) && isGrounded;

        // Calculate the current speed based on various states
        float currentSpeed = isSprinting && CanSprint() ? sprintSpeed : walkSpeed;
        if (isCrouched) currentSpeed *= speedReduction;

        if (charController != null)
        {
            // Apply final movement
            float moveDirectionY = moveDirection.y;
            moveDirection = (forward * verticalInput + right * horizontalInput).normalized * currentSpeed;
            moveDirection.y = moveDirectionY;
        }
    }

    private void HandleRigidbodyMovement()
    {
        Vector3 targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

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
               (!useSprintBar || unlimitedSprint || sprintRemaining > 0f) &&
               (Input.GetAxis("Vertical") > 0 || Input.GetAxis("Horizontal") != 0);
    }

    private void HandleZoom()
    {
        if (enableZoom)
        {
            // Toggle zoom or hold to zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom)
            {
                isZoomed = !isZoomed;
                targetFOV = isZoomed ? zoomFOV : defaultFOV;
                targetStepTime = zoomStepTime;
            }

            if (holdToZoom)
            {
                // Holding to zoom
                if (Input.GetKey(zoomKey))
                {
                    isZoomed = true;
                    targetFOV = zoomFOV;
                    targetStepTime = zoomStepTime;
                }
                else
                {
                    isZoomed = false;
                    targetFOV = defaultFOV;
                    targetStepTime = zoomStepTime;
                }
            }

            // Apply FOV change
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, targetStepTime * Time.deltaTime);
        }
    }

    private void HandleSprint()
    {
        // Check for sprint input
        if (Input.GetKeyDown(sprintKey) && !isSprintCooldown)
        {
            isSprinting = !isSprinting;
        }

        // Cancel sprint if no movement input
        if (isSprinting && (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0))
        {
            isSprinting = false;
        }

        if (isSprinting)
        {
            // Apply FOV change when sprinting
            if (!isZoomed)
            {
                targetFOV = sprintFOV;
                targetStepTime = sprintFOVStepTime;
            }

            // Handle unlimited sprint or limited sprint logic
            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                }
            }
        }
        else
        {
            // Reset FOV if not sprinting
            if (targetFOV != defaultFOV && !isZoomed)
            {
                targetFOV = defaultFOV;
                targetStepTime = sprintFOVStepTime;
            }

            // Regenerate sprint meter when not sprinting
            if (!unlimitedSprint)
            {
                sprintRemaining = Mathf.Min(sprintRemaining + Time.deltaTime, sprintDuration);
            }
        }

        // Handle sprint cooldown
        if (isSprintCooldown)
        {
            sprintCooldown -= Time.deltaTime;
            if (sprintCooldown <= 0)
            {
                isSprintCooldown = false;
                sprintCooldown = sprintCooldownReset;
                sprintRemaining = sprintDuration;
            }
        }

        // Update sprint UI
        if (useSprintBar && !unlimitedSprint)
        {
            float sprintRemainingPercent = sprintRemaining / sprintDuration;
            if (sprintBar != null)
            {
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }

            if (hideBarWhenFull && sprintBarCG != null)
            {
                sprintBarCG.alpha = isSprinting || sprintRemaining < sprintDuration ?
                    1 : Mathf.MoveTowards(sprintBarCG.alpha, 0, Time.deltaTime * 2f);
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            if (rb != null)
            {
                // Apply jump force to rigidbody
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
            else if (charController != null)
            {
                // Apply jump force to character controller
                moveDirection.y = jumpPower;
            }

            isGrounded = false;

            // Cancel crouch when jumping if not holding to crouch
            if (isCrouched && !holdToCrouch)
                Crouch();
        }
    }

    private void HandleCrouch()
    {
        if (holdToCrouch)
        {
            if (Input.GetKey(crouchKey) != isCrouched)
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

        // Apply scale change
        transform.localScale = new Vector3(
            originalScale.x,
            isCrouched ? crouchHeight : originalScale.y,
            originalScale.z
        );
    }

    private void HandleHeadbob()
    {
        if (joint == null) return;

        if (isWalking || isSprinting)
        {
            // Calculate headbob movement
            float bobSpeedMultiplier = isSprinting ? 2f :
                                      isCrouched ? speedReduction :
                                      1f;

            bobTimer += Time.deltaTime * bobSpeed * bobSpeedMultiplier;

            // Apply bobbing effect
            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(bobTimer) * (isSprinting ? bobAmount.x * 2 : bobAmount.x),
                jointOriginalPos.y + Mathf.Sin(bobTimer) * (isSprinting ? bobAmount.y * 2 : bobAmount.y),
                jointOriginalPos.z + Mathf.Sin(bobTimer) * (isSprinting ? bobAmount.z * 2 : bobAmount.z)
            );
        }
        else
        {
            // Reset joint position when not moving
            bobTimer = 0;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
        }
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || !footstepAudioSource) return;

        if (isWalking || isSprinting)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                // Play footstep sound
                if (footstepAudioSource.enabled)
                {
                    // Raycast to detect surface type
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, Vector3.down, out hit, 3))
                    {
                        // Determine surface based on tag and play appropriate sound
                        switch (hit.collider.tag)
                        {
                            case "Wood":
                                if (woodClips != null && woodClips.Length > 0)
                                    footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length)]);
                                break;
                            case "Metal":
                                if (metalClips != null && metalClips.Length > 0)
                                    footstepAudioSource.PlayOneShot(metalClips[UnityEngine.Random.Range(0, metalClips.Length)]);
                                break;
                            case "Grass":
                                if (grassClips != null && grassClips.Length > 0)
                                    footstepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length)]);
                                break;
                            default:
                                // Default sound
                                if (woodClips != null && woodClips.Length > 0)
                                    footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length)]);
                                break;
                        }
                    }
                }

                // Reset timer
                footstepTimer = GetCurrentOffset;
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = GetCurrentOffset;
        }
    }

    private void ApplyFinalMovements()
    {
        // Apply gravity
        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Handle slopes
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2f))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (angle > maxSlopeAngle)
            {
                // Too steep, slide down
                moveDirection.x += (1f - slopeHit.normal.y) * slopeHit.normal.x * slopeSlideSpeed;
                moveDirection.z += (1f - slopeHit.normal.y) * slopeHit.normal.z * slopeSlideSpeed;
            }
        }

        // Move the controller
        charController.Move(moveDirection * Time.deltaTime);

        // Update grounded state
        isGrounded = charController.isGrounded;
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
        else if (rb != null) // Only update if we're using Rigidbody
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    private void CreateCrosshair()
    {
        // Create canvas for crosshair
        GameObject canvas = new GameObject("CrosshairCanvas");
        canvas.transform.parent = transform;
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create crosshair
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.parent = canvas.transform;
        crosshairObject = crosshairObj.AddComponent<UnityEngine.UI.Image>();

        // Set crosshair properties
        if (crosshairImage != null)
        {
            crosshairObject.sprite = crosshairImage;
        }
        else
        {
            // Create a basic crosshair
            Texture2D tex = new Texture2D(2, 2);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.SetPixel(0, 1, Color.white);
            tex.SetPixel(1, 1, Color.white);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            crosshairObject.sprite = sprite;
        }

        crosshairObject.color = crosshairColor;

        // Position the crosshair
        RectTransform rect = crosshairObject.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(25, 25);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    private bool IsSlope()
    {
        if (!isGrounded || !Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2f))
            return false;

        float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
        return angle != 0f && angle <= maxSlopeAngle;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded, re-enabling camera movement");
        cameraCanMove = true;

    }

    public void OnValidate()
    {
        Debug.Log($"Sensitivity changed to: {mouseSensitivity}");
    }
}