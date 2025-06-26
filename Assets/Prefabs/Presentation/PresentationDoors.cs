using UnityEngine;

public class PresentationDoors : MonoBehaviour
{
    [Header("Door Animators")]
    [SerializeField] private Animator doorLeftAnimator;
    [SerializeField] private Animator doorRightAnimator;

    private bool hasOpened = false;

    private void Awake()
    {
        // Ensure both Animators start disabled.
        doorLeftAnimator.enabled = false;
        doorRightAnimator.enabled = false;
    }

    private void Update()
    {
        // Either the physical Return/Enter key …
        bool pressedReturnKey = Input.GetKeyDown(KeyCode.Return);

        // … or Unity’s default “Submit” button (mapped to Return/Enter & game-pad A, etc.).
        bool pressedSubmit = Input.GetButtonDown("Submit");

        if (!hasOpened && (pressedReturnKey || pressedSubmit))
        {
            doorLeftAnimator.enabled = true;
            doorRightAnimator.enabled = true;

            hasOpened = true;      // prevents re-triggering
        }
    }
}
