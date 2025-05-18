using StarterAssets;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class CustomThridPersonController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Header("Dodge Settings")]
    [Tooltip("How far the player will dodge")]
    public float dodgeDistance = 4f;

    [Tooltip("How long the dodge animation/movement lasts")]
    public float dodgeDuration = 0.2f;

    [Tooltip("Cooldown time between dodges")]
    public float dodgeCooldown = 1f;

    [Tooltip("Audio clip played when dodging")]
    public AudioClip DodgeAudioClip;

    [Tooltip("Volume for dodge sound effect")]
    [Range(0, 1)] public float DodgeAudioVolume = 0.7f;

    [Tooltip("Invulnerability frames during dodge (in seconds)")]
    public float dodgeInvulnerabilityTime = 0.15f;

    [Header("Gravity Settings")]
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // dodge variables
    private bool isDodging = false;
    private float dodgeCooldownTimer = 0f;
    private bool isInvulnerable = false;

    // VFX references (optional)
    private ParticleSystem dodgeVFX;
    private TrailRenderer dodgeTrail;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private CustomStarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    // UI reference for dodge cooldown indicator (optional)
    public UnityEngine.UI.Image dodgeCooldownIndicator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }


    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // Try to find VFX components (optional)
        dodgeVFX = GetComponentInChildren<ParticleSystem>();
        dodgeTrail = GetComponentInChildren<TrailRenderer>();
        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<CustomStarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        HandleDodge();
        GroundedCheck();

        // Only run movement logic if not dodging
        if (!isDodging)
        {
            Move();
        }
        else
        {
            // When dodging, make sure the normal move code isn't applying gravity
            // This prevents the coroutine movement from being affected
            _verticalVelocity = 0f;
        }

        // Update UI cooldown indicator if available
        if (dodgeCooldownIndicator != null)
        {
            dodgeCooldownIndicator.fillAmount = 1 - (dodgeCooldownTimer / dodgeCooldown);
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void HandleDodge()
    {
        // Update cooldown timer
        if (dodgeCooldownTimer > 0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }

        // Check if we can dodge
        if (_input.jump && !isDodging && dodgeCooldownTimer <= 0f && Grounded)
        {
            // Set cooldown timer immediately to prevent multiple dodges
            dodgeCooldownTimer = dodgeCooldown;

            // Start the dodge coroutine
            StartCoroutine(PerformDodge());

            // Reset jump input to prevent accidental double dodges
            _input.jump = false;
        }
    }

    private IEnumerator PerformDodge()
    {
        isDodging = true;
        isInvulnerable = true;

        Debug.Log("Dodge started");

        // Use jump animation for dodge start
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDJump, true);
            _animator.SetBool(_animIDFreeFall, false);
        }

        // Play dodge sound effect
        if (DodgeAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(DodgeAudioClip, transform.position, DodgeAudioVolume);
        }

        // Enable VFX if available
        if (dodgeVFX != null)
        {
            dodgeVFX.Play();
        }
        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = true;
        }

        // Determine dodge direction based on input
        Vector3 dodgeDirection;

        // If the player is giving movement input, dodge in that direction
        if (_input.move != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            dodgeDirection = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * inputDirection;
        }
        // Otherwise, dodge backward relative to player facing
        else
        {
            dodgeDirection = -transform.forward;
        }

        // Calculate the dodge vector
        Vector3 dodgeVector = dodgeDirection.normalized * dodgeDistance;

        // Define the height parameters for the dodge arc
        float maxHeight = 0.5f; // Maximum height of the jump during dodge

        Debug.Log("Dodge direction: " + dodgeDirection + ", distance: " + dodgeDistance);

        // Execute the dodge movement over time
        float timeElapsed = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dodgeVector;

        // Halfway through the dodge, switch to free fall animation
        float halfwayPoint = dodgeDuration / 2f;
        bool triggeredFreefall = false;

        // Use quadratic easing for more natural dodge movement
        while (timeElapsed < dodgeDuration)
        {
            // Calculate progress with easing for horizontal movement
            float progress = timeElapsed / dodgeDuration;
            float easedProgress = progress < 0.5f ? 2 * progress * progress : -1 + (4 - 2 * progress) * progress;

            // Calculate horizontal position
            Vector3 horizontalMovement = Vector3.Lerp(startPosition, targetPosition, easedProgress);

            // Calculate vertical position using a parabolic arc
            // Sin curve for first half, then declining for second half
            float verticalOffset;
            if (progress < 0.5f)
            {
                // Rising phase - accelerate upward
                verticalOffset = maxHeight * (progress / 0.5f) * (progress / 0.5f);
            }
            else
            {
                // Falling phase - accelerate downward
                float fallProgress = (progress - 0.5f) / 0.5f;
                verticalOffset = maxHeight * (1 - (fallProgress * fallProgress));
            }

            // Apply vertical offset to the movement
            Vector3 newPosition = horizontalMovement + new Vector3(0, verticalOffset, 0);

            // Calculate the movement delta from current position
            Vector3 movement = newPosition - transform.position;

            // Use controller.Move for proper physics interactions
            _controller.Move(movement);

            // Check if we should transition to free fall animation
            if (!triggeredFreefall && timeElapsed >= halfwayPoint && _hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, true);
                triggeredFreefall = true;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly at the target position to prevent any residual movement
        Vector3 finalMovement = targetPosition - transform.position;
        if (finalMovement.magnitude > 0.01f)
        {
            _controller.Move(finalMovement);
        }

        // Set animation to landing
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDFreeFall, false);
        }

        Debug.Log("Dodge movement completed");

        // End invulnerability frames after specified time (but don't make it negative)
        float remainingInvulnerabilityTime = Mathf.Max(0, dodgeInvulnerabilityTime - dodgeDuration);
        if (remainingInvulnerabilityTime > 0)
        {
            yield return new WaitForSeconds(remainingInvulnerabilityTime);
        }

        isInvulnerable = false;

        // Disable VFX
        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;
        }

        // End dodge state
        isDodging = false;
        Debug.Log("Dodge fully completed");
    }

    // Method that can be called by other scripts to check if player is invulnerable
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    // Method to get current dodge cooldown progress (0-1)
    public float GetDodgeCooldownProgress()
    {
        return 1 - (dodgeCooldownTimer / dodgeCooldown);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}