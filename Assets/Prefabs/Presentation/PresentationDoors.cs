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
        bool pressedReturnKey = Input.GetKeyDown(KeyCode.Return);


        if (!hasOpened && (pressedReturnKey))
        {
            doorLeftAnimator.enabled = true;
            doorRightAnimator.enabled = true;

            hasOpened = true;      // prevents re-triggering
        }
    }
}
