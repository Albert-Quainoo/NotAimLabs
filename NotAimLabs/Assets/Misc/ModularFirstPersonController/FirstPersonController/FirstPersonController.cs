using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;

public class FirstPersonController : MonoBehaviour
{
    #region Camera Movement Variables

    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float maxLookAngle = 80f;

    [SerializeField]
    [Range(0.1f, 20f)]
    public float _mouseSensitivity = 10f;

    public float mouseSensitivity
    {
        get => _mouseSensitivity;
        set
        {
            if (Mathf.Approximately(_mouseSensitivity, value))
            {
                return;
            }

            _mouseSensitivity = value;

            OnSensitivityChanged?.Invoke(_mouseSensitivity);

            PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, _mouseSensitivity);
            PlayerPrefs.Save();

            Debug.Log($"FPC: Sensitivity set to: {_mouseSensitivity}");
        }
    }
    public event System.Action<float> OnSensitivityChanged;

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
    private float slopeSlideSpeed = 10f;
    private RaycastHit slopeHit;

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

    private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";

    private void Awake()
    {
        LoadSensitivtyFromPlayerPrefs();

        if (PlayerPrefs.HasKey(SENSITIVITY_PREF_KEY))
        {
            PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, _mouseSensitivity);
            PlayerPrefs.Save();
        }
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
        if (componentsInitialized) return;

        rb = GetComponent<Rigidbody>();
        charController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                Debug.LogError("Camera component missing from FPS Controller!");
        }

        if (!HasRequiredComponents())
        {
            Debug.LogError("FPC Missing required components (Camera and Rigidbody/CharacterController).");
            return;
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = fov;
        }
        defaultFOV = fov;
        targetFOV = defaultFOV;

        Vector3 rotation = transform.rotation.eulerAngles;
        yaw = rotation.y;

        if (playerCamera != null)
        {
            pitch = playerCamera.transform.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
        }

        originalScale = transform.localScale;
        if (joint != null)
        {
            jointOriginalPos = joint.localPosition;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        sprintRemaining = sprintDuration;
        sprintCooldownReset = sprintCooldown;

        componentsInitialized = true;
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
        if (sensitivityLoaded && Application.isPlaying) return;

        if (PlayerPrefs.HasKey(SENSITIVITY_PREF_KEY))
        {
            float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY);
            _mouseSensitivity = savedSensitivity;
            Debug.Log($"FPC Awake - Loaded Sensitivity: {_mouseSensitivity}");
        }
        else
        {
            Debug.Log($"FPC Awake - No Saved Sensitivity found, using default {_mouseSensitivity}. Saving default.");
            PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, _mouseSensitivity);
            PlayerPrefs.Save();
        }
        sensitivityLoaded = true;
    }

    private void SetupCursor()
    {
        bool lockTheCursor = lockCursor && (countdownTimer == null || !countdownTimer.isGameOver);
        if (lockTheCursor)
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

            if (hideBarWhenFull && sprintBarCG != null)
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

        if (countdownTimer != null && countdownTimer.isGameOver)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }

        if (playerCanMove)
        {
            if (cameraCanMove) HandleMouseLook();
            HandleMovementInput();

          
            if (Pause.isGamePaused || !playerCanMove)
            {
 
            }
            else
            {
              
                if (Input.GetMouseButtonDown(0)) 
                {
                }
              
            }
        


            if (enableZoom) HandleZoom();
            if (enableSprint) HandleSprint();
            if (enableJump) HandleJump();
            if (enableCrouch) HandleCrouch();
            if (enableHeadBob) HandleHeadbob();
            if (enableFootsteps) HandleFootsteps();

            if (charController != null) ApplyFinalMovements();
        }
    }

    private void FixedUpdate()
    {
        if (!componentsInitialized || !playerCanMove || rb == null) return;

        HandleRigidbodyMovement();
        CheckGround();
    }

    public void HandleMouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && lockCursor && Cursor.lockState == CursorLockMode.None)
        {
            if (!(countdownTimer != null && countdownTimer.isGameOver))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (cameraCanMove && (Cursor.lockState == CursorLockMode.Locked || !lockCursor))
        {
            float mouseX = Input.GetAxisRaw(xAxis) * _mouseSensitivity;
            float mouseY = Input.GetAxisRaw(yAxis) * _mouseSensitivity * (invertCamera ? 1f : -1f);

            yaw += mouseX;
            pitch += mouseY;
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        isWalking = !isSprinting && (horizontalInput != 0 || verticalInput != 0) && isGrounded;

        float currentSpeed = isSprinting && CanSprint() ? sprintSpeed : walkSpeed;
        if (isCrouched) currentSpeed *= speedReduction;

        if (charController != null)
        {
            float moveDirectionY = moveDirection.y;
            moveDirection = (forward * verticalInput + right * horizontalInput).normalized * currentSpeed;
            moveDirection.y = moveDirectionY;
        }
    }

    private void HandleRigidbodyMovement()
    {
        Vector3 targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        float currentSpeed = isSprinting && CanSprint() ? sprintSpeed : walkSpeed;
        if (isCrouched) currentSpeed *= speedReduction;

        targetVelocity = transform.TransformDirection(targetVelocity).normalized * currentSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - new Vector3(velocity.x, 0, velocity.z));

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private bool CanSprint()
    {
        return enableSprint &&
               Input.GetKey(sprintKey) &&
               !isSprintCooldown &&
               (unlimitedSprint || sprintRemaining > 0f) &&
               (Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Horizontal") != 0) &&
               !isCrouched;
    }

    private void HandleZoom()
    {
        if (enableZoom)
        {
            if (Input.GetKeyDown(zoomKey) && !holdToZoom)
            {
                isZoomed = !isZoomed;
            }

            if (holdToZoom)
            {
                isZoomed = Input.GetKey(zoomKey);
            }

            targetFOV = isZoomed ? zoomFOV : defaultFOV;
            targetStepTime = zoomStepTime;

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, targetStepTime * Time.deltaTime);
            }
        }
    }

    private void HandleSprint()
    {
        bool sprintInput = Input.GetKey(sprintKey) && CanSprint();

        if (sprintInput && !isSprinting)
        {
            isSprinting = true;
        }
        else if (!sprintInput && isSprinting)
        {
            isSprinting = false;
        }

        if (isSprinting)
        {
            if (!isZoomed && playerCamera != null)
            {
                targetFOV = sprintFOV;
                targetStepTime = sprintFOVStepTime;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, targetStepTime * Time.deltaTime);
            }

            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    sprintRemaining = 0;
                    isSprinting = false;
                    isSprintCooldown = true;
                    sprintCooldown = sprintCooldownReset;
                }
            }
        }
        else
        {
            if (!isZoomed && playerCamera != null && playerCamera.fieldOfView != defaultFOV)
            {
                targetFOV = defaultFOV;
                targetStepTime = sprintFOVStepTime;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, targetStepTime * Time.deltaTime);
            }

            if (isSprintCooldown)
            {
                sprintCooldown -= Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else if (!unlimitedSprint && sprintRemaining < sprintDuration)
            {
                sprintRemaining += Time.deltaTime;
                sprintRemaining = Mathf.Min(sprintRemaining, sprintDuration);
            }
        }

        if (useSprintBar && !unlimitedSprint && sprintBarCG != null)
        {
            float sprintRemainingPercent = sprintDuration > 0 ? sprintRemaining / sprintDuration : 0;
            if (sprintBar != null)
            {
                sprintBar.rectTransform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }

            if (hideBarWhenFull)
            {
                bool shouldShow = isSprinting || isSprintCooldown || sprintRemaining < sprintDuration;
                sprintBarCG.alpha = Mathf.MoveTowards(sprintBarCG.alpha, shouldShow ? 1 : 0, Time.deltaTime * 3f);
            }
            else
            {
                sprintBarCG.alpha = 1;
            }
        }
    }

    private void HandleJump()
    {
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            if (rb != null)
            {
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
            else if (charController != null)
            {
                moveDirection.y = jumpPower;
            }

            isGrounded = false;

            if (isCrouched && !holdToCrouch)
                Crouch();
        }
    }

    private void HandleCrouch()
    {
        if (!enableCrouch) return;

        bool crouchInput = holdToCrouch ? Input.GetKey(crouchKey) : Input.GetKeyDown(crouchKey);
        bool shouldBeCrouched = holdToCrouch ? crouchInput : (crouchInput ? !isCrouched : isCrouched);

        if (shouldBeCrouched != isCrouched)
        {
            if (!shouldBeCrouched && charController != null && Physics.Raycast(transform.position, Vector3.up, charController.height))
            {
                return;
            }
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouched = !isCrouched;
        float targetYScale = isCrouched ? crouchHeight : originalScale.y;

        transform.localScale = new Vector3(originalScale.x, targetYScale, originalScale.z);

        if (charController != null)
        {
            float originalControllerHeight = originalScale.y * charController.height / originalScale.y; 
            float newHeight = isCrouched ? originalControllerHeight * crouchHeight : originalControllerHeight;
            float centerOffsetY = (originalControllerHeight - newHeight) / 2f;
            charController.height = newHeight;
            charController.center = new Vector3(charController.center.x, charController.center.y + (isCrouched ? -centerOffsetY : centerOffsetY), charController.center.z);
        }
    }


    private void HandleHeadbob()
    {
        if (!enableHeadBob || joint == null) return;

        bool isMoving = isWalking || isSprinting;

        if (isMoving && isGrounded)
        {
            float bobSpeedMultiplier = 1f;
            if (isSprinting) bobSpeedMultiplier = 1.8f;
            else if (isCrouched) bobSpeedMultiplier = 0.7f;

            bobTimer += Time.deltaTime * bobSpeed * bobSpeedMultiplier;

            float sinWave = Mathf.Sin(bobTimer);
            float cosWave = Mathf.Cos(bobTimer * 0.5f);

            Vector3 bobOffset = new Vector3(
                cosWave * bobAmount.x * bobSpeedMultiplier,
                sinWave * bobAmount.y * bobSpeedMultiplier,
                0
            );

            joint.localPosition = jointOriginalPos + bobOffset;
        }
        else
        {
            bobTimer = 0;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed * 0.5f);
        }
    }

    private void HandleFootsteps()
    {
        if (!enableFootsteps || !isGrounded || footstepAudioSource == null) return;

        bool isMoving = isWalking || isSprinting;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlayFootstepSound();
                footstepTimer = GetCurrentOffset;
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource.enabled && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            AudioClip[] clipsToUse = woodClips;

            switch (hit.collider.tag)
            {
                case "Wood": clipsToUse = woodClips; break;
                case "Metal": clipsToUse = metalClips; break;
                case "Grass": clipsToUse = grassClips; break;
                default: clipsToUse = woodClips; break;
            }

            if (clipsToUse != null && clipsToUse.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, clipsToUse.Length);
                footstepAudioSource.PlayOneShot(clipsToUse[index]);
            }
        }
    }

    private void ApplyFinalMovements()
    {
        if (charController == null) return; 

        if (!charController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.1f;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, charController.height / 2 * 1.1f))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (angle > charController.slopeLimit) 
            {
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                moveDirection.x += slideDirection.x * slopeSlideSpeed;
                moveDirection.z += slideDirection.z * slopeSlideSpeed;
            }
        }

        charController.Move(moveDirection * Time.deltaTime);
        isGrounded = charController.isGrounded;
    }

    private void CheckGround()
    {
        if (rb != null)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float checkDistance = 0.3f;
            isGrounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            groundNormal = isGrounded ? hit.normal : Vector3.up;
        }
        else if (charController != null)
        {
            isGrounded = charController.isGrounded;
        }
    }

    private void CreateCrosshair()
    {
        if (transform.Find("CrosshairCanvas") != null) return;

        GameObject canvasGO = new GameObject("CrosshairCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject crosshairGO = new GameObject("CrosshairImage");
        crosshairGO.transform.SetParent(canvasGO.transform);

        crosshairObject = crosshairGO.AddComponent<Image>();

        if (crosshairImage != null)
        {
            crosshairObject.sprite = crosshairImage;
        }
        else
        {
            Debug.LogWarning("Crosshair sprite not assigned, creating default pixel.");
            Texture2D tex = new Texture2D(2, 2);
            Color[] colors = new Color[4];
            for (int i = 0; i < colors.Length; ++i) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            crosshairObject.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100.0f);
        }

        crosshairObject.color = crosshairColor;
        crosshairObject.raycastTarget = false;

        RectTransform rect = crosshairObject.rectTransform;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(25, 25);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    private bool IsSlope()
    {
        if (!isGrounded) return false;
        float checkDist = charController != null ? charController.height / 2 * 1.1f : 1.0f;
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, checkDist))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            float limit = charController != null ? charController.slopeLimit : maxSlopeAngle;
            return angle > 0.1f && angle <= limit;
        }
        return false;
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
        if (this == null || !gameObject.activeInHierarchy) return;

        Debug.Log($"FPC: Scene {scene.name} loaded. Re-enabling camera movement.");
        cameraCanMove = true;
        playerCanMove = true;
        sensitivityLoaded = false;
        SetupCursor();
        LoadSensitivtyFromPlayerPrefs();
    }

    public void OnValidate()
    {
        if (playerCamera != null && !Application.isPlaying)
        {
            defaultFOV = fov;
            playerCamera.fieldOfView = fov;
        }

        _mouseSensitivity = Mathf.Clamp(_mouseSensitivity, 0.1f, 20f);
    }
}
