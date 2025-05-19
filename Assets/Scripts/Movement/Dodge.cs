using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;
using System.Collections;   

public class Dodge : MonoBehaviour
{

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

    private Animator _animator;
    private GameObject _mainCamera;
    private CharacterController _controller;
    private CustomThridPersonController _customThridPersonController;
    private bool _hasAnimator;
    // dodge variables
    public bool isDodging = false;
    public float dodgeCooldownTimer = 0f;
    private bool isInvulnerable = false;

    private CustomStarterAssetsInputs _input;

    // VFX references (optional)
    private ParticleSystem dodgeVFX;
    private TrailRenderer dodgeTrail;

    // animation IDs
    private int _animIDJump;
    private int _animIDFreeFall;



    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _hasAnimator = TryGetComponent(out _animator);
        _customThridPersonController = GetComponent<CustomThridPersonController>();
        _input = GetComponent<CustomStarterAssetsInputs>();

    // Initialize animation IDs
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }

    public void HandleDodge()
    {
        // Update cooldown timer
        if (dodgeCooldownTimer > 0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }

        // Check if we can dodge
        if (_input.jump && !isDodging && dodgeCooldownTimer <= 0f && _customThridPersonController.Grounded)
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
            Debug.Log("Animator found, setting dodge animation");
            _animator.SetBool(_animIDJump, true);
            _animator.SetBool(_animIDFreeFall, false);
        }
        else
        {
            Debug.Log("No animator found, skipping dodge animation");
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

}
